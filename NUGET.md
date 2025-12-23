# NuGet Package Guide

Guide for building, testing, and publishing NuGet packages for MasterZdran.Logging projects.

---

## 📦 Available Packages

### 1. MasterZdran.Logging.AzureTableStorage

**Core logging library** with Azure Table Storage implementation.

- **Package ID**: `MasterZdran.Logging.AzureTableStorage`
- **Version**: 0.1.0 (pre-production)
- **NuGet**: [View on NuGet.org](https://www.nuget.org/packages/MasterZdran.Logging.AzureTableStorage/)
- **Description**: Production-ready Azure Table Storage logging with Microsoft.Extensions.Logging support
- **Target Frameworks**: .NET 10.0, .NET 9.0, .NET 8.0, .NET 7.0
- **Dependencies**:
  - Azure.Data.Tables (≥12.8.0)
  - Microsoft.Extensions.Logging.Abstractions (≥8.0.0)
  - OpenTelemetry (≥1.7.0)

### 2. MasterZdran.Logging.Configuration

**Configuration providers** for Azure Key Vault, App Configuration, and more.

- **Package ID**: `MasterZdran.Logging.Configuration`
- **Version**: 0.1.0 (pre-production)
- **NuGet**: [View on NuGet.org](https://www.nuget.org/packages/MasterZdran.Logging.Configuration/)
- **Description**: Configuration helpers for MasterZdran.Logging.AzureTableStorage
- **Target Frameworks**: .NET 10.0, .NET 9.0, .NET 8.0, .NET 7.0
- **Dependencies**:
  - MasterZdran.Logging.AzureTableStorage (≥0.1.0)
  - Azure.Security.KeyVault.Secrets (≥4.5.0)
  - Azure.Data.AppConfiguration (≥1.4.0)

---

## 🔨 Building NuGet Packages

### Using PowerShell Script (Recommended)

```powershell
# Build and pack with default settings (Release, run tests)
.\build-nuget.ps1

# Build in Debug mode without tests
.\build-nuget.ps1 -Configuration Debug -SkipTests

# Override version
.\build-nuget.ps1 -Version 1.0.1

# Custom output path
.\build-nuget.ps1 -OutputPath ./packages
```

### Manual Build Commands

```bash
# Clean solution
dotnet clean -c Release

# Restore packages
dotnet restore

# Build solution
dotnet build -c Release

# Run tests (recommended)
dotnet test -c Release

# Pack individual projects
dotnet pack src/MasterZdran.Logging.AzureTableStorage/MasterZdran.Logging.AzureTableStorage.csproj -c Release -o ./nupkgs
dotnet pack src/MasterZdran.Logging.Configuration/MasterZdran.Logging.Configuration.csproj -c Release -o ./nupkgs

# Pack entire solution
dotnet pack -c Release -o ./nupkgs
```

---

## ✅ Testing Packages Locally

### Install from Local Directory

```bash
# Add package from local directory
dotnet add package MasterZdran.Logging.AzureTableStorage --source ./nupkgs --version 1.0.0

# Or with explicit path
dotnet add package MasterZdran.Logging.AzureTableStorage --source "C:\path\to\nupkgs"
```

### Create Local NuGet Feed

```powershell
# Add local source
dotnet nuget add source C:\path\to\nupkgs --name LocalPackages

# List sources
dotnet nuget list source

# Install from local source
dotnet add package MasterZdran.Logging.AzureTableStorage --source LocalPackages
```

### Test in Sample Project

```bash
# Create test project
mkdir TestApp
cd TestApp
dotnet new console

# Add local package
dotnet add package MasterZdran.Logging.AzureTableStorage --source ../nupkgs

# Run to verify
dotnet run
```

---

## 🚀 Publishing to NuGet.org

### Prerequisites

1. **NuGet.org Account**: Create account at [nuget.org](https://www.nuget.org/)
2. **API Key**: Generate at [nuget.org/account/apikeys](https://www.nuget.org/account/apikeys)
   - Scope: Push new packages and package versions
   - Packages: Select specific packages or use glob pattern `MasterZdran.Logging.*`
   - Expiration: Set appropriate expiration

### Publish Commands

```bash
# Set API key (one-time setup)
dotnet nuget push --help

# Push single package
dotnet nuget push ./nupkgs/MasterZdran.Logging.AzureTableStorage.1.0.0.nupkg \
  --api-key <YOUR_API_KEY> \
  --source https://api.nuget.org/v3/index.json

# Push all packages
dotnet nuget push ./nupkgs/*.nupkg \
  --api-key <YOUR_API_KEY> \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate

# Store API key (Windows)
dotnet nuget setapikey <YOUR_API_KEY> --source https://api.nuget.org/v3/index.json
```

### Secure API Key Management

**Environment Variable:**

```powershell
# Windows
$env:NUGET_API_KEY = "your-api-key-here"
dotnet nuget push ./nupkgs/*.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json

# Linux/macOS
export NUGET_API_KEY="your-api-key-here"
dotnet nuget push ./nupkgs/*.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

**Azure Key Vault:**

```powershell
# Retrieve from Key Vault
$apiKey = az keyvault secret show --name "NuGetApiKey" --vault-name "MyVault" --query value -o tsv
dotnet nuget push ./nupkgs/*.nupkg --api-key $apiKey --source https://api.nuget.org/v3/index.json
```

---

## 📤 Publishing to Private NuGet Feed

### Azure Artifacts

```bash
# Add Azure Artifacts source
dotnet nuget add source https://pkgs.dev.azure.com/{organization}/_packaging/{feed}/nuget/v3/index.json \
  --name AzureArtifacts \
  --username {username} \
  --password {PAT}

# Push to Azure Artifacts
dotnet nuget push ./nupkgs/*.nupkg \
  --api-key AzureArtifacts \
  --source https://pkgs.dev.azure.com/{organization}/_packaging/{feed}/nuget/v3/index.json
```

### GitHub Packages

```bash
# Add GitHub Packages source
dotnet nuget add source https://nuget.pkg.github.com/{owner}/index.json \
  --name github \
  --username {username} \
  --password {PAT}

# Push to GitHub Packages
dotnet nuget push ./nupkgs/*.nupkg \
  --api-key {PAT} \
  --source https://nuget.pkg.github.com/{owner}/index.json
```

### Private NuGet Server

```bash
# Add private source
dotnet nuget add source https://your-nuget-server.com/api/v3/index.json \
  --name PrivateNuGet \
  --username {username} \
  --password {password}

# Push to private server
dotnet nuget push ./nupkgs/*.nupkg \
  --api-key {api-key} \
  --source https://your-nuget-server.com/api/v3/index.json
```

---

## 🔄 Versioning

### Version Format

Following [Semantic Versioning 2.0.0](https://semver.org/):

- **Major.Minor.Patch** (e.g., 1.2.3)
- **Major.Minor.Patch-PreRelease** (e.g., 1.2.3-beta.1)

### Version Strategy

**Increment:**

- **Major**: Breaking changes
- **Minor**: New features (backward compatible)
- **Patch**: Bug fixes (backward compatible)

**Pre-release Tags:**

- `alpha`: Early development
- `beta`: Feature complete, testing
- `rc`: Release candidate

### Updating Version

**Option 1: Edit .csproj Files**

```xml
<Version>1.0.1</Version>
```

**Option 2: Command Line Override**

```bash
dotnet pack -p:Version=1.0.1 -o ./nupkgs
```

**Option 3: Build Script**

```powershell
.\build-nuget.ps1 -Version 1.0.1
```

---

## 📋 Pre-Release Checklist

Before publishing packages:

- [ ] **Version Updated**: Increment version in .csproj files
- [ ] **CHANGELOG.md Updated**: Document changes
- [ ] **Tests Pass**: All 114 tests passing
- [ ] **Documentation Updated**: README and API docs current
- [ ] **Build Successful**: Clean Release build completes
- [ ] **Packages Created**: Both .nupkg files generated
- [ ] **Local Testing**: Packages tested in sample project
- [ ] **Dependencies Verified**: Package references correct
- [ ] **README Included**: README.md packed with package
- [ ] **License Verified**: MIT license included
- [ ] **Release Notes**: Added to package metadata

---

## 🔍 Package Validation

### Inspect Package Contents

```bash
# Install NuGet Package Explorer (Windows)
choco install nugetpackageexplorer

# Or use command line
dotnet tool install -g dotnet-nuget-validator
dotnet nuget-validator ./nupkgs/MasterZdran.Logging.AzureTableStorage.1.0.0.nupkg
```

### Verify Package Metadata

```powershell
# Extract .nupkg (it's a ZIP file)
Expand-Archive ./nupkgs/MasterZdran.Logging.AzureTableStorage.1.0.0.nupkg -DestinationPath ./temp

# View .nuspec
Get-Content ./temp/MasterZdran.Logging.AzureTableStorage.nuspec

# Check contents
Get-ChildItem ./temp -Recurse
```

### Validate Dependencies

```bash
# Check package dependencies
dotnet list package --include-transitive
```

---

## 🐛 Troubleshooting

### Package Already Exists

```
error: Response status code does not indicate success: 409 (Conflict - The package already exists.)
```

**Solution**: Increment version number and rebuild.

### Missing Dependencies

```
error: Unable to find package 'MasterZdran.Logging.AzureTableStorage'
```

**Solution**:

1. Ensure core package is published first
2. Check version compatibility
3. Verify package source is added

### Authentication Failed

```
error: 401 (Unauthorized)
```

**Solution**:

1. Verify API key is valid
2. Check API key has correct permissions
3. Ensure key hasn't expired

### Symbols Package Issues

If symbol packages (.snupkg) fail:

```bash
# Disable symbol package generation
dotnet pack --no-symbols -o ./nupkgs
```

---

## 📊 Package Analytics

After publishing to NuGet.org:

1. **Download Stats**: View at `https://www.nuget.org/packages/MasterZdran.Logging.AzureTableStorage`
2. **Dependency Analysis**: See which packages depend on yours
3. **Version History**: Track adoption of different versions

---

## 🔐 Security Best Practices

1. **Never Commit API Keys**: Add `.nuget-keys` to `.gitignore`
2. **Use Environment Variables**: Store keys in environment variables
3. **Rotate Keys Regularly**: Generate new API keys periodically
4. **Limit Key Scope**: Only grant necessary permissions
5. **Monitor Usage**: Watch for suspicious activity
6. **Enable 2FA**: Secure your NuGet.org account

---

## 📚 Additional Resources

- [NuGet Package Documentation](https://docs.microsoft.com/en-us/nuget/)
- [Creating NuGet Packages](https://docs.microsoft.com/en-us/nuget/create-packages/creating-a-package)
- [Publishing Packages](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [NuGet Package Explorer](https://github.com/NuGetPackageExplorer/NuGetPackageExplorer)
- [Semantic Versioning](https://semver.org/)

---

## 🆘 Support

For issues or questions:

- **GitHub Issues**: [Report issues](https://github.com/masterzdran/mz-logging-azure-tablestorage-csharp/issues)
- **Email**: nuno.cancelo@gmail.com
- **Documentation**: [Full Documentation](DOCUMENTATION.md)

---

**Last Updated**: December 23, 2024  
**Package Version**: 1.0.0
