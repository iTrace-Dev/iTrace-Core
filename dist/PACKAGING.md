# Packaging
## iTrace Core
### Dependencies
- Requires Microsoft Visual Studio [Installer Projects Plugin](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects)
- This Repo

### Steps to Generate Installer
1. Open the itrace_core.sln file in Visual Studio
2. Click on the itrace_core project in the Solution Explorer
3. At the top, select the desired build type and target platform (Ex. Release and x64)
4. Build the core project
5. Build the installer project
6. The .msi and Setup.exe should be in the installer project folder under the build type selected 
