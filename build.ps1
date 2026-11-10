param(
    [Parameter(Mandatory=$true)][string]$GamePath,
    [switch]$Install
)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    & dotnet build TypeDemandStars.csproj -c Release "-p:GamePath=$GamePath" --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    if ($Install) {
        $target = Join-Path $GamePath 'Mods/wrt_type_demand_stars'
        New-Item -ItemType Directory -Force -Path $target | Out-Null
        Copy-Item 'out/wrt_type_demand_stars/*' -Destination $target -Recurse -Force
        Write-Host "Installed to $target"
        Write-Host 'Disable the original demand-highlighting mods, enable this fork and restart.'
    }
} finally { Pop-Location }
