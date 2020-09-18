$ErrorActionPreference = "Stop"

dotnet pack Covarsky/Covarsky.csproj -o Covarsky.Tests/test-packages /p:Version=0.0.0-test
dotnet test Covarsky.Tests/Covarsky.Tests.fsproj /bl:run-1.binlog
dotnet test Covarsky.Tests/Covarsky.Tests.fsproj /bl:run-2.binlog
