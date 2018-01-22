#ifndef TRACKER_H
#define TRACKER_H

#include <QObject>

class Tracker: public QObject
{
    Q_OBJECT

public:
    virtual void enterCalibration() = 0;
    virtual void leaveCalibration() = 0;
    virtual void useCalibrationPoint(float x, float y) = 0;
    virtual void discardCalibrationPoint(float x, float y) = 0;
    virtual void startTracker() = 0;
    virtual void stopTracker() = 0;
};

#endif // TRACKER_H
