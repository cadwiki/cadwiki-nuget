param (
    [string]$RootDir
)

#enable for powershell ISE testing
#$RootDir = "$env:Temp\cadwiki.TestPlugin\4.0.222.55" + '"'  # Example with trailing quote
$RootDir = $RootDir -replace '"$', ''

if (-not $RootDir -or -not (Test-Path $RootDir)) {
    #Write-Error "Invalid or missing root directory argument."
    #exit 1
}


$scriptPath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptPath -Parent
$netload = Join-Path -Path $dir -ChildPath "\netload.scr"
if(Test-Path $netload)
{
    Remove-Item $netload
}
New-Item $netload

#create array of all of the assemblies that need to be netloaded
$assemblies = @(
"cadwiki.AC.TestPlugin.dll", 
"cadwiki.AC.dll"
)


$fd = "filedia 0"
$sd = "secureload 0"
Add-Content -Path $netload -Value $fd
Add-Content -Path $netload -Value $sd
$nd = "netload"
foreach ($assembly in $assemblies)
{
    $d = "$nd ""$RootDir\$assembly"""
    Add-Content -Path $netload -Value $d
}
$fd = "filedia 1"
$sd = "secureload 1"
Add-Content -Path $netload -Value $fd
Add-Content -Path $netload -Value $sd