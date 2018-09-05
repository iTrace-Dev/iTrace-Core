#-------------------------------------------------
#
# Project created by QtCreator 2017-01-25T21:42:14
#
#-------------------------------------------------

QT       += core gui

greaterThan(QT_MAJOR_VERSION, 4): QT += widgets

TARGET = itrace_core
TEMPLATE = app

# The following define makes your compiler emit warnings if you use
# any feature of Qt which as been marked as deprecated (the exact warnings
# depend on your compiler). Please consult the documentation of the
# deprecated API in order to know how to port your code away from it.
DEFINES += QT_DEPRECATED_WARNINGS

# You can also make your code fail to compile if you use deprecated APIs.
# In order to do so, uncomment the following line.
# You can also select to disable deprecated APIs only up to a certain version of Qt.
#DEFINES += QT_DISABLE_DEPRECATED_BEFORE=0x060000    # disables all the APIs deprecated before Qt 6.0.0

QT += network websockets

SOURCES += main.cpp\
    tracker_manager.cpp \
    main_window.cpp \
    tobii_tracker.cpp \
    mouse_tracker.cpp \
    calibration_screen.cpp \
    gaze_buffer.cpp \
    reticle.cpp \
    gaze_handler.cpp \
    session_window.cpp \
    socket_server.cpp \
    websocket_server.cpp \
    session_manager.cpp \
    xml_writer.cpp

HEADERS  += \
    tracker_manager.hpp \
    tobii_tracker.hpp \
    tracker.hpp \
    main_window.hpp \
    mouse_tracker.hpp \
    calibration_screen.hpp \
    gaze_data.hpp \
    gaze_buffer.hpp \
    reticle.hpp \
    gaze_handler.hpp \
    session_window.hpp \
    socket_server.hpp \
    websocket_server.hpp \
    session_manager.hpp \
    xml_writer.hpp

HEADERS  +=

FORMS    += \
    reticle.ui \
    session_window.ui \
    main_window.ui


win32: LIBS += -L$$PWD/deps/x64/release/lib/ -ltobii_research

INCLUDEPATH += $$PWD/deps/include/tobii_sdk
DEPENDPATH += $$PWD/deps/include/tobii_sdk
