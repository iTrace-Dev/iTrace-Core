#include "mouse_tracker.hpp"
#include <QDebug>

MouseTracker::MouseTracker()
{
    timer = new QTimer(this);
    deviceName = "Mouse Tracker";
    connect(timer, SIGNAL(timeout()), this, SLOT(trackMouse()));
}

MouseTracker::~MouseTracker() {
    delete timer;
}

void MouseTracker::enterCalibration() {}
void MouseTracker::leaveCalibration() {}
void MouseTracker::useCalibrationPoint(float x, float y) {}
void MouseTracker::discardCalibrationPoint(float x, float y) {}

void MouseTracker::trackMouse() {
    cursor = QCursor::pos();
    qDebug() << "mouse @: " << cursor.x() << ", " << cursor.y();
}

void MouseTracker::startTracker() {
    timer->start(16);
}

void MouseTracker::stopTracker() {
    timer->stop();
}

std::string MouseTracker::trackerName() const {
    return deviceName;
}
