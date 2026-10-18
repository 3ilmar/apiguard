$ErrorActionPreference = 'Stop'
$qaRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$qaWebRoot = Join-Path $qaRepoRoot 'SecureApiQualityPlatform.Web'
$qaOutput = Join-Path $PSScriptRoot 'app-stdout.txt'
$qaError = Join-Path $PSScriptRoot 'app-stderr.txt'
$qaProcess = Start-Process -FilePath 'dotnet' -ArgumentList @(
    'bin/Release/net8.0/SecureApiQualityPlatform.Web.dll', '--urls', 'http://127.0.0.1:0'
) -WorkingDirectory $qaWebRoot -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput $qaOutput -RedirectStandardError $qaError
$qaClient = New-Object System.Net.Http.HttpClient
$qaChecks = [System.Collections.Generic.List[object]]::new()
try {
    $qaBaseUrl = $null
    for ($qaAttempt = 0; $qaAttempt -lt 100; $qaAttempt++) {
        if ($qaProcess.HasExited) { throw 'Application exited before listening.' }
        $qaStartupLog = Get-Content -LiteralPath $qaOutput -Raw -ErrorAction SilentlyContinue
        if ($qaStartupLog -match 'Now listening on: (http://127\.0\.0\.1:\d+)') {
            $qaBaseUrl = $Matches[1]
            break
        }
        Start-Sleep -Milliseconds 100
    }
    if (-not $qaBaseUrl) { throw 'Application did not start within 10 seconds.' }

    function Test-QaResponse([string]$Path, [int]$ExpectedStatus, [string[]]$ExpectedText) {
        $qaResponse = $qaClient.GetAsync($qaBaseUrl + $Path).GetAwaiter().GetResult()
        try {
            $qaBody = $qaResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            if ([int]$qaResponse.StatusCode -ne $ExpectedStatus) {
                throw "$Path returned $([int]$qaResponse.StatusCode); expected $ExpectedStatus."
            }
            foreach ($qaText in $ExpectedText) {
                if (-not $qaBody.Contains($qaText)) { throw "$Path did not contain: $qaText" }
            }
            $qaChecks.Add([ordered]@{
                Path = $Path; Status = [int]$qaResponse.StatusCode; Outcome = 'PASS'
                AssertedText = $ExpectedText; ContentType = [string]$qaResponse.Content.Headers.ContentType
            })
            return $qaBody
        }
        finally { $qaResponse.Dispose() }
    }

    $qaDashboard = Test-QaResponse '/' 200 @(
        '<h2 class="card-header h6 mb-0" id="severity-title">',
        'role="group" aria-label="Quality metrics"',
        '<span>Pass rate</span><strong>N/A</strong>',
        '<span>Avg response</span><strong>N/A</strong>',
        'No open defects to classify.', 'No checks have been run yet.', 'No defects recorded.'
    )
    $qaDashboard | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'empty-dashboard.html')
    $null = Test-QaResponse '/Reports/ExportResultsCsv?passed=garbage' 400 @('passed as true or false', 'Omit a filter')
    $null = Test-QaResponse '/Reports/ExportResultsCsv?apiId=bad' 400 @('apiId as a GUID')
    $null = Test-QaResponse '/Reports/ExportDefectsCsv?status=garbage' 400 @('status as one of New, Triaged')
    $null = Test-QaResponse '/Reports/ExportDefectsCsv?severity=999' 400 @('severity as one of Low, Medium, High, Critical')
    $null = Test-QaResponse '/Reports/ExportDefectsCsv?apiId=bad' 400 @('apiId as a GUID')
    $qaResultsCsv = Test-QaResponse '/Reports/ExportResultsCsv?passed=false' 200 @('ExecutedAtUtc,ApiName,ApiId', 'ApiBaseUrl,Outcome')
    $qaDefectsCsv = Test-QaResponse '/Reports/ExportDefectsCsv?status=New&severity=Critical' 200 @('DefectId,ApiName,ApiId', 'ApiBaseUrl,LifecycleStage,IsOpen')
    if (($qaResultsCsv.Trim() -split '\r?\n').Count -ne 1 -or ($qaDefectsCsv.Trim() -split '\r?\n').Count -ne 1) {
        throw 'An empty export contained more than its header.'
    }
    [ordered]@{
        ExecutedAtNZ = [TimeZoneInfo]::ConvertTime([DateTimeOffset]::UtcNow,
            [TimeZoneInfo]::FindSystemTimeZoneById('New Zealand Standard Time')).ToString('o')
        BaseUrl = $qaBaseUrl; Data = 'Fresh isolated process; no APIs, results or defects'
        Scope = 'Real application HTTP/MVC/Razor smoke; no browser, CSS layout or screen-reader verification'
        Checks = $qaChecks
    } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'http-smoke.json')
    Write-Output "HTTP smoke passed: $($qaChecks.Count) requests, including Razor no-data output and MVC filter binding."
}
finally {
    $qaClient.Dispose()
    if (-not $qaProcess.HasExited) { Stop-Process -Id $qaProcess.Id -Force }
    $qaProcess.Dispose()
}
