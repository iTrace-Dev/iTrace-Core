cmake_minimum_required(VERSION 2.8.11)

project(itrace)

if("x${CMAKE_CXX_COMPILER_ID}" STREQUAL "xMSVC")

	if ("${CMAKE_EXE_LINKER_FLAGS}" STREQUAL "/machine:X86")
		set(BUILD_ARCH "x86")
	else()
		set(BUILD_ARCH "x64")
	endif()

	set(WINDOWS_DEP_PATH ${PROJECT_SOURCE_DIR}/deps) 
	
	set (CMAKE_PREFIX_PATH $ENV{QT_CMAKE_DIR})

	# Find includes in corresponding build and deps directories
	set (CMAKE_INCLUDE_CURRENT_DIR ON)

	# Identify include and libraries in deps
	include_directories(${WINDOWS_DEP_PATH}/include/tobii_sdk)
	link_directories(${WINDOWS_DEP_PATH}/${BUILD_ARCH}/$(ConfigurationName)/lib)

	# Instruct CMake to run moc automatically when needed
	set (CMAKE_AUTOMOC ON)

	# Define required QT DLLS (for copying post build)
	set (QT_DLL_DEPS
		Qt5Widgets
		Qt5Core
		Qt5Xml
		Qt5Network
		Qt5Gui)
	
	# Define required Tobii DLLS (for copying post build)
	set (TOBII_DLL_DEPS
		tobii_pro
		tobii_research
		tobii_stream_engine)

	# Find the Qt libraries
	find_package(Qt5 REQUIRED 
		Widgets
		Core
		Xml
		Network
	)

	# Tell CMake to create the itrace executable
	add_executable (itrace main.cpp tobiipro.cpp tracker.cpp xmlwriter.cpp mousetracker.cpp reticle.cpp sessionsetup.cpp tobiiEyetracker.cpp calibrationscreen.cpp gazeserver.cpp jsonwriter.cpp mainwindow.cpp)

	# Link QT libraries to itrace executable
	target_link_libraries(itrace Qt5::Widgets Qt5::Core Qt5::Xml Qt5::Network)
		
	# Copy required QT DLL files to executable directory
	foreach (dep ${QT_DLL_DEPS})
		add_custom_command (TARGET itrace POST_BUILD
			COMMAND ${CMAKE_COMMAND} -E copy_if_different
			"$ENV{QT_CMAKE_DIR}/bin/${dep}.dll"
			$<TARGET_FILE_DIR:itrace>)
	endforeach(dep)

	# Copy required TOBII DLL files to executable directory
	foreach (dep ${TOBII_DLL_DEPS})
		add_custom_command (TARGET itrace POST_BUILD
			COMMAND ${CMAKE_COMMAND} -E copy_if_different
			"${WINDOWS_DEP_PATH}/${BUILD_ARCH}/release/lib/${dep}.dll"
			$<TARGET_FILE_DIR:itrace>)
	endforeach(dep)
endif()
