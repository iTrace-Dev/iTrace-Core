#include "mouse_tracker.hpp"
#include "gaze_buffer.hpp"
#include "gaze_data.hpp"
#include <cstdint> //provides int64_t
#include <QDateTime>
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

void MouseTracker::trackMouse() {
    cursor = QCursor::pos();
    GazeBuffer& buffer = GazeBuffer::Instance();
    buffer.enqueue( new GazeData(int64_t(QDateTime::currentDateTime().toTime_t()), cursor.x(), cursor.y(), "mouse") );
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
