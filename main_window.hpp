#ifndef MAIN_WINDOW_HPP
#define MAIN_WINDOW_HPP

#include <QMainWindow>
#include <string>
#include "ui_main_window.h" //Auto-generated
#include "tracker_manager.hpp"
#include "socket_server.hpp"
#include "websocket_server.hpp"
#include "gaze_buffer.hpp"
#include "gaze_handler.hpp"
#include "reticle.hpp"
#include "session_window.hpp"
#include "xml_writer.hpp"

Q_DECLARE_METATYPE(std::string)
Q_DECLARE_METATYPE(GazeData)

enum state {IDLE, TRACKING};

namespace Ui {
    class MainWindow;
}

class MainWindow : public QMainWindow {
    Q_OBJECT

    public:
        explicit MainWindow(QWidget *parent = 0);
        ~MainWindow();

    private slots:
        void startTracker();
        void displayData();
        void toggleReticle();
        void startCalibration();
        void showSessionSetup();
        void setActiveTracker();

    private:
        Ui::MainWindow *ui;
        Reticle reticle;
        SessionWindow sessionDialog;

        state app_state;

        TrackerManager trackerManager;
        XMLWriter xml;
        SocketServer socketServer;
        WebsocketServer websocketServer;
        GazeHandler* bufferHandler;
};

#endif // MAIN_WINDOW_HPP
