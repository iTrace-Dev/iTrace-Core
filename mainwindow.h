#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include "mousetracker.h"
#include <QMainWindow>
#include "tracker.h"


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

private:
    Ui::MainWindow *ui;
    MouseTracker * mouseTracker;
    QTcpSocket * socket;
    quint32 size;
};

#endif // MAINWINDOW_H
