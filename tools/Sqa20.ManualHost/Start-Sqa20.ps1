<#
.SYNOPSIS
Starts the real MVC application with isolated SQA-20 fixtures at http://localhost:51920/.
.NOTES
Keep this terminal open; Ctrl+C stops this host. Restart to reset fixtures.
All writes and API scans are disabled. Original application data is untouched.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$webRoot = Join-Path $repositoryRoot 'SecureApiQualityPlatform.Web'
$project = Join-Path $PSScriptRoot 'Sqa20.ManualHost.csproj'
$dotnetCommand = (Get-Command dotnet -CommandType Application).Source

$portProbe = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 51920)
try { $portProbe.Start() }
catch { throw 'Port 51920 is in use. Existing processes were left untouched; stop the previous SQA-20 session before restarting.' }
finally { $portProbe.Stop() }

Push-Location -LiteralPath $repositoryRoot
try {
    & $dotnetCommand build $project --configuration Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'SQA-20 host build failed; see output.' }
    & $dotnetCommand (Join-Path $PSScriptRoot 'bin/Release/net8.0/Sqa20.ManualHost.dll') $webRoot
    if ($LASTEXITCODE -ne 0) { throw 'SQA-20 host stopped with an error; see output.' }
}
finally { Pop-Location }
