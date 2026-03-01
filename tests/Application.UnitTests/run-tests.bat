@echo off
echo Running tests...
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

echo Generating report...
reportgenerator -reports:./TestResults/**/coverage.cobertura.xml -targetdir:./TestReport -reporttypes:Html

echo Report generated in TestReport/index.html
start ./TestReport/index.html