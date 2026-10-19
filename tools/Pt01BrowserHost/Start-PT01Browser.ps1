param(
    [ValidateRange(1024, 65535)][int]$Port = 51818,
    [string]$EvidenceDirectory
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$branch = (& git -C $repositoryRoot branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -notin @('feature/dashboard-reporting', 'qa/sqa19-sqa20-manual-validation')) {
    throw "Expected feature/dashboard-reporting or qa/sqa19-sqa20-manual-validation; found $branch. No server was started."
}
if ([string]::IsNullOrWhiteSpace($EvidenceDirectory)) {
    $aucklandZone = [TimeZoneInfo]::FindSystemTimeZoneById('New Zealand Standard Time')
    $day = [TimeZoneInfo]::ConvertTime([DateTimeOffset]::UtcNow, $aucklandZone).ToString('yyyy-MM-dd')
    $stamp = [DateTimeOffset]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
    $EvidenceDirectory = Join-Path $repositoryRoot "docs/qa-evidence/$day/sqa18-browser-$stamp"
}
$EvidenceDirectory = [IO.Path]::GetFullPath($EvidenceDirectory)
if (Test-Path -LiteralPath $EvidenceDirectory) {
    if (@(Get-ChildItem -LiteralPath $EvidenceDirectory -Force).Count -gt 0) {
        throw 'Use a new or empty evidence directory; existing evidence will not be overwritten.'
    }
}
$null = New-Item -ItemType Directory -Path $EvidenceDirectory -Force
$project = Join-Path $PSScriptRoot 'Pt01BrowserHost.csproj'
$webRoot = Join-Path $repositoryRoot 'SecureApiQualityPlatform.Web'
$dotnet = (Get-Command dotnet -CommandType Application).Source
& $dotnet build $project --configuration Release *> (Join-Path $EvidenceDirectory 'host-build.txt')
if ($LASTEXITCODE -ne 0) { throw "Host build failed. See $EvidenceDirectory/host-build.txt" }
$dll = Join-Path $PSScriptRoot 'bin/Release/net8.0/Pt01BrowserHost.dll'

# Only the newly created child inherits these temporary environment values.
$environmentNames = @('DOTNET_ENVIRONMENT', 'ASPNETCORE_ENVIRONMENT', 'ASPNETCORE_HTTPS_PORT', 'ASPNETCORE_ANCM_HTTPS_PORT')
$originalEnvironment = @{}
foreach ($name in $environmentNames) { $originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
try {
    [Environment]::SetEnvironmentVariable('DOTNET_ENVIRONMENT', 'Development', 'Process')
    [Environment]::SetEnvironmentVariable('ASPNETCORE_ENVIRONMENT', 'Development', 'Process')
    [Environment]::SetEnvironmentVariable('ASPNETCORE_HTTPS_PORT', $null, 'Process')
    [Environment]::SetEnvironmentVariable('ASPNETCORE_ANCM_HTTPS_PORT', $null, 'Process')
    $server = Start-Process -FilePath $dotnet -WorkingDirectory $webRoot -WindowStyle Hidden -PassThru `
        -ArgumentList @("`"$dll`"", '--port', "$Port", '--evidence-directory', "`"$EvidenceDirectory`"", '--content-root', "`"$webRoot`"") `
        -RedirectStandardOutput (Join-Path $EvidenceDirectory 'server-stdout.txt') `
        -RedirectStandardError (Join-Path $EvidenceDirectory 'server-stderr.txt')
}
finally {
    foreach ($name in $environmentNames) { [Environment]::SetEnvironmentVariable($name, $originalEnvironment[$name], 'Process') }
}
$head = (& git -C $repositoryRoot rev-parse HEAD).Trim()
$sourcePaths = @(
    Get-ChildItem -LiteralPath $webRoot, (Join-Path $repositoryRoot 'SecureApiQualityPlatform.Tests'), $PSScriptRoot -Recurse -File |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.Extension -in @('.cs', '.csproj', '.cshtml', '.css', '.json', '.ps1') }
)
$sourcePaths | Sort-Object FullName -Unique | ForEach-Object {
    [pscustomobject]@{ Path = $_.FullName.Substring($repositoryRoot.Length + 1).Replace('\', '/'); SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
} | Export-Csv -LiteralPath (Join-Path $EvidenceDirectory 'source-sha256.csv') -NoTypeInformation -Encoding utf8
[ordered]@{
    CapturedAtUtc = [DateTimeOffset]::UtcNow.ToString('O'); Branch = $branch; Head = $head
    WorkingTree = @(& git -C $repositoryRoot status --short); ProcessId = $server.Id
    Url = "http://127.0.0.1:$Port/"; SDK = (& $dotnet --version).Trim()
    OS = [Runtime.InteropServices.RuntimeInformation]::OSDescription
    PowerShellVersion = $PSVersionTable.PSVersion.ToString(); BrowserStatus = 'NOT RUN'
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'launch.json') -Encoding utf8

$metadata = Join-Path $EvidenceDirectory 'instance.json'
for ($attempt = 0; $attempt -lt 100; $attempt++) {
    $server.Refresh()
    if ($server.HasExited) { throw "Host exited with $($server.ExitCode). See server-stderr.txt and server-stdout.txt in $EvidenceDirectory" }
    if (Test-Path -LiteralPath $metadata) {
        & (Join-Path $PSScriptRoot 'Test-PT01Browser.ps1') -EvidenceDirectory $EvidenceDirectory
        Write-Host "Dashboard running: http://127.0.0.1:$Port/"
        Write-Host "PID: $($server.Id) (kept running after this script exits)"
        Write-Host "Evidence directory: $EvidenceDirectory"
        Write-Host 'Browser performance: NOT RUN. Follow docs/qa-evidence/SQA18-Browser-Performance.md.'
        return
    }
    Start-Sleep -Milliseconds 100
}
throw "Host did not become ready within 10 seconds. Inspect logs and PID $($server.Id); this script has not stopped any process."
