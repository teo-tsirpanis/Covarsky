#! /usr/bin/env pwsh

function Remove-Directory-Checked {
    param ([string]$Directory)
    if (Test-Path $Directory -PathType Container) {
        Remove-Item $Directory -Recurse -Force
    }
}

$PackagesDir = './Covarsky.Tests/packages'
$TestLogs = './test-logs/'
$TestProject = './Covarsky.Tests/Covarsky.Tests.fsproj'

Remove-Directory-Checked $TestLogs
Remove-Directory-Checked $PackagesDir
dotnet clean /v:m /nodereuse:false
dotnet pack Covarsky/Covarsky.csproj -o $PackagesDir /p:Version=0.0.0-test

for ($i = 1; ($i -le 3) -and ($LASTEXITCODE -eq 0); $i++) {
    dotnet test $TestProject /nodereuse:false ("/bl:{0}run-{1}.binlog" -f $TestLogs, $i)
}

Compress-Archive $TestLogs -DestinationPath "test-logs.zip" -Force
exit $LASTEXITCODE
