#ifndef MAIN_WINDOW_HPP
#define MAIN_WINDOW_HPP

#include <QMainWindow>
#include <string>
#include "ui_main_window.h" //Auto-generated
#include "tracker_manager.hpp"
#include "server.hpp"
#include "gaze_buffer.hpp"
#include "gaze_handler.hpp"
#include "reticle.hpp"
#include "session_window.hpp"
#include "xml_writer.hpp"

Q_DECLARE_METATYPE(std::string)

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
        TrackerManager trackerManager;
        GazeBuffer* buffer;
        GazeHandler* bufferHandler;
        Reticle reticle;
        SessionWindow sessionDialog;
        XMLWriter xml;

        Server server;
        state app_state;
        void toggleStartButtonText();
};

#endif // MAIN_WINDOW_HPP
