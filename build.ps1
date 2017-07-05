$ErrorActionPreference = "Stop"

$PSVersionTable.PSVersion

function testExitCode() {
    If ($LASTEXITCODE -ne 0) {
        write-host -f green "lastexitcode: $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}

# Build sonaranalyzer
Set-Location sonaranalyzer-dotnet
& .\build\Build.ps1 `
  -analyze `
  -build `
  -test `
  -package `
  -configuration Release
testExitCode
Set-Location ..
