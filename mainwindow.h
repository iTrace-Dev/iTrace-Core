#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include "mousetracker.h"
#include <QMainWindow>
#include <fstream>
#include "sessionsetup.h"
#include "tracker.h"
#include "xmlwriter.h"
#include "jsonwriter.h"


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

private:
    Ui::MainWindow *ui;
    MouseTracker * mouseTracker;
    QTcpSocket * socket;
    int timer;
    quint32 size;
    xmlWriter xmlwrite;

};

#endif // MAINWINDOW_H
