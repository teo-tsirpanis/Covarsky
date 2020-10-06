function Test-ExitCode {if ($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}}

$PackagesDir = './Covarsky.Tests/packages'

dotnet pack Covarsky/Covarsky.csproj -o $PackagesDir /p:Version=0.0.0-test
Test-ExitCode
for ($i = 1; $i -le 3; $i++) {
    dotnet test Covarsky.Tests/Covarsky.Tests.fsproj /bl:run-$i.binlog
    Test-ExitCode
}
