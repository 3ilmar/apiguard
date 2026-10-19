<#
.SYNOPSIS
Starts the isolated SQA-19 development dashboard at http://localhost:51919/.
.NOTES
Keep this terminal open; Ctrl+C stops only this host. Restart for fresh fixtures.
No changes to the normal application's data, no API requests, no commit or push.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$webRoot = Join-Path $repositoryRoot 'SecureApiQualityPlatform.Web'
$project = Join-Path $PSScriptRoot 'Sqa19.ManualHost.csproj'
$dotnetCommand = (Get-Command dotnet -CommandType Application).Source

# Fail early if another process already owns the session port; never stop it.
$portProbe = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 51919)
try { $portProbe.Start() }
catch { throw 'Port 51919 is in use. Stop the previous SQA-19 host or choose a different time; existing processes were left untouched.' }
finally { $portProbe.Stop() }

Push-Location -LiteralPath $repositoryRoot
try {
    & $dotnetCommand build $project --configuration Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'SQA-19 host build failed; see the build output.' }
    & $dotnetCommand (Join-Path $PSScriptRoot 'bin/Release/net8.0/Sqa19.ManualHost.dll') $webRoot
    if ($LASTEXITCODE -ne 0) { throw 'SQA-19 host stopped with an error; see the output.' }
}
finally { Pop-Location }
