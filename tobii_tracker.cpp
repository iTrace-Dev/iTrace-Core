#include "tobii_tracker.hpp"
#include <QDebug>

TobiiTracker::TobiiTracker():eyeTracker(nullptr) {}

TobiiTracker::TobiiTracker(TobiiResearchEyeTracker *tracker):eyeTracker(tracker) {
    char* name;
    tobii_research_get_device_name(eyeTracker, &name);
    deviceName = name;
    tobii_research_free_string(name);
}

std::string TobiiTracker::trackerName() const {
    return deviceName;
}

void TobiiTracker::startTracker() {
    qDebug() << "Start";
    TobiiResearchGazeData gaze_data;
    TobiiResearchStatus status = tobii_research_subscribe_to_gaze_data(eyeTracker, gazeDataCallback, &gaze_data);
    if (status != TOBII_RESEARCH_STATUS_OK) {
        qDebug() << "Unable to subscribe to eye tracker data";
        return;
    }
}

void TobiiTracker::stopTracker() {
    qDebug() << "Stop";
    TobiiResearchStatus status = tobii_research_unsubscribe_from_gaze_data(eyeTracker, gazeDataCallback);
    if (status != TOBII_RESEARCH_STATUS_OK) {
        qDebug() << "Unable to unsubscribe from eye tracker data";
        return;
    }
}

void TobiiTracker::enterCalibration() {}
void TobiiTracker::leaveCalibration(){}
void TobiiTracker::useCalibrationPoint(float x, float y){}
void TobiiTracker::discardCalibrationPoint(float x, float y){}

TobiiResearchEyeTrackers* get_tobii_trackers() {
    TobiiResearchEyeTrackers* eyetrackers = nullptr;
    TobiiResearchStatus result;
    result = tobii_research_find_all_eyetrackers(&eyetrackers);

    if (result != TOBII_RESEARCH_STATUS_OK) {
        return nullptr;
    }

    return eyetrackers;
}

void gazeDataCallback(TobiiResearchGazeData* gaze_data, void* user_data) {
    memcpy(user_data, gaze_data, sizeof(*gaze_data));
    //qDebug() << static_cast<TobiiResearchGazeData*>(user_data)->left_eye.gaze_origin.position_in_user_coordinates.x;
}
