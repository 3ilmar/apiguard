<#
.SYNOPSIS
Verifies the running SQA-20 fixture host using local HTTP requests only.
.DESCRIPTION
This is automated environment preparation. It does not open a browser, evaluate
accessibility or compatibility, create screenshots, or change SQA-20's historical
automated FAIL or unperformed human acceptance. Start-Sqa20.ps1 must already be
running in another terminal. A real demo Admin cookie permits read-only inspection
of the registration form; requests without that cookie keep the Viewer default.
.PARAMETER OutputDirectory
Optional directory for automated CSV samples and setup-verification.json.
These samples are HTTP-client responses, not evidence of browser downloads.
#>
[CmdletBinding()]
param([string]$OutputDirectory)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
Add-Type -AssemblyName Microsoft.VisualBasic

$baseUrl = 'http://localhost:51920/'
$failedApiId = '20200000-0000-0000-0000-000000000001'
$healthyApiId = '20200000-0000-0000-0000-000000000002'
$startedAt = [DateTimeOffset]::UtcNow
# Matches CookieRoleProvider.CookieName; this is a demo role, not authentication.
$demoRoleCookieName = 'apiguard_role'

function Assert-Setup([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "SQA-20 setup verification failed: $Message" }
}

function Get-LocalResponse([string]$Path, [string]$Method = 'GET', [string]$DemoRole = '') {
    $uri = [Uri]::new([Uri]$baseUrl, $Path.TrimStart('/'))
    $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($Method), $uri)
    $response = $null
    try {
        if (-not [string]::IsNullOrWhiteSpace($DemoRole)) {
            [void]$request.Headers.TryAddWithoutValidation('Cookie', "$demoRoleCookieName=$DemoRole")
        }
        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        return [PSCustomObject]@{
            Path = $Path
            Method = $Method
            Status = [int]$response.StatusCode
            ContentType = [string]$response.Content.Headers.ContentType
            ContentDisposition = [string]$response.Content.Headers.ContentDisposition
            Allow = [string]($response.Content.Headers.Allow -join ', ')
            Location = [string]$response.Headers.Location
            DemoRole = if ($DemoRole) { $DemoRole } else { 'No cookie; application default Viewer' }
            Body = $body
        }
    }
    finally {
        if ($null -ne $response) { $response.Dispose() }
        $request.Dispose()
    }
}

function Read-CheckedCsv([string]$Body, [string[]]$ExpectedHeaders) {
    # TextFieldParser handles quoted commas, doubled quotes, and quoted newlines.
    # Check every physical CSV record's field count before creating row objects;
    # ConvertFrom-Csv alone can otherwise silently discard surplus fields.
    $reader = [IO.StringReader]::new($Body)
    $parser = [Microsoft.VisualBasic.FileIO.TextFieldParser]::new($reader)
    $parser.TextFieldType = [Microsoft.VisualBasic.FileIO.FieldType]::Delimited
    $parser.SetDelimiters(',')
    $parser.HasFieldsEnclosedInQuotes = $true
    $parser.TrimWhiteSpace = $false
    $rows = [Collections.Generic.List[object]]::new()
    try {
        Assert-Setup (-not $parser.EndOfData) 'CSV response was empty.'
        $headers = $parser.ReadFields()
        Assert-Setup ($headers.Count -eq $ExpectedHeaders.Count) 'CSV header column count differs from the current report contract.'
        for ($index = 0; $index -lt $ExpectedHeaders.Count; $index++) {
            Assert-Setup ($headers[$index] -ceq $ExpectedHeaders[$index]) "Unexpected CSV column at index $index."
        }
        while (-not $parser.EndOfData) {
            $fields = $parser.ReadFields()
            Assert-Setup ($fields.Count -eq $headers.Count) "CSV data record $($rows.Count + 1) has $($fields.Count) columns; expected $($headers.Count)."
            $record = [ordered]@{}
            for ($index = 0; $index -lt $headers.Count; $index++) { $record[$headers[$index]] = $fields[$index] }
            $rows.Add([PSCustomObject]$record)
        }
        return [PSCustomObject]@{ Headers = $headers; Rows = $rows.ToArray() }
    }
    finally { $parser.Dispose(); $reader.Dispose() }
}

function Assert-CsvResponse($Response, [string]$FilePrefix) {
    Assert-Setup ($Response.Status -eq 200) "$($Response.Path) did not return HTTP 200."
    Assert-Setup ($Response.ContentType -match '^text/csv;\s*charset=utf-8$') "$($Response.Path) did not return UTF-8 text/csv."
    Assert-Setup ($Response.ContentDisposition -match '^attachment;' -and $Response.ContentDisposition -match ($FilePrefix + '-\d{8}-\d{6}\.csv')) "$($Response.Path) did not return the expected attachment filename."
}

function Assert-RecentTimestamp([string]$Value, [string]$Label) {
    $timestamp = [DateTimeOffset]::Parse($Value, [Globalization.CultureInfo]::InvariantCulture)
    Assert-Setup ($timestamp -ge $startedAt.AddHours(-24) -and $timestamp -le [DateTimeOffset]::UtcNow.AddMinutes(1)) "$Label was not within the last 24 hours. Restart the fixture host to reset data."
}

$handler = [System.Net.Http.HttpClientHandler]::new()
$handler.UseProxy = $false
$handler.AllowAutoRedirect = $false
$handler.UseCookies = $false # Each role cookie below is explicit; no hidden cookie state.
$client = [System.Net.Http.HttpClient]::new($handler)
$client.Timeout = [TimeSpan]::FromSeconds(20)
try {
    $pageFacts = [Collections.Generic.List[object]]::new()
    $pages = @{}
    $viewerRegistration = Get-LocalResponse '/Apis/Create'
    Assert-Setup ($viewerRegistration.Status -eq 302 -and $viewerRegistration.Location -match '^/Apis(?:/Index)?(?:\?.*)?$') 'Registration without a role cookie should retain the default Viewer redirect to APIs.'
    foreach ($path in @('/', '/Apis', '/Apis/Create', "/Apis/Details/$failedApiId", "/Apis/Details/$healthyApiId", '/Defects', '/css/site.css')) {
        $demoRole = if ($path -eq '/Apis/Create') { 'Admin' } else { '' }
        $response = Get-LocalResponse $path -DemoRole $demoRole
        Assert-Setup ($response.Status -eq 200) "GET $path returned HTTP $($response.Status); expected 200 with no redirect."
        $pages[$path] = [Net.WebUtility]::HtmlDecode($response.Body)
        $pageFacts.Add([PSCustomObject]@{ Path = $path; HttpStatus = $response.Status; DemoRole = $response.DemoRole; Verified = $true })
    }
    Assert-Setup ($pages['/'] -match 'Open defect severity distribution' -and $pages['/'] -match 'Recent checks' -and $pages['/'] -match 'Recent defects') 'Dashboard fixture sections were missing.'
    Assert-Setup ($pages['/'] -match '<span>Registered APIs</span>\s*<strong>2</strong>' -and $pages['/'] -match '<span>Configured endpoints</span>\s*<strong>2</strong>') 'Dashboard did not report exactly two registered APIs and two configured endpoints.'
    Assert-Setup ($pages['/Apis'].Contains('SQA-20 Orders, "review"') -and $pages['/Apis'].Contains('SQA-20 Catalogue, "healthy"')) 'Both synthetic API names were not displayed.'
    Assert-Setup ($pages["/Apis/Details/$failedApiId"].Contains('/v1/orders/accessibility-and-browser-compatibility-review') -and $pages["/Apis/Details/$healthyApiId"].Contains('/v1/catalogue')) 'Both configured fixture endpoints were not displayed.'

    $resultHeaders = @('ExecutedAtUtc', 'ApiName', 'ApiId', 'EndpointName', 'EndpointId', 'Method', 'Path', 'CheckType', 'Passed', 'ActualStatusCode', 'ResponseTimeMs', 'FailureSeverity', 'Message', 'ApiBaseUrl', 'Outcome')
    $defectHeaders = @('DefectId', 'ApiName', 'ApiId', 'EndpointName', 'EndpointId', 'Method', 'Path', 'CheckType', 'Severity', 'Priority', 'Status', 'AssignedTo', 'CreatedAtUtc', 'UpdatedAtUtc', 'RetestedAtUtc', 'SourceCheckResultId', 'Title', 'Description', 'ApiBaseUrl', 'LifecycleStage', 'IsOpen')
    $resultsResponse = Get-LocalResponse '/Reports/ExportResultsCsv'
    $defectsResponse = Get-LocalResponse '/Reports/ExportDefectsCsv'
    Assert-CsvResponse $resultsResponse 'api-check-results'
    Assert-CsvResponse $defectsResponse 'defects'
    $results = Read-CheckedCsv $resultsResponse.Body $resultHeaders
    $defects = Read-CheckedCsv $defectsResponse.Body $defectHeaders
    Assert-Setup ($results.Rows.Count -eq 8) 'Expected exactly eight synthetic check results.'
    Assert-Setup (@($results.Rows | Where-Object { $_.Passed -ceq 'True' -and $_.Outcome -ceq 'Pass' }).Count -eq 4) 'Expected four passing check results.'
    Assert-Setup (@($results.Rows | Where-Object { $_.Passed -ceq 'False' -and $_.Outcome -ceq 'Fail' }).Count -eq 4) 'Expected four failing check results.'
    $apiIds = @($results.Rows | Select-Object -ExpandProperty ApiId -Unique)
    $endpointIds = @($results.Rows | Select-Object -ExpandProperty EndpointId -Unique)
    Assert-Setup ($apiIds.Count -eq 2 -and $apiIds -contains $failedApiId -and $apiIds -contains $healthyApiId) 'Expected results belonging to precisely the two fixture APIs.'
    Assert-Setup ($endpointIds.Count -eq 2) 'Expected precisely two configured endpoint IDs in results.'
    Assert-Setup ($defects.Rows.Count -eq 4 -and @($defects.Rows | Where-Object { $_.IsOpen -ceq 'True' }).Count -eq 4) 'Expected four open defects.'
    $severityStatus = [ordered]@{ Critical = 'New'; High = 'Assigned'; Medium = 'InProgress'; Low = 'Retest' }
    foreach ($severity in $severityStatus.Keys) {
        $matching = @($defects.Rows | Where-Object { $_.Severity -ceq $severity })
        Assert-Setup ($matching.Count -eq 1 -and $matching[0].Status -ceq $severityStatus[$severity]) "Expected one $severity defect with status $($severityStatus[$severity])."
        $expectedStage = if ($severity -eq 'Low') { 'Verification' } else { 'Active' }
        Assert-Setup ($matching[0].LifecycleStage -ceq $expectedStage) "Unexpected $severity defect lifecycle stage."
        Assert-Setup ($pages['/'].Contains("aria-label=`"$severity open defects`" aria-valuenow=`"1`"")) "Dashboard did not expose the seeded $severity count in rendered markup."
    }
    foreach ($row in $results.Rows) { Assert-RecentTimestamp $row.ExecutedAtUtc 'Check execution timestamp' }
    foreach ($row in $defects.Rows) {
        Assert-RecentTimestamp $row.CreatedAtUtc 'Defect creation timestamp'
        Assert-RecentTimestamp $row.UpdatedAtUtc 'Defect update timestamp'
    }
    $ordersEndpointName = 'Orders, "manual review" endpoint with a deliberately long descriptive label'
    $syntheticDisclosure = 'Server header disclosed: SQA-20 fixture, "synthetic" server.'
    $failedRows = @($results.Rows | Where-Object { $_.ApiId -ceq $failedApiId })
    Assert-Setup ($failedRows.Count -eq 4 -and @($failedRows | Where-Object { $_.ApiName -ceq 'SQA-20 Orders, "review"' -and $_.EndpointName -ceq $ordersEndpointName }).Count -eq 4) 'Results CSV did not preserve quoted commas and quotation marks in fixture API/endpoint names.'
    Assert-Setup (@($results.Rows | Where-Object { $_.Message -ceq $syntheticDisclosure }).Count -eq 1) 'Results CSV did not preserve the comma/quote fixture message.'
    Assert-Setup (@($defects.Rows | Where-Object { $_.ApiName -ceq 'SQA-20 Orders, "review"' -and $_.EndpointName -ceq $ordersEndpointName }).Count -eq 4) 'Defects CSV did not preserve quoted fixture API/endpoint names.'
    Assert-Setup (@($defects.Rows | Where-Object { $_.Description -ceq $syntheticDisclosure }).Count -eq 1) 'Defects CSV did not preserve the comma/quote fixture description.'

    $filteredResultsResponse = Get-LocalResponse "/Reports/ExportResultsCsv?apiId=$failedApiId&passed=false"
    $filteredDefectsResponse = Get-LocalResponse '/Reports/ExportDefectsCsv?status=Assigned&severity=High'
    Assert-CsvResponse $filteredResultsResponse 'api-check-results'
    Assert-CsvResponse $filteredDefectsResponse 'defects'
    $filteredResults = Read-CheckedCsv $filteredResultsResponse.Body $resultHeaders
    $filteredDefects = Read-CheckedCsv $filteredDefectsResponse.Body $defectHeaders
    Assert-Setup ($filteredResults.Rows.Count -eq 4 -and @($filteredResults.Rows | Where-Object { $_.ApiId -cne $failedApiId -or $_.Passed -cne 'False' }).Count -eq 0) 'Failed/API-filtered results export did not contain the expected four rows.'
    Assert-Setup ($filteredDefects.Rows.Count -eq 1 -and $filteredDefects.Rows[0].Status -ceq 'Assigned' -and $filteredDefects.Rows[0].Severity -ceq 'High') 'Assigned/High-filtered defects export did not contain the expected one row.'

    $guardFacts = [Collections.Generic.List[object]]::new()
    $firstDefectId = $defects.Rows[0].DefectId
    $guardRequests = @(
        @{ Method = 'POST'; Path = '/Apis/Create' },
        @{ Method = 'POST'; Path = '/Role/SetRole' },
        @{ Method = 'POST'; Path = "/Apis/AddEndpoint/$failedApiId" },
        @{ Method = 'POST'; Path = "/Apis/RunChecks/$failedApiId" },
        @{ Method = 'POST'; Path = "/Defects/Update/$firstDefectId" },
        @{ Method = 'PUT'; Path = '/' },
        @{ Method = 'GET'; Path = "/Apis/RunChecks/$failedApiId" }
    )
    foreach ($guardRequest in $guardRequests) {
        $response = Get-LocalResponse $guardRequest.Path $guardRequest.Method
        Assert-Setup ($response.Status -eq 405) "$($guardRequest.Method) $($guardRequest.Path) returned $($response.Status); expected 405."
        if ($guardRequest.Method -ne 'GET') {
            Assert-Setup ($response.Body.Contains('writes and API scans are disabled')) 'Rejected write did not return the fixture-host guard message.'
        }
        $guardFacts.Add([PSCustomObject]@{ Method = $guardRequest.Method; Path = $guardRequest.Path; HttpStatus = $response.Status; Verified = $true })
    }
    $resultsAfter = Get-LocalResponse '/Reports/ExportResultsCsv'
    $defectsAfter = Get-LocalResponse '/Reports/ExportDefectsCsv'
    Assert-CsvResponse $resultsAfter 'api-check-results'
    Assert-CsvResponse $defectsAfter 'defects'
    Assert-Setup ($resultsAfter.Body -ceq $resultsResponse.Body -and $defectsAfter.Body -ceq $defectsResponse.Body) 'CSV records changed after rejected writes.'

    $facts = [PSCustomObject][ordered]@{
        Verification = 'Automated local HTTP setup checks only; no browser/manual execution'
        verified = $true
        StartedAtUtc = $startedAt.ToString('O')
        CompletedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
        BaseUrl = $baseUrl
        HttpClient = 'System.Net.Http.HttpClient; UseProxy=false; AllowAutoRedirect=false'
        DemoRoleSetup = [PSCustomObject]@{ CookieName = $demoRoleCookieName; RegistrationInspectionRole = 'Admin'; CookieScope = $baseUrl; OtherRequests = 'No role cookie; application default Viewer'; NoCookieRegistrationStatus = $viewerRegistration.Status; NoCookieRegistrationLocation = $viewerRegistration.Location; ApplicationRoleFiltersPreserved = $true }
        Pages = $pageFacts.ToArray()
        Fixtures = [PSCustomObject]@{ RegisteredApis = 2; ConfiguredEndpoints = 2; Results = 8; PassingResults = 4; FailingResults = 4; OpenDefects = 4; SeverityStatuses = $severityStatus; RecordsWithinLastHours = 24; SyntheticData = $true }
        Csv = [PSCustomObject]@{ ResultsColumns = $results.Headers.Count; DefectsColumns = $defects.Headers.Count; QuotedFieldsPreserved = $true; DownloadHeadersVerified = $true; FilteredFailedApiResults = $filteredResults.Rows.Count; FilteredAssignedHighDefects = $filteredDefects.Rows.Count; UnchangedAfterRejectedWrites = $true }
        RejectedRequests = $guardFacts.ToArray()
        HumanAcceptance = 'NOT PERFORMED'
        BrowserExecution = 'NOT PERFORMED BY THIS SCRIPT; automated HTTP setup only'
        HistoricalAutomatedSqa20 = '19 October execution overall FAIL; retained evidence is not rerun or reassessed by this script'
        HumanObservations = 'None recorded by this script. HTTP setup does not establish human acceptance or new browser/accessibility outcomes.'
    }
    $json = $facts | ConvertTo-Json -Depth 8
    if (-not [string]::IsNullOrWhiteSpace($OutputDirectory)) {
        $outputPath = if ([IO.Path]::IsPathRooted($OutputDirectory)) { [IO.Path]::GetFullPath($OutputDirectory) } else { [IO.Path]::GetFullPath((Join-Path (Get-Location).Path $OutputDirectory)) }
        [IO.Directory]::CreateDirectory($outputPath) | Out-Null
        $utf8 = [Text.UTF8Encoding]::new($false)
        [IO.File]::WriteAllText((Join-Path $outputPath 'automated-results.csv'), $resultsResponse.Body, $utf8)
        [IO.File]::WriteAllText((Join-Path $outputPath 'automated-defects.csv'), $defectsResponse.Body, $utf8)
        [IO.File]::WriteAllText((Join-Path $outputPath 'setup-verification.json'), $json + [Environment]::NewLine, $utf8)
    }
    Write-Output $json
}
finally { $client.Dispose(); $handler.Dispose() }
