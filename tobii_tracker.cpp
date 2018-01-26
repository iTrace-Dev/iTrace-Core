#include "tobii_tracker.hpp"
#include <QDebug>

TobiiTracker::TobiiTracker():eyeTracker(nullptr) {}

TobiiTracker::TobiiTracker(TobiiResearchEyeTracker *tracker):eyeTracker(tracker) {
    char* name;
    tobii_research_get_device_name(eyeTracker, &name);
    deviceName = name;
    tobii_research_free_string(name);
}

TobiiResearchEyeTrackers* get_tobii_trackers() {
    TobiiResearchEyeTrackers* eyetrackers = nullptr;
    TobiiResearchStatus result;
    result = tobii_research_find_all_eyetrackers(&eyetrackers);

    if (result != TOBII_RESEARCH_STATUS_OK) {
        return nullptr;
    }

    return eyetrackers;
}

std::string TobiiTracker::trackerName() const {
    return deviceName;
}

void TobiiTracker::startTracker() {
    qDebug() << "Start";
}

void TobiiTracker::stopTracker() {
    qDebug() << "Stop";
}

void TobiiTracker::enterCalibration() {}
void TobiiTracker::leaveCalibration(){}
void TobiiTracker::useCalibrationPoint(float x, float y){}
void TobiiTracker::discardCalibrationPoint(float x, float y){}
