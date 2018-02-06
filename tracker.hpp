#ifndef TRACKER_HPP
#define TRACKER_HPP

#include <QObject>
#include <string>

/*
 * Abstract interface that all trackers must implement
 */
class Tracker: public QObject {

    public:
        virtual void enterCalibration() = 0;
        virtual void leaveCalibration() = 0;
        virtual void useCalibrationPoint(float x, float y) = 0;
        virtual void discardCalibrationPoint(float x, float y) = 0;
        virtual void startTracker() = 0;
        virtual void stopTracker() = 0;
        virtual std::string trackerName() const = 0;
        virtual ~Tracker() {}
};

#endif // TRACKER_HPP
