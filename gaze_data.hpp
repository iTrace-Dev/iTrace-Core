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

#include <cstdint> //provides int64_t
#include <string>
#include <cmath>
#include <QDebug>

class GazeData {

    public:

        GazeData(): leftDiameter(0), leftValidity(0), leftX(0), leftY(0),
                    rightDiameter(0), rightValidity(0), rightX(0), rightY(0),
                    trackerTime(0), systemTime(0) {}

        GazeData(double lDiam, double lValid, double lX, double lY, double user_lX, double user_lY, double user_lZ,
                 double rDiam, double rValid, double rX, double rY, double user_rX, double user_rY, double user_rZ,
                 int64_t tTime, int64_t sTime, std::string tracker):
                    leftDiameter(lDiam), leftValidity(lValid), leftX(lX), leftY(lY),              // LEFT EYE BASIC DATA
                    user_pos_leftX(user_lX), user_pos_leftY(user_lY), user_pos_leftZ(user_lZ),    // LEFT EYE BASED USER POSITIONS
                    rightDiameter(rDiam), rightValidity(rValid), rightX(rX), rightY(rY),          // RIGHT EYE BASIC DATA
                    user_pos_rightX(user_rX), user_pos_rightY(user_rY), user_pos_rightZ(user_rZ), // RIGHT EYE BASED USER POSITIONS
                    trackerTime(tTime), systemTime(sTime), trackerType(tracker) {}                // TRACKER TYPES AND TIMESTAMPS


        // Temporary cheat constructor for mouse ONLY!
        // I know...just roll with it for now.
        GazeData(int64_t timestamp, double x, double y, std::string tracker): GazeData() {
            systemTime = timestamp;

            // Set them both to be the same so when they are averaged it comes out correct
            leftX = x;
            leftY = y;
            rightX = x;
            rightY = y;

            //Data will always be valid
            leftValidity = 1;
            rightValidity = 1;


            trackerType = tracker;
        }

        double getCalculatedX() {

            // Perfect data for both eyes
            if ( !(std::isnan(rightX)) && !(std::isnan(leftX)) )
                if (rightValidity && leftValidity && rightX >= 0 && leftX >= 0)
                    return ( (rightX + leftX) / 2 );

            // Perfect right eye data
            if ( !(std::isnan(rightX)) )
                if (rightValidity && rightX >= 0)
                    return rightX;

            // Perfect left eye data
            if ( !(std::isnan(leftX)) )
                if (leftValidity && leftX >= 0)
                    return leftX;

            // Everything is bad....
            return std::numeric_limits<double>::quiet_NaN();
        }

        double getCalculatedY() {
            // Perfect data for both eyes
            if ( !(std::isnan(rightY)) && !(std::isnan(leftY)) )
                if (rightValidity && leftValidity && rightY >= 0 && leftY >= 0)
                    return ( (rightY + leftY) / 2 );

            // Perfect right eye data
            if ( !(std::isnan(rightY)) )
                if (rightValidity && rightY >= 0)
                    return rightY;

            // Perfect left eye data
            if ( !(std::isnan(leftY)) )
                if (leftValidity && leftY >= 0)
                    return leftY;

            // Everything is bad....
            return std::numeric_limits<double>::quiet_NaN();
        }

        ~GazeData() {}

    public:
        double leftDiameter;
        double leftValidity;
        double leftX;
        double leftY;
        double user_pos_leftX;
        double user_pos_leftY;
        double user_pos_leftZ;

        double rightDiameter;
        double rightValidity;
        double rightX;
        double rightY;
        double user_pos_rightX;
        double user_pos_rightY;
        double user_pos_rightZ;

        std::string trackerType;

        int64_t trackerTime;
        int64_t systemTime;
};

#endif // GAZE_DATA_HPP
