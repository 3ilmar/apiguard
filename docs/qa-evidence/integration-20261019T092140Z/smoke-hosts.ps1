# Reproducible integration preparation checks. No browser acceptance is inferred.
# Run with PowerShell 7 from the repository root after building all three hosts.
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$webRoot = Join-Path $repositoryRoot 'SecureApiQualityPlatform.Web'
$dotnetPath = (Get-Command dotnet -CommandType Application).Source
$runDirectory = Join-Path $PSScriptRoot 'host-smoke'
if (Test-Path -LiteralPath $runDirectory) { throw 'Use fresh smoke output; do not overwrite previous evidence.' }
New-Item -ItemType Directory -Path $runDirectory | Out-Null
$sentinel = Join-Path $runDirectory 'not-a-database-directory'
[IO.File]::WriteAllText($sentinel, 'This file must remain unchanged; no fixture may open a SQLite database below it.')
$sentinelHash = (Get-FileHash -LiteralPath $sentinel -Algorithm SHA256).Hash
$processes = [Collections.Generic.List[Diagnostics.Process]]::new()
$checks = [Collections.Generic.List[object]]::new()
$handler = [Net.Http.HttpClientHandler]::new()
$handler.UseProxy = $false
$client = [Net.Http.HttpClient]::new($handler)
$client.Timeout = [TimeSpan]::FromSeconds(2)

function Start-Fixture([string]$Name, [string[]]$Arguments, [string]$Url) {
    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $dotnetPath
    $startInfo.WorkingDirectory = $repositoryRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.Environment['DOTNET_ENVIRONMENT'] = 'Development'
    $startInfo.Environment['ASPNETCORE_ENVIRONMENT'] = 'Development'
    $startInfo.Environment['ConnectionStrings__PlatformDatabase'] = 'Data Source=' + (Join-Path $sentinel 'must-not-open.db')
    foreach ($argument in $Arguments) { $startInfo.ArgumentList.Add($argument) }
    $fixtureProcess = [Diagnostics.Process]::Start($startInfo)
    $processes.Add($fixtureProcess)
    $outputTask = $fixtureProcess.StandardOutput.ReadToEndAsync()
    $errorTask = $fixtureProcess.StandardError.ReadToEndAsync()
    try {
        $ready = $false
        for ($attempt = 0; $attempt -lt 30; $attempt++) {
            if ($fixtureProcess.HasExited) { throw "$Name stopped before readiness." }
            try {
                $response = $client.GetAsync($Url).GetAwaiter().GetResult()
                try { $ready = [int]$response.StatusCode -eq 200 }
                finally { $response.Dispose() }
            }
            catch { $ready = $false }
            if ($ready) { break }
            Start-Sleep -Milliseconds 200
        }
        if (-not $ready) { throw "$Name failed loopback readiness." }
        if ($Name -eq 'Sqa19.ManualHost') {
            & (Join-Path $repositoryRoot 'tools/Sqa19.ManualHost/Verify-Sqa19.ps1') |
                Set-Content -LiteralPath (Join-Path $runDirectory 'sqa19-http.json') -Encoding utf8
        }
        elseif ($Name -eq 'Sqa20.ManualHost') {
            & (Join-Path $repositoryRoot 'tools/Sqa20.ManualHost/Verify-Sqa20.ps1') -OutputDirectory (Join-Path $runDirectory 'sqa20') | Out-Null
        }
        else {
            & (Join-Path $repositoryRoot 'tools/Pt01BrowserHost/Test-PT01Browser.ps1') -EvidenceDirectory (Join-Path $runDirectory 'pt01')
        }
        $checks.Add([ordered]@{Host=$Name;Url=$Url;PreparationStatus='PASS';ConfiguredDatabase='Unusable sentinel path; fixture must use private memory';BrowserOrHumanAcceptance='Not assessed by this smoke check'})
    }
    finally {
        if (-not $fixtureProcess.HasExited) { $fixtureProcess.Kill(); $fixtureProcess.WaitForExit() }
        [IO.File]::WriteAllText((Join-Path $runDirectory ($Name + '-stdout.txt')), $outputTask.GetAwaiter().GetResult())
        [IO.File]::WriteAllText((Join-Path $runDirectory ($Name + '-stderr.txt')), $errorTask.GetAwaiter().GetResult())
    }
}

try {
    foreach ($port in @(51919, 51920, 51881)) {
        $probe = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, $port)
        try { $probe.Start() }
        finally { $probe.Stop() }
    }
    Start-Fixture 'Sqa19.ManualHost' @((Join-Path $repositoryRoot 'tools/Sqa19.ManualHost/bin/Release/net8.0/Sqa19.ManualHost.dll'), $webRoot) 'http://localhost:51919/'
    Start-Fixture 'Sqa20.ManualHost' @((Join-Path $repositoryRoot 'tools/Sqa20.ManualHost/bin/Release/net8.0/Sqa20.ManualHost.dll'), $webRoot) 'http://localhost:51920/'
    Start-Fixture 'Pt01BrowserHost' @((Join-Path $repositoryRoot 'tools/Pt01BrowserHost/bin/Release/net8.0/Pt01BrowserHost.dll'), '--port', '51881', '--evidence-directory', (Join-Path $runDirectory 'pt01'), '--content-root', $webRoot) 'http://127.0.0.1:51881/'
    if ((Get-FileHash -LiteralPath $sentinel -Algorithm SHA256).Hash -ne $sentinelHash) { throw 'Database sentinel changed.' }
    $databaseFiles = @(Get-ChildItem -LiteralPath $repositoryRoot,$webRoot -File | Where-Object { $_.Name -match '\.db($|-)|\.sqlite($|-)' })
    if ($databaseFiles.Count -ne 0) { throw 'Unexpected normal application database created by fixture smoke.' }
    [ordered]@{CapturedAtUtc=[DateTimeOffset]::UtcNow.ToString('O');PreparationStatus='PASS';Checks=$checks.ToArray();SentinelUnchanged=$true;NormalDatabaseCreated=$false;AllSmokeServersStopped=$true} |
        ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $runDirectory 'summary.json') -Encoding utf8
    Get-Content -LiteralPath (Join-Path $runDirectory 'summary.json')
}
finally {
    foreach ($fixtureProcess in $processes) {
        if (-not $fixtureProcess.HasExited) { $fixtureProcess.Kill(); $fixtureProcess.WaitForExit() }
        $fixtureProcess.Dispose()
    }
    $client.Dispose()
    $handler.Dispose()
}
