#ifndef MOUSETRACKER_H
#define MOUSETRACKER_H

#include <QCursor>
#include <QTimer>
#include <QLocalServer>
#include <QLocalSocket>
#include <QTcpServer>
#include <QTcpSocket>
#include <vector>
#include "gazeserver.h"
#include "tracker.h"

using namespace std;

class MouseTracker: Tracker
{
    Q_OBJECT
    QPoint cursor;
    QTimer * timer;
    GazeServer* gazeServer;

private slots:


public:
    MouseTracker();
    ~MouseTracker();
    //void start();
    //void stop();
    void enterCalibration() override;
    void leaveCalibration() override;
    void useCalibrationPoint(float x, float y) override;
    void discardCalibrationPoint(float x, float y) override;
    void startTracker() override;
    void stopTracker() override;

public slots:
    void trackMouse();


};

#endif // MOUSETRACKER_H
