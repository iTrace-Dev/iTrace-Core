:: CREATE BUILD DIRECTORY IF IT DOES NOT EXIST
IF NOT EXIST ..\itrace_build mkdir ..\itrace_build

:: GENERATE BUILD PROJECT
cd ..\itrace_build
cmake ..\iTrace-Core -G "Visual Studio 15 2017 Win64"
IF errorlevel 1 GOTO:EOF

:: RUN BUILD
cmake --build . --config release
