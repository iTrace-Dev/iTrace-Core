#ifndef MAIN_WINDOW_HPP
#define MAIN_WINDOW_HPP

#include <QMainWindow>
#include "ui_mainwindow.h" //Auto-generated
#include "tracker_manager.hpp"
#include "server.hpp"
#include "gaze_buffer.hpp"

enum state {IDLE, TRACKING};

namespace Ui {
class MainWindow;
}

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit MainWindow(QWidget *parent = 0);
    ~MainWindow();

public slots:
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
    Server server;
    state app_state;
    void toggleStartButtonText();
};

#endif // MAIN_WINDOW_HPP
