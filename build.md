### SUPPORTED ARCHITECTURES
	* x64 (QT libraries are 64bit only for Visual Studio 2017)
	
### DEPENDENCY STRUCTURE NOTES
	* Tobii pro sdk dlls and lib files must be in the same directory (for dynamic loading)
	
### REQUIREMENTS
* QT 5.9.3 (5.10.0 not supported)
* Git
* Visual Studio 2017
* Tobii Pro SDK v1.2.0.33 (newer versions not supported)
* CMAKE (only for cmake builds)

### WINDOWS BUILD
* Clone iTrace-Core repo
* Download dependency zip to a folder named "deps" in root of
	iTrace-Core repository
	
### BUILDING IN QT CREATOR
* Ensure project is type is set to: Desktop QT 5.9.3 MSVC2017 64bit
* Double click enginetest.pro 
	* If it contains any path/build information for the tobii sdk as seen below, remove it
	
	```
	win32: LIBS += -L$$PWD/../../../Downloads/TobiiPro.SDK.C_Binding.Windows_1.2.0.33/64/lib/ -ltobii_research
	INCLUDEPATH += $$PWD/../../../Downloads/TobiiPro.SDK.C_Binding.Windows_1.2.0.33/64/include
	DEPENDPATH += $$PWD/../../../Downloads/TobiiPro.SDK.C_Binding.Windows_1.2.0.33/64/include
	```
	
* Highlight enginetest
	* right click add library
		* external library
		* browse to library file (..\iTrace-Core\deps\x64\release\lib\tobii_research.lib)
		* browse to include file (..\iTrace-Core\deps\include\tobii_sdk)
		* platform
			* only check "Windows"
		* linkage
			* static
		* uncheck add "d" for debug libraries
* Click "Projects" (Need to do this step for each release mode (release, debug, profile))
	* Find Build Environment
	* Hit details
		* Add environment variable called TOBII_HOME
		* Set path to tobii_research.dll (..\iTrace-Core\deps\x64\release\lib)

### BUILDING WITH CMAKE (ONLY SUPPORTS RELEASE BUILDS...FOR NOW)
* Create a user or system environment variable called "QT_CMAKE_DIR"
* This variable needs to identify the path to QT, the appropriate QT version, and the compilation target.
* Example:
	* To target QT 5.9.3 for microsoft visual c++ 2017 64-bit, the environment variable would be similar to this:
	* C:\Qt\5.9.3\msvc2017_64

#### CLI
* Open a cmd prompt (not powershell)
* Create a build directory outside of the iTrace-Core directory
* Run the command from build directory:
	cmake [path-to-sourcecode]\iTrace-Core -G "Visual Studio 15 2017 Win64"
* You can now run the build command from the build directory:
	cmake --build . --config release
* You can also run the win_build.bat script to create a build directory and build iTraceCore

#### CMake GUI + Visual Studio
* Open CMake GUI
* Use the first browse button to locate the iTrace-Core source directory
* Use the second browse button to locate the directory where you would like to built
	* Use a directory OUTSIDE of the source code directory
* Click configure
* Choose the Visual Studio 15 2017 Win64 Generator
* Leave default native compilers selected
* Click finish
* When configuration is complete, click Generate.
* You can click Open Project to launch visual studio or open the project file from your selected
	build directory
* In Visual Studio, set project build configuration dropdown to Release
* In solution explorer right click itrace and select "set as startup project"
* In solution explorer right click itrace and select "build"
* You should now be able to use the play button to run iTrace-Core




