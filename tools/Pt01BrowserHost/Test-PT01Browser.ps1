param([Parameter(Mandatory)][string]$EvidenceDirectory)

$ErrorActionPreference = 'Stop'
$instance = Get-Content -LiteralPath (Join-Path $EvidenceDirectory 'instance.json') -Raw | ConvertFrom-Json
if ($instance.FixtureSha256 -ne 'CCBE6723FB603221C3CECB95BB176BD6426E8F7611B96D3AC0565A1D7927AC18' -or $instance.ResultCount -ne 2000) {
    throw 'Unexpected PT-01 fixture identity.'
}
if (-not ([Uri]$instance.Url).IsLoopback) { throw 'Only the isolated loopback instance may be checked.' }
$handler = [Net.Http.HttpClientHandler]::new()
$handler.AllowAutoRedirect = $false
$handler.UseProxy = $false
$client = [Net.Http.HttpClient]::new($handler)
$client.BaseAddress = [Uri]$instance.Url
$client.Timeout = [TimeSpan]::FromSeconds(15)
$requests = @()
function Read-FixtureResponse([string]$Path) {
    $response = $client.GetAsync($Path).GetAwaiter().GetResult()
    try {
        if ([int]$response.StatusCode -ne 200) { throw "$Path returned HTTP $([int]$response.StatusCode); expected 200 without redirects." }
        # Static files run before the fixture header middleware.
        if ($Path -ne '/css/site.css' -and ($response.Headers.GetValues('X-PT01-Fixture-SHA256') -join '') -ne $instance.FixtureSha256) {
            throw 'Response belongs to a different host/fixture.'
        }
        $script:requests += [pscustomobject]@{ Path = $Path; Status = [int]$response.StatusCode; ContentType = $response.Content.Headers.ContentType.MediaType }
        return $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    }
    finally { $response.Dispose() }
}
try {
    $html = Read-FixtureResponse '/'
    foreach ($expected in @(
        'API Quality Dashboard', 'Executed checks</span><strong>2000</strong>',
        'Passed / Failed</span><strong>1000 / 1000</strong>', 'Open defects</span><strong>48</strong>',
        'High/Critical open</span><strong>24</strong>', 'PT-01 check 1999', 'PT-01 defect 0079'
    )) {
        if (-not $html.Contains($expected)) { throw "Missing dashboard content: $expected" }
    }
    if ([regex]::Matches($html, 'PT-01 check \d{4}').Count -ne 12 -or [regex]::Matches($html, 'PT-01 defect \d{4}').Count -ne 8) {
        throw 'Unexpected recent-list counts.'
    }
    $css = Read-FixtureResponse '/css/site.css'
    if (-not $css.Contains('.metric-card')) { throw 'Application CSS content missing.' }
    $resultCsv = Read-FixtureResponse '/Reports/ExportResultsCsv'
    $defectCsv = Read-FixtureResponse '/Reports/ExportDefectsCsv'
    $results = @($resultCsv | ConvertFrom-Csv)
    $defects = @($defectCsv | ConvertFrom-Csv)
    $passed = @($results | Where-Object Passed -eq 'True').Count
    $failed = @($results | Where-Object Passed -eq 'False').Count
    $open = @($defects | Where-Object IsOpen -eq 'True').Count
    if ($results.Count -ne 2000 -or $passed -ne 1000 -or $failed -ne 1000 -or $defects.Count -ne 80 -or $open -ne 48) {
        throw 'Actual exports do not match the fixture counts.'
    }
    $post = $client.PostAsync('/', [Net.Http.StringContent]::new('')).GetAwaiter().GetResult()
    try { if ([int]$post.StatusCode -ne 405) { throw 'Synthetic fixture must reject POST.' } }
    finally { $post.Dispose() }
    if ((Read-FixtureResponse '/Reports/ExportResultsCsv') -cne $resultCsv -or (Read-FixtureResponse '/Reports/ExportDefectsCsv') -cne $defectCsv) {
        throw 'Readiness requests changed fixture records.'
    }
    $html | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'dashboard-http.html') -Encoding utf8
    [ordered]@{
        CheckedAtUtc = [DateTimeOffset]::UtcNow.ToString('O'); SetupStatus = 'PASS'
        Url = $instance.Url; FixtureSha256 = $instance.FixtureSha256
        ResultCount = $results.Count; Passed = $passed; Failed = $failed; DefectCount = $defects.Count; OpenDefects = $open
        Requests = $requests; PostStatus = 405; ExportsUnchangedAfterRequests = $true
        ServerState = 'MVC/Razor and aggregation warmed by HTTP readiness requests'
        BrowserStatus = 'NOT RUN'; BrowserTimings = $null
        CDN = 'Not fetched by this smoke check; verify Bootstrap in Chrome Network'
    } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'readiness.json') -Encoding utf8
}
finally { $client.Dispose() }
