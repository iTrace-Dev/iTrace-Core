:: SET PATH TO ROOT OF QT INSTALL VERSION
SET CMAKE_ROOT=C:\Qt\5.9.2

:: SET TARGET COMPILER TARGET DIRECTORY (WITHIN QT INSTALL DIRECTORY)
SET TARGET_VSVERSION=msvc2017_64

:: ENVIRONMENT VARIABLE USED BY CMAKE TO LOCATE QT CMAKE FILES
SET QT_CMAKE_DIR=%CMAKE_ROOT%\%TARGET_VSVERSION%

:: CREATE BUILD DIRECTORY IF IT DOES NOT EXIST
IF NOT EXIST ..\itrace_build mkdir ..\itrace_build

:: GENERATE BUILD PROJECT
cd ..\itrace_build
cmake ..\iTrace-Core -G "Visual Studio 15 2017 Win64"
IF errorlevel 1 GOTO:EOF

:: RUN BUILD
cmake --build . --config release
