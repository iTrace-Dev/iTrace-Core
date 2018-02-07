#ifndef TOBII_TRACKER_HPP
#define TOBII_TRACKER_HPP

#include <string>
#include "tobii_research.h"
#include "tobii_research_eyetracker.h"
#include "tobii_research_streams.h"
#include "tobii_research_calibration.h"
#include "tracker.hpp"
#include <QDebug>

class TobiiTracker: public Tracker {

    public:
        TobiiTracker();
        TobiiTracker(TobiiResearchEyeTracker* tracker);
        ~TobiiTracker() {}

        void enterCalibration();
        void leaveCalibration();
        void useCalibrationPoint(float x, float y);
        void discardCalibrationPoint(float x, float y);
        void startTracker();
        void stopTracker();
        std::string trackerName() const;

    private:
        TobiiResearchEyeTracker* eyeTracker;
        std::string deviceName;
};

/*
 * Free function to populate the list of tobii trackers
 *    A collection of trackers is the only available format
 *    for the trackers which allows for an appropriate free
 *    to occur.
 */
TobiiResearchEyeTrackers* get_tobii_trackers();

//Callback function for tobii api
void gazeDataCallback(TobiiResearchGazeData* gaze_data, void* user_data);

#endif // TOBII_TRACKER_HPP
