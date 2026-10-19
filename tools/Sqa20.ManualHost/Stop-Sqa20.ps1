<#
.SYNOPSIS
Stops only this repository's SQA-20 host after verifying process ownership.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$expectedDll = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'bin/Release/net8.0/Sqa20.ManualHost.dll'))
$listeners = @(Get-NetTCPConnection -State Listen -LocalPort 51920 -ErrorAction SilentlyContinue)
if ($listeners.Count -eq 0) { Write-Output 'No SQA-20 listener is running.'; return }
$hostProcessIds = @($listeners | Select-Object -ExpandProperty OwningProcess -Unique)
foreach ($hostProcessId in $hostProcessIds) {
    $hostProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $hostProcessId"
    if ($null -eq $hostProcess -or $hostProcess.Name -ne 'dotnet.exe' -or
        [string]::IsNullOrEmpty($hostProcess.CommandLine) -or
        $hostProcess.CommandLine.IndexOf($expectedDll, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Port 51920 belongs to a different or unverifiable process ($hostProcessId). Nothing was stopped."
    }
}
foreach ($hostProcessId in $hostProcessIds) {
    Stop-Process -Id $hostProcessId
    Write-Output "Stopped this repository's SQA-20 host process $hostProcessId."
}
