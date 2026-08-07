# PengoTarot build & pack script
# Produces dist/PengoTarot/ ready for Workshop upload.
# Auto-generates PengoTarot.json with a source-content hash in the version
# to prevent multiplayer desync from mismatched DLLs.
# You must still manually add PengoTarot.pck.

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# ===== Mod metadata (bump this when releasing a new version) =====
$ModVersion = "v1.4.9"
$ModAuthor = "Pengo"
$ModDescription = "Adds 44 Tarot cards and 40 new enchantments.`nAuthor: Pengo | QQ: 3411737922"

$ProjectRoot = $PSScriptRoot
$DistDir = "$ProjectRoot\dist\PengoTarot"

# Clean
if (Test-Path -LiteralPath $DistDir) { Remove-Item -Recurse -Force -LiteralPath $DistDir }

# ===== Build main DLL for each version =====
$Versions = @("0.107.0", "0.110.0")
# Corresponding DLL directories (without patch version suffix)
$VersionDllRoots = @{
    "0.107.0" = "D:\[Tool] Godot\STS2dll\v0.107"
    "0.110.0" = "D:\[Tool] Godot\STS2dll\v0.110"
}

# ===== Build Loader =====
Write-Host "=== Building Loader ===" -ForegroundColor Cyan
Push-Location -LiteralPath "$ProjectRoot\loader"
dotnet build -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "Loader build failed" }
Pop-Location

# ===== Verify Loader compatibility against NEWEST target =====
# Loader compiles against oldest (v0.107) but must run on newest (v0.110.0).
# This dry-run compile catches direct field/method refs that were renamed.
# e.g. Mod.assembly → Mod.assemblies in v0.110.0 would fail here at BUILD time.
$NewestVer = $Versions[-1]
$NewestDllRoot = $VersionDllRoots[$NewestVer]
Write-Host "=== Verifying Loader vs $NewestVer ===" -ForegroundColor Cyan
Push-Location -LiteralPath "$ProjectRoot\loader"
dotnet build -c $Configuration /p:Sts2DllRoot="$NewestDllRoot"
if ($LASTEXITCODE -ne 0) {
    throw "Loader incompatible with v$NewestVer! Fix direct references to version-dependent API (use reflection only)."
}
Pop-Location
Write-Host "Loader OK against all targets." -ForegroundColor Green

# ===== Build main DLL for each version =====
foreach ($ver in $Versions) {
    Write-Host "=== Building PengoTarot for $ver ===" -ForegroundColor Cyan
    Push-Location -LiteralPath $ProjectRoot
    dotnet build -c $Configuration /p:Sts2ApiCompat=$ver
    if ($LASTEXITCODE -ne 0) { throw "Build failed for $ver" }
    Pop-Location
}

# ===== Generate build timestamp suffix (for multiplayer version check) =====
$buildSuffix = (Get-Date).ToUniversalTime().ToString("MMddHH")
$fullVersion = "$ModVersion-b$buildSuffix"
Write-Host "  Version: $fullVersion" -ForegroundColor Green

# ===== Create dist structure =====
Write-Host "=== Packaging ===" -ForegroundColor Cyan

# Root files
New-Item -ItemType Directory -Force -Path $DistDir | Out-Null

$LoaderDll = "$ProjectRoot\loader\bin\$Configuration\net9.0\PengoTarot.Loader.dll"
# Rename to PengoTarot.dll so game finds it via mod_manifest.json id
Copy-Item -LiteralPath $LoaderDll -Destination "$DistDir\PengoTarot.dll"

# Variant DLLs
foreach ($ver in $Versions) {
    $libDir = "$DistDir\lib\$ver"
    New-Item -ItemType Directory -Force -Path $libDir | Out-Null

    # DLL (OutputPath includes version subdirectory per csproj)
    $srcDll = "$ProjectRoot\.godot\mono\temp\bin\$Configuration\$ver\PengoTarot.dll"
    Copy-Item -LiteralPath $srcDll -Destination $libDir
}

# ===== Generate mod manifest JSON =====
Write-Host "  Generating PengoTarot.json" -ForegroundColor Cyan

$manifest = @{
    id = "PengoTarot"
    name = "PengoTarot"
    author = $ModAuthor
    description = $ModDescription
    version = $fullVersion
    min_game_version = "0.107.0"
    has_pck = $true
    has_dll = $true
    dependencies = @()
    affects_gameplay = $true
}
$json = $manifest | ConvertTo-Json -Depth 3
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText("$DistDir\PengoTarot.json", $json, $utf8NoBom)

Write-Host "=== Done ===" -ForegroundColor Green
Write-Host "Output: $DistDir"
Write-Host ""
Write-Host "Files:" -ForegroundColor Yellow
Get-ChildItem -Recurse -File -LiteralPath $DistDir | ForEach-Object {
    $rel = $_.FullName.Replace($DistDir, "").TrimStart("\")
    Write-Host "  $rel"
}

Write-Host "Perfectly Finished!" -ForegroundColor Yellow