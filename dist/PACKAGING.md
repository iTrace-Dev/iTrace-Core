# Packaging
## iTrace Core
### Visual Studio Installer Project
#### Dependencies
- Requires Microsoft Visual Studio [Installer Projects Plugin](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects)
- This Repo

#### Steps to Generate Installer
1. Open the itrace_core.sln file in Visual Studio
2. Click on the itrace_core project in the Solution Explorer
3. At the top, select the desired build type and target platform (Ex. Release and x64)
4. Build the core project
5. Build the installer project
6. The .msi and Setup.exe should be in the installer project folder under the build type selected 

### Inno Script
#### Dependencies
- Requires [Inno Setup](http://www.jrsoftware.org/isdl.php)
- HIGHLY RECOMMENDED [Inno Script Studio](https://www.kymoto.org/products/inno-script-studio/downloads)

#### Steps to Generate Installer
1. If you want to re-create the .iss file, simply create a new script in the Inno Script Studio. It has a great wizard for creating the scripts.
2. Otherwise, the installer can be re-created by 'compiling' the .iss script. Compiling can be done in a few ways, however the simplest is to open the file in Inno Script Setup go to Project->Compile.
3. Once compiled, the actuall installer exe will be in whatever folder specified by the line
	```OutputDir=```
   Default is ```win_installer```
