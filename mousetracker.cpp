#include "mousetracker.h"
#include <QCursor>
#include <QDebug>
#include <QtNetwork>
#include <sstream>
#include <vector>

using namespace std;

MouseTracker::MouseTracker()
{
    gazeServer = GazeServer::getGazeServer();
    timer = new QTimer(this);
    connect(timer, SIGNAL(timeout()), this, SLOT(trackMouse()));
}

MouseTracker::~MouseTracker(){
    delete timer;
}

/*void MouseTracker::start(){
    timer->start(16);
}*/

/*void MouseTracker::stop(){
    timer->stop();
}*/

void MouseTracker::trackMouse(){
    cursor = QCursor::pos();
    stringstream ss;
    ss << cursor.x() << "," << cursor.y() << '\n';
    gazeServer->sendGazeData(ss.str().c_str(), ss.str().length()+1);
}

void MouseTracker::enterCalibration(){
    return;
}

void MouseTracker::leaveCalibration(){
    return;
}

void MouseTracker::useCalibrationPoint(float x, float y){
    return;
}

void MouseTracker::discardCalibrationPoint(float x, float y){
    return;
}

void MouseTracker::startTracker(){
    timer->start(16);
}

void MouseTracker::stopTracker() {
    timer->stop();
}
