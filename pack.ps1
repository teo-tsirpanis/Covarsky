#! /usr/bin/env pwsh

$PackageOutputDir = './bin'

Remove-Item $PackageOutputDir -Recurse -Force -ErrorAction Ignore

dotnet pack Covarsky/Covarsky.csproj -c Release -o $PackageOutputDir
