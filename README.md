# Atlassian-Authentication-Helper
A GUI helper to prompt for Atlassian credentials.


# Build

## Publish to single file executable

### Mac

    dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true

### Windows

    dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true

### Linux

    dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
