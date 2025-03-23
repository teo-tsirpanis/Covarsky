#! /usr/bin/env pwsh

$PackagesDir = './Covarsky.Tests/packages'
$TestLogs = './test-logs/'
$TestProject = './Covarsky.Tests/Covarsky.Tests.fsproj'

Remove-Item $TestLogs -Recurse -Force -ErrorAction Ignore
Get-ChildItem $PackagesDir -Filter 'Covarsky*' -ErrorAction Ignore | ForEach-Object { Remove-Item $_.FullName -Recurse -Force }
dotnet clean /v:m /nodereuse:false
dotnet pack Covarsky/Covarsky.csproj -o $PackagesDir /p:Version=0.0.0-test

for ($i = 1; ($i -le 3) -and ($LASTEXITCODE -eq 0); $i++) {
    dotnet test $TestProject /nodereuse:false ("/bl:{0}run-{1}.binlog" -f $TestLogs, $i)
}

exit $LASTEXITCODE
