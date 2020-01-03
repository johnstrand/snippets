$target = $args[0]

if(-not $target) {
    Write-Host "No target specified, run with .\install.ps1 <directory>"
    return
}

$stat = Get-Item -Path $target -ErrorAction SilentlyContinue

if(-not $stat) {
    Write-Host "Couldn't find directory $target";
    return
}

if(-not $stat.PSIsContainer) {
    Write-Host "$target does not appear to be a directory";
    return
}

& dotnet publish -c release -o "$target"