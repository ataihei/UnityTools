$ProjectDir = [string]$env:ProjectDir
$ProjectDir = $ProjectDir.TrimStart('"').TrimEnd('"')

$TargetDir = [string]$env:TargetDir
$TargetDir = $TargetDir.TrimStart('"').TrimEnd('"')

$settingPath = $ProjectDir + '\CopySettings.json'
$settings = Get-Content $settingPath -Encoding UTF8 -Raw  | ConvertFrom-Json

$dllPath = [IO.Path]::Combine($ProjectDir, $settings.destination)
[string[]] $excludes = $settings.excludes

$pattern = $settings.pattern
if ($pattern -eq $null) { $pattern = '*' }

. ./copy_dlls.ps1 $TargetDir $dllPath $pattern $excludes
