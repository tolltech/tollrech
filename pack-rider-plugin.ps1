Add-Type -AssemblyName System.IO.Compression.FileSystem

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

del Tolltech.Tollrider.Rider.zip
del Tolltech.Tollrider.Rider\*.nupkg

&$nuget pack "package.nuspec" -OutputDirectory "Tolltech.Tollrider.Rider"

get-childitem -Path "./Tolltech.Tollrider.Rider" | where-object { $_.Name -like "Tolltech.Tollrider.*.nupkg" } | %{ rename-item -LiteralPath $_.FullName -NewName "Tolltech.Tollrider.nupkg" }

$PluginXml = [xml] (Get-Content "./Tolltech.Tollrider.Rider/META-INF/plugin.xml")
$Version = $PluginXml.SelectSingleNode(".//idea-plugin/version").innerText

Write-Host "Version is $Version"
[System.IO.Compression.ZipFile]::CreateFromDirectory("./Tolltech.Tollrider.Rider/META-INF", "Tolltech.Tollrider-$Version.jar", [System.IO.Compression.CompressionLevel]::Optimal, $True)

$source = (Get-Item -Path ".\Tolltech.Tollrider.Rider" -Verbose).FullName
$destination = Join-Path $source "..\Tolltech.Tollrider.Rider.zip"

If (Test-path $destination) {
    Remove-item $destination
}

WriteHeader "Zipping"
7z a $destination $Source

Write-Host "Done!" -ForegroundColor Green