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
1. If you want to re-create the .iss file, simply create a new script in the Inno Script Studio. It has a great wizard for creating the scripts. However, the wizard is unable to create root folders it would seem. So, you will need to add any folders that should be in the main folder of your projects install directory.
**Example** 
In the iTrace Toolkit client folder we have three folder ```lxml, tcl, and tk``` these folders will not be created instead their contents will be dumped into the main installation directory. 
It will look something like this in the script:
```
Source: "..\dist\client\lxml\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\dist\client\tcl\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\dist\client\tk\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
```
To fix this we add the name oif the folder to create after the {app} variable in DestDir, like this:
```
Source: "..\dist\client\lxml\*"; DestDir: "{app}\lxml"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\dist\client\tcl\*"; DestDir: "{app}\tcl"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\dist\client\tk\*"; DestDir: "{app}\tk"; Flags: ignoreversion recursesubdirs createallsubdirs
```
There is likely a more elegant way to do this but for now, this works.
2. Whether you re-compile or use the existing the installer can be (re)created by 'compiling' the .iss script. Compiling can be done in a few ways, you can compile by right clicking on the script and chosing **Compile** or, alternatively, you can open the file in Inno Script Setup go to **Project->Compile**.
3. Once compiled, the actuall installer exe will be in whatever folder specified by the line
	```OutputDir=```
   Default is ```win_installer```
