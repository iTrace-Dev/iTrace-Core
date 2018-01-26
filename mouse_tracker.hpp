#ifndef MOUSE_TRACKER_HPP
#define MOUSE_TRACKER_HPP

#include <QTimer>
#include <QCursor>
#include <string>
#include "tracker.hpp"

class MouseTracker: public Tracker {

    Q_OBJECT

public:
    MouseTracker();
    ~MouseTracker();

    void enterCalibration();
    void leaveCalibration();
    void useCalibrationPoint(float x, float y);
    void discardCalibrationPoint(float x, float y);
    void startTracker();
    void stopTracker();
    std::string trackerName() const;

public slots:
    void trackMouse();

private:
    QTimer* timer;
    QPoint cursor;
    std::string deviceName;
};

#endif // MOUSE_TRACKER_HPP
