function WriteHeader {
    param([string]$header)

    WriteMessage
    WriteMessage " === $header ===" Yellow
    WriteMessage
}

function WriteMessage ($message, $color) {
    if (!$color) {
        Write-Host $message
    } else {
        Write-Host $message -ForegroundColor $color
    }
}

$nuget = $path + "nuget.exe"
$dotnet = $path + "dotnet.exe"

WriteHeader "Build"
&$nuget restore
&$dotnet build .\Tollrech\Tollrech.csproj -c Release

WriteHeader "Make nuget package"
&$nuget pack "package.nuspec" -OutputDirectory "Tolltech.Tollrech.Rider"

$source = (Get-Item -Path ".\Tolltech.Tollrech.Rider" -Verbose).FullName
$destination = Join-Path $source "..\Tolltech.Tollrech.Rider.zip"

If (Test-path $destination) {
    Remove-item $destination
}

Add-Type -assembly "system.io.compression.filesystem"

WriteHeader "Zipping"
[io.compression.zipfile]::CreateFromDirectory($Source, $destination, 1, $true)

Write-Host "Done!" -ForegroundColor Green