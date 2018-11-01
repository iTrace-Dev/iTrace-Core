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

        void startTracker();
        void stopTracker();
        std::string trackerName() const;

        // These don't and probablly never will apply to the mouse
        // They still need to be here to conform to the tracker interface
        void enterCalibration() {}
        void leaveCalibration() {}
        void useCalibrationPoint(float, float) {}
        void discardCalibrationPoint(float, float) {}

    public slots:
        void trackMouse();

    private:
        QTimer* timer;
        QPoint cursor;
        std::string deviceName;
};

#endif // MOUSE_TRACKER_HPP
