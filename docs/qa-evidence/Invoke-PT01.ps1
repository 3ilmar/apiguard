<#
.SYNOPSIS
Runs the Release suite and the isolated PT-01 checks, retaining actual evidence.
.EXAMPLE
pwsh -NoProfile -File docs/qa-evidence/Invoke-PT01.ps1
.EXAMPLE
pwsh -NoProfile -File docs/qa-evidence/Invoke-PT01.ps1 -SkipCoverage
.NOTES
The tests create their own stores and loopback application. This script never
seeds a running application or reads/writes manually recorded application data.
Coverage is requested unless -SkipCoverage is supplied. Use the separate
performance run for uninstrumented PT-01 measurements. No commit or push is performed.
#>
[CmdletBinding()]
param([switch]$SkipCoverage)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (Get-Variable PSNativeCommandUseErrorActionPreference -ErrorAction SilentlyContinue) {
    $PSNativeCommandUseErrorActionPreference = $false
}

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$startedUtc = [DateTimeOffset]::UtcNow
try { $aucklandZone = [TimeZoneInfo]::FindSystemTimeZoneById('Pacific/Auckland') }
catch { $aucklandZone = [TimeZoneInfo]::FindSystemTimeZoneById('New Zealand Standard Time') }
$startedAuckland = [TimeZoneInfo]::ConvertTime($startedUtc, $aucklandZone)
$runName = 'pt01-' + $startedUtc.ToString('yyyyMMddTHHmmssfffZ') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 6)
$dateDirectory = Join-Path $PSScriptRoot $startedAuckland.ToString('yyyy-MM-dd')
$outputDirectory = Join-Path $dateDirectory $runName
if (Test-Path -LiteralPath $outputDirectory) { throw "Evidence directory already exists: $outputDirectory" }
# Capture source state before the evidence files themselves dirty a clean tree.
$branch = (& git -C $repositoryRoot branch --show-current).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Unable to identify the repository branch.' }
$head = (& git -C $repositoryRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Unable to identify the repository commit.' }
$workingTree = @(& git -C $repositoryRoot status --porcelain=v1 --untracked-files=all)
if ($LASTEXITCODE -ne 0) { throw 'Unable to capture the repository working tree.' }
$null = New-Item -ItemType Directory -Path $outputDirectory
$run = [ordered]@{
    Case = 'PT-01'
    StartedAtUtc = $startedUtc.ToString('O')
    StartedAtAuckland = $startedAuckland.ToString('O')
    RepositoryRoot = $repositoryRoot
    EvidenceDirectory = $outputDirectory
    Configuration = 'Release'
    CoverageRequested = -not $SkipCoverage.IsPresent
    ThresholdMilliseconds = 2000
    DatasetResultCount = 2000
    Status = 'Running'
    Commands = @()
}

function Save-RunStatus {
    $run | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $outputDirectory 'execution-status.json') -Encoding utf8
}

function Invoke-EvidenceCommand {
    param([string]$Name, [string]$Executable, [string[]]$Arguments)
    $commandStarted = [DateTimeOffset]::UtcNow
    $timer = [Diagnostics.Stopwatch]::StartNew()
    $logPath = Join-Path $outputDirectory ($Name + '.txt')
    Write-Host "Executing: $Executable $($Arguments -join ' ')"
    $commandExitCode = -1
    try {
        # Continue allows native stderr to be retained by Tee-Object on Windows
        # PowerShell as well. The actual native exit code determines the outcome.
        $previousPreference = $ErrorActionPreference
        $null = Get-Command -Name $Executable -CommandType Application -ErrorAction Stop
        $ErrorActionPreference = 'Continue'
        & $Executable @Arguments 2>&1 | Tee-Object -FilePath $logPath | Out-Host
        $commandExitCode = $LASTEXITCODE
    }
    catch {
        $_ | Out-String | Add-Content -LiteralPath $logPath
    }
    finally {
        $ErrorActionPreference = $previousPreference
        $timer.Stop()
        $run.Commands += [ordered]@{
            Name = $Name
            Executable = $Executable
            Arguments = $Arguments
            StartedAtUtc = $commandStarted.ToString('O')
            CompletedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
            DurationMilliseconds = $timer.Elapsed.TotalMilliseconds
            ExitCode = $commandExitCode
            Log = [IO.Path]::GetFileName($logPath)
        }
        Save-RunStatus
    }
    return $commandExitCode
}

function Copy-TestAttachments {
    param([string]$ResultsDirectory, [string]$RunLabel)
    if (-not (Test-Path -LiteralPath $ResultsDirectory)) { return }
    $attachmentDirectory = Join-Path $outputDirectory ('attachments/' + $RunLabel)
    foreach ($file in Get-ChildItem -LiteralPath $ResultsDirectory -File -Recurse) {
        if ($file.Extension -notin @('.json', '.html', '.txt', '.png') -and $file.Name -ne 'coverage.cobertura.xml') { continue }
        $relativePath = $file.FullName.Substring($ResultsDirectory.Length).TrimStart([char[]]'\/')
        # The repository ignores coverage*.xml; preserve a reviewable copy under
        # a filename that is not ignored, with its original directory structure.
        if ($file.Name -eq 'coverage.cobertura.xml') {
            $relativePath = Join-Path ([IO.Path]::GetDirectoryName($relativePath)) 'cobertura.xml'
        }
        $destination = Join-Path $attachmentDirectory $relativePath
        $null = New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($destination)) -Force
        Copy-Item -LiteralPath $file.FullName -Destination $destination
    }
}

Save-RunStatus
$overallExitCode = 1
Push-Location -LiteralPath $repositoryRoot
try {
    $workingTree | Set-Content -LiteralPath (Join-Path $outputDirectory 'git-status.txt') -Encoding utf8
    $run.Branch = $branch
    $run.Head = $head
    $run.WorkingTreeDirty = $workingTree.Count -gt 0
    Save-RunStatus
    if ($branch -ne 'feature/dashboard-reporting') { throw "Expected feature/dashboard-reporting; found $branch. No tests were run." }

    $environmentEvidence = [ordered]@{
        CapturedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
        CapturedAtAuckland = [TimeZoneInfo]::ConvertTime([DateTimeOffset]::UtcNow, $aucklandZone).ToString('O')
        OS = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        OSArchitecture = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
        ProcessArchitecture = [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
        LogicalProcessorCount = [Environment]::ProcessorCount
        PowerShellVersion = $PSVersionTable.PSVersion.ToString()
        CPU = $null
        PhysicalMemoryBytes = $null
        Branch = $branch
        Head = $head
        WorkingTreeDirty = $workingTree.Count -gt 0
        Configuration = 'Release'
        CI = 'Local execution; no CI run URL is generated by this script.'
    }
    if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) {
        try {
            $environmentEvidence.CPU = @((Get-CimInstance Win32_Processor).Name)
            $environmentEvidence.PhysicalMemoryBytes = [long](Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory
        }
        catch { $environmentEvidence.HardwareQueryError = $_.Exception.Message }
    }
    elseif (Test-Path -LiteralPath '/proc/cpuinfo') {
        $environmentEvidence.CPU = @(Get-Content -LiteralPath '/proc/cpuinfo' | Where-Object { $_ -match '^model name\s*:' } | Select-Object -Unique)
        $memoryLine = Get-Content -LiteralPath '/proc/meminfo' | Where-Object { $_ -match '^MemTotal:' }
        if ($memoryLine -match '(\d+)\s+kB') { $environmentEvidence.PhysicalMemoryBytes = [long]$Matches[1] * 1024 }
    }
    $environmentEvidence | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $outputDirectory 'environment.json') -Encoding utf8

    $sourceFiles = @(
        Get-ChildItem -LiteralPath (Join-Path $repositoryRoot 'SecureApiQualityPlatform.Web'), (Join-Path $repositoryRoot 'SecureApiQualityPlatform.Tests'), (Join-Path $repositoryRoot '.github/workflows') -File -Recurse |
            Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.Extension -in @('.cs', '.csproj', '.cshtml', '.css', '.json', '.yml', '.yaml', '.runsettings') }
        Get-Item -LiteralPath $PSCommandPath, (Join-Path $repositoryRoot 'SecureApiQualityPlatform.sln')
    ) | Sort-Object -Property FullName -Unique
    $sourceFiles | ForEach-Object {
        [pscustomobject]@{
            Path = $_.FullName.Substring($repositoryRoot.Length + 1).Replace('\', '/')
            SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
            Bytes = $_.Length
        }
    } | Export-Csv -LiteralPath (Join-Path $outputDirectory 'tested-source-sha256.csv') -NoTypeInformation -Encoding utf8

    $null = Invoke-EvidenceCommand 'git-version' 'git' @('--version')
    $null = Invoke-EvidenceCommand 'git-diff' 'git' @('diff', '--no-ext-diff')
    $null = Invoke-EvidenceCommand 'git-staged-diff' 'git' @('diff', '--cached', '--no-ext-diff')
    $infoExitCode = Invoke-EvidenceCommand 'dotnet-info' 'dotnet' @('--info')
    if ($infoExitCode -ne 0) { throw 'dotnet --info failed; restore, build and tests were not run.' }
    $null = Invoke-EvidenceCommand 'dotnet-sdk-version' 'dotnet' @('--version')
    $null = Invoke-EvidenceCommand 'dotnet-sdks' 'dotnet' @('--list-sdks')
    $null = Invoke-EvidenceCommand 'dotnet-runtimes' 'dotnet' @('--list-runtimes')

    $restoreExitCode = Invoke-EvidenceCommand 'restore' 'dotnet' @('restore', 'SecureApiQualityPlatform.sln')
    if ($restoreExitCode -ne 0) { throw 'Restore failed; build and tests were not run. See restore.txt.' }
    $null = Invoke-EvidenceCommand 'test-package-versions' 'dotnet' @('list', 'SecureApiQualityPlatform.Tests/SecureApiQualityPlatform.Tests.csproj', 'package', '--include-transitive')
    $buildExitCode = Invoke-EvidenceCommand 'build-release' 'dotnet' @('build', 'SecureApiQualityPlatform.sln', '--configuration', 'Release', '--no-restore')
    if ($buildExitCode -ne 0) { throw 'Build failed; tests were not run. See build-release.txt.' }

    $fullSuiteDirectory = Join-Path $outputDirectory 'full-suite'
    $performanceDirectory = Join-Path $outputDirectory 'performance'
    $suiteSettings = Join-Path $outputDirectory 'full-suite.runsettings'
    $performanceSettings = Join-Path $outputDirectory 'performance.runsettings'
    $performanceEvidenceDirectory = Join-Path $performanceDirectory 'pt01'
    foreach ($settings in @(
        @{ Path = $suiteSettings; EvidenceDirectory = (Join-Path $fullSuiteDirectory 'pt01') },
        @{ Path = $performanceSettings; EvidenceDirectory = $performanceEvidenceDirectory }
    )) {
        # XmlDocument escapes the absolute path safely for an XML attribute.
        [xml]$document = '<RunSettings><TestRunParameters><Parameter name="PT01EvidenceDirectory" value="" /></TestRunParameters></RunSettings>'
        $document.RunSettings.TestRunParameters.Parameter.SetAttribute('value', $settings.EvidenceDirectory)
        $document.Save($settings.Path)
        $null = New-Item -ItemType Directory -Path $settings.EvidenceDirectory
    }
    $testProject = 'SecureApiQualityPlatform.Tests/SecureApiQualityPlatform.Tests.csproj'
    $suiteArguments = @('test', $testProject, '--configuration', 'Release', '--no-build', '--no-restore', '--settings', $suiteSettings, '--logger', 'trx;LogFileName=full-suite.trx', '--logger', 'console;verbosity=detailed', '--results-directory', $fullSuiteDirectory)
    if (-not $SkipCoverage) { $suiteArguments += @('--collect', 'XPlat Code Coverage') }
    $suiteExitCode = Invoke-EvidenceCommand 'test-full-suite' 'dotnet' $suiteArguments
    Copy-TestAttachments $fullSuiteDirectory 'full-suite'
    $performanceExitCode = Invoke-EvidenceCommand 'test-performance' 'dotnet' @('test', $testProject, '--configuration', 'Release', '--no-build', '--no-restore', '--settings', $performanceSettings, '--filter', 'TestCategory=Performance', '--logger', 'trx;LogFileName=performance.trx', '--logger', 'console;verbosity=detailed', '--results-directory', $performanceDirectory)
    Copy-TestAttachments $performanceDirectory 'performance'

    $trxSummaries = @(foreach ($trxFile in Get-ChildItem -LiteralPath $outputDirectory -Filter '*.trx' -Recurse -File) {
        [xml]$trx = Get-Content -LiteralPath $trxFile.FullName -Raw
        [pscustomobject]@{
            File = $trxFile.FullName.Substring($outputDirectory.Length + 1)
            Outcome = [string]$trx.TestRun.ResultSummary.outcome
            Counters = @($trx.TestRun.ResultSummary.Counters.Attributes | ForEach-Object { [pscustomobject]@{ Name = $_.Name; Value = $_.Value } })
            Results = @($trx.TestRun.Results.UnitTestResult | ForEach-Object { [pscustomobject]@{ Name = $_.testName; Outcome = $_.outcome; Duration = $_.duration } })
        }
    })
    $trxSummaries | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $outputDirectory 'trx-summary.json') -Encoding utf8
    $performanceSummary = @($trxSummaries | Where-Object { $_.File -like 'performance*' })
    $expectedTests = @('PT01_BuildWithTwoThousandResults_UnderTwoSeconds', 'PT01_DashboardHttpWithTwoThousandResults_UnderTwoSeconds')
    foreach ($expectedTest in $expectedTests) {
        if (@($performanceSummary.Results | Where-Object { $_.Name -eq $expectedTest }).Count -ne 1) {
            throw "Expected PT-01 test was not executed exactly once in the dedicated run: $expectedTest. Inspect performance.trx."
        }
    }
    $measurementDirectory = Join-Path $outputDirectory 'measurements'
    $null = New-Item -ItemType Directory -Path $measurementDirectory
    foreach ($name in @('pt01-aggregation.json', 'pt01-http.json', 'pt01-dashboard.html')) {
        $matchingFiles = @(Get-ChildItem -LiteralPath $performanceEvidenceDirectory -Filter $name -File)
        if ($matchingFiles.Count -ne 1) {
            throw "Expected exactly one dedicated performance artifact named $name; found $($matchingFiles.Count). PT-01 evidence is incomplete."
        }
        Copy-Item -LiteralPath $matchingFiles[0].FullName -Destination (Join-Path $measurementDirectory $name)
    }
    $aggregation = Get-Content -LiteralPath (Join-Path $measurementDirectory 'pt01-aggregation.json') -Raw | ConvertFrom-Json
    $http = Get-Content -LiteralPath (Join-Path $measurementDirectory 'pt01-http.json') -Raw | ConvertFrom-Json
    if ($aggregation.Status -ne 'PASS' -or $http.Status -ne 'PASS') {
        throw 'Dedicated performance JSON does not report PASS for both aggregation and HTTP. Inspect measurements/pt01-*.json.'
    }
    if ($aggregation.FixtureSha256 -notmatch '^[A-Fa-f0-9]{64}$' -or $aggregation.FixtureSha256 -ne $http.FixtureSha256) {
        throw 'Dedicated performance JSON has missing or different fixture SHA256 values; the aggregation and HTTP fixtures are not verified as identical.'
    }
    $run.MeasurementEvidence = @('measurements/pt01-aggregation.json', 'measurements/pt01-http.json', 'measurements/pt01-dashboard.html')
    $run.FixtureSha256 = $aggregation.FixtureSha256
    if ($suiteExitCode -eq 0 -and $performanceExitCode -eq 0) {
        $run.Status = 'CommandsCompletedSuccessfully'
        $overallExitCode = 0
    }
    else { $run.Status = 'TestCommandFailed' }
}
catch {
    $run.Status = 'StoppedWithError'
    $run.Error = $_.Exception.Message
    $_ | Out-String | Set-Content -LiteralPath (Join-Path $outputDirectory 'runner-error.txt') -Encoding utf8
    Write-Warning $_.Exception.Message
}
finally {
    $run.CompletedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    $run.ExitCode = $overallExitCode
    Save-RunStatus
    Pop-Location
    Write-Host "Evidence saved to: $outputDirectory"
    Write-Host "Runner exit code: $overallExitCode. Read the TRX outcomes and PT-01 JSON attachments for observed test results."
}
exit $overallExitCode
