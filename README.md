# Introduction

This project provides the primary API for the Pulse application.

# Application Secrets

The local secrets are not available in source control. To run locally:

1. Obtain the secrets.json file from another developer.
2. Place it in the project root directory. (It is already in .gitignore)
3. Pipe it into the `dotnet user-secrets` command, as follows:

- Windows: `type .\secrets.json | dotnet user-secrets set`
- Linux/MacOS: `cat ./secrets.json | dotnet user-secrets set`
