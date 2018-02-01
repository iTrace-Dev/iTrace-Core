#ifndef GAZE_DATA_HPP
#define GAZE_DATA_HPP

/*
 * POD Class
 *
 * Public members as the only other functions (so far)
 * would be get/set.
 *
 * Should only be created in specific tracker instances,
 * held by the buffer queue, and written out (socket/file).
 *
 * If different trackers supply data in a
 * significantly different fashion/format,
 * just make abstract (like with Tracker).
 *
 */

#include <cstdint>
#include <QDebug>

class GazeData {

    public:

        GazeData(): leftDiameter(0), leftValidity(0), leftX(0), leftY(0),
                    rightDiameter(0), rightValidity(0), rightX(0), rightY(0),
                    trackerTime(0), systemTime(0) {}

        GazeData(double lDiam, double lValid, double lX, double lY,
                 double rDiam, double rValid, double rX, double rY,
                 int64_t tTime, int64_t sTime):
                    leftDiameter(lDiam), leftValidity(lValid), leftX(lX), leftY(lY),
                    rightDiameter(rDiam), rightValidity(rValid), rightX(rX), rightY(rY),
                    trackerTime(tTime), systemTime(sTime) {}

        ~GazeData() {}

    public:
        double leftDiameter;
        double leftValidity;
        double leftX;
        double leftY;

        double rightDiameter;
        double rightValidity;
        double rightX;
        double rightY;

        int64_t trackerTime;
        int64_t systemTime;
};

#endif // GAZE_DATA_HPP
