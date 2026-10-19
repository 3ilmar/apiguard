<#
.SYNOPSIS
Checks SQA-19 preparation via loopback HTTP; does not assess human usability.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$handler = [Net.Http.HttpClientHandler]::new()
$handler.UseProxy = $false
$handler.AllowAutoRedirect = $false
$client = [Net.Http.HttpClient]::new($handler)
$client.Timeout = [TimeSpan]::FromSeconds(10)
$checks = [Collections.Generic.List[object]]::new()

function Read-Response {
    param([string]$Method, [string]$Path, [int]$ExpectedStatus)
    $request = [Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::new($Method), ('http://localhost:51919' + $Path))
    try {
        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        try {
            $status = [int]$response.StatusCode
            if ($status -ne $ExpectedStatus) { throw "$Method $Path expected $ExpectedStatus; received $status." }
            $checks.Add([ordered]@{Method=$Method;Path=$Path;Status=$status;PreparationCheck='PASS'})
            return $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        }
        finally { $response.Dispose() }
    }
    finally { $request.Dispose() }
}

try {
    $html = Read-Response GET / 200
    $section = [regex]::Match($html, '<section[^>]*aria-labelledby="recent-checks-title"[^>]*>(.*?)</section>', 'Singleline')
    if (-not $section.Success) { throw 'Original Recent checks section was not rendered.' }
    $messages = @([regex]::Matches($section.Groups[1].Value, '<small class="text-muted">(.*?)</small>', 'Singleline') | ForEach-Object {
        [Net.WebUtility]::HtmlDecode($_.Groups[1].Value)
    })
    $expected = @(
        'Expected HTTP 200 but received 400.',
        'Response took 1001 ms, exceeding the 1000 ms threshold.',
        'Endpoint is marked authentication-required but unauthenticated request returned HTTP 200.',
        "Expected content type containing 'application/json' but received 'text/html'.",
        'Request failed: Request timed out.'
    )
    if ($messages.Count -ne 5) { throw "Expected five messages; received $($messages.Count)." }
    for ($index = 0; $index -lt 5; $index++) {
        if ($messages[$index] -cne $expected[$index]) { throw "Scenario $($index + 1) message/order mismatch." }
    }
    if ([regex]::Matches($section.Value, '>FAIL</span>').Count -ne 5) { throw 'Expected five FAIL badges.' }

    $null = Read-Response HEAD / 200
    $null = Read-Response GET /Home/Index 200
    $null = Read-Response GET /css/site.css 200
    $null = Read-Response POST /Apis/RunChecks 405
    $null = Read-Response POST / 405
    $null = Read-Response GET /Apis/RunChecks 404
    $null = Read-Response GET /Apis/Create 404
    $after = Read-Response GET / 200
    # The merged shared layout includes an antiforgery token in its role form.
    # Ignore only that per-render token when comparing the immutable fixture HTML.
    $tokenPattern = '(<input\b[^>]*name="__RequestVerificationToken"[^>]*value=")[^"]*(")'
    $beforeStable = [regex]::Replace($html, $tokenPattern, '$1$2')
    $afterStable = [regex]::Replace($after, $tokenPattern, '$1$2')
    if ($afterStable -cne $beforeStable) { throw 'Dashboard changed during the read-only host verification.' }

    [ordered]@{
        CapturedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
        URL = 'http://localhost:51919/'
        PreparationStatus = 'PASS'
        HumanUsabilityStatus = 'Not evaluated by this preparation check'
        Data = 'Synthetic evaluator results; no external API requests made'
        RecentCheckMessagesInOrder = $messages
        FailedRowCount = 5
        DashboardUnchangedAfterBlockedActions = $true
        HTTPChecks = $checks.ToArray()
    } | ConvertTo-Json -Depth 6
}
finally { $client.Dispose(); $handler.Dispose() }
