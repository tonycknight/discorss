dotnet tool restore

dotnet test -c Debug --logger "trx;LogFileName=test_results.trx" /p:CollectCoverage=true /p:CoverletOutput=./TestResults/coverage.info /p:CoverletOutputFormat=cobertura

dotnet reportgenerator -reports:"./test/**/coverage.info" -targetdir:"./publish/codecoverage" -reporttypes:"Html"

./publish/codecoverage\index.html