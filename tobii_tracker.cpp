#include <QApplication>
#include <QDesktopWidget>
#include "tobii_tracker.hpp"
#include "gaze_data.hpp"
#include "gaze_buffer.hpp"
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
    }
}

void TobiiTracker::stopTracker() {
    qDebug() << "Stop";
    TobiiResearchStatus status = tobii_research_unsubscribe_from_gaze_data(eyeTracker, gazeDataCallback);
    if (status != TOBII_RESEARCH_STATUS_OK) {
        qDebug() << "Unable to unsubscribe from eye tracker data";
    }
}

void TobiiTracker::enterCalibration() {
    TobiiResearchStatus status = tobii_research_screen_based_calibration_enter_calibration_mode(eyeTracker);
    if (status != TOBII_RESEARCH_STATUS_OK){
    }
}
void TobiiTracker::leaveCalibration(){
    TobiiResearchCalibrationResult* calibration_result = NULL;
    TobiiResearchStatus status = tobii_research_screen_based_calibration_compute_and_apply(eyeTracker, &calibration_result);
    if (!(status == TOBII_RESEARCH_STATUS_OK && calibration_result->status == TOBII_RESEARCH_CALIBRATION_SUCCESS)) {
        qDebug() << "Calibration Failed";
    }

    status = tobii_research_screen_based_calibration_leave_calibration_mode(eyeTracker);
    if (status != TOBII_RESEARCH_STATUS_OK){
        qDebug() << "Unable to leave calibration";
    }
}

void TobiiTracker::useCalibrationPoint(float x, float y){
    while (tobii_research_screen_based_calibration_collect_data(eyeTracker, x, y) != TOBII_RESEARCH_STATUS_OK) {
       qDebug() << "Unable to collect calibration data.\nRetrying...";
    }
}

void TobiiTracker::discardCalibrationPoint(float x, float y){
    if (tobii_research_screen_based_calibration_discard_data(eyeTracker, x, y) != TOBII_RESEARCH_STATUS_OK) {
       qDebug() << "Unable to discard calibration data point.\n";
    }
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

void gazeDataCallback(TobiiResearchGazeData* gd, void* user_data) {
    GazeBuffer& buffer = GazeBuffer::Instance();

    QRect screen= QApplication::desktop()->screenGeometry();
    int width = screen.width();
    int height = screen.height();

    //The GazeData Constructor is gross and needs to be made better...
    buffer.enqueue( new GazeData( gd->left_eye.pupil_data.diameter, gd->left_eye.pupil_data.validity,
                                  width * gd->left_eye.gaze_point.position_on_display_area.x, height * gd->left_eye.gaze_point.position_on_display_area.y,
                                  gd->left_eye.gaze_origin.position_in_user_coordinates.x, gd->left_eye.gaze_origin.position_in_user_coordinates.y, gd->left_eye.gaze_origin.position_in_user_coordinates.z,

                                  gd->right_eye.pupil_data.diameter, gd->right_eye.pupil_data.validity,
                                  width * gd->right_eye.gaze_point.position_on_display_area.x, height* gd->right_eye.gaze_point.position_on_display_area.y,
                                  gd->right_eye.gaze_origin.position_in_user_coordinates.x, gd->right_eye.gaze_origin.position_in_user_coordinates.y, gd->right_eye.gaze_origin.position_in_user_coordinates.z,

                                  gd->device_time_stamp, gd->system_time_stamp) );
}
