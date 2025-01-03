$scriptPath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptPath -Parent

Get-ChildItem -path $dir -Include 'bin' -Recurse -force | Remove-Item -force -Recurse
Get-ChildItem -path $dir -Include 'obj' -Recurse -force | Remove-Item -force -Recurse
