#include "tobii_tracker.hpp"
#include "gaze_data.hpp"
#include "gaze_buffer.hpp"
#include <QXmlStreamWriter>
#include <QFile>
#include <chrono>
#include <ctime>
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
    TobiiResearchGazeData gazeData;
    TobiiResearchStatus status = tobii_research_subscribe_to_gaze_data(eyeTracker, gazeDataCallback, &gazeData);
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
    TobiiResearchCalibrationResult* calibrationResult = NULL;
    TobiiResearchStatus status = tobii_research_screen_based_calibration_compute_and_apply(eyeTracker, &calibrationResult);
    if (!(status == TOBII_RESEARCH_STATUS_OK && calibrationResult->status == TOBII_RESEARCH_CALIBRATION_SUCCESS)) {
        qDebug() << "Calibration Failed";
    }

    status = tobii_research_screen_based_calibration_leave_calibration_mode(eyeTracker);
    if (status != TOBII_RESEARCH_STATUS_OK){
        qDebug() << "Unable to leave calibration";
    }

    writeCalibrationData("calibration", calibrationResult);
    tobii_research_free_screen_based_calibration_result(calibrationResult);
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


// FREE FUNCTIONS
// TRACKER DATA CALLBACK
void gazeDataCallback(TobiiResearchGazeData* gd, void* userData) {
    GazeBuffer& buffer = GazeBuffer::Instance();

    //The GazeData Constructor is gross and needs to be made better...
    buffer.enqueue( new GazeData( gd->left_eye.pupil_data.diameter, gd->left_eye.pupil_data.validity,
                                  gd->left_eye.gaze_point.position_on_display_area.x, gd->left_eye.gaze_point.position_on_display_area.y,
                                  gd->left_eye.gaze_origin.position_in_user_coordinates.x, gd->left_eye.gaze_origin.position_in_user_coordinates.y, gd->left_eye.gaze_origin.position_in_user_coordinates.z,

                                  gd->right_eye.pupil_data.diameter, gd->right_eye.pupil_data.validity,
                                  gd->right_eye.gaze_point.position_on_display_area.x, gd->right_eye.gaze_point.position_on_display_area.y,
                                  gd->right_eye.gaze_origin.position_in_user_coordinates.x, gd->right_eye.gaze_origin.position_in_user_coordinates.y, gd->right_eye.gaze_origin.position_in_user_coordinates.z,

                                  gd->device_time_stamp, gd->system_time_stamp, "tobii"));

}

// WRITE OUT CALIBRATION DATA
void writeCalibrationData(const std::string& directory, TobiiResearchCalibrationResult* calibrationData) {
    std::time_t t = std::time(nullptr);
    std::string startDateTime(ctime(&t));

    QFile calibrationOutputFile;
    calibrationOutputFile.setFileName(QString::fromStdString("calibration.xml"));
    calibrationOutputFile.open(QIODevice::WriteOnly);

    QXmlStreamWriter writer;
    writer.setDevice(&calibrationOutputFile);
    writer.setAutoFormatting(true); //Human readable formatting (can disable later)

    writer.writeStartDocument();
    writer.writeStartElement("calibration");
    writer.writeAttribute("status", QString::number(calibrationData->status));

    for (size_t i = 0; i < calibrationData->calibration_point_count; ++i) {
        TobiiResearchCalibrationPoint& calibrationPoint = calibrationData->calibration_points[i];
        writer.writeStartElement("point");
        writer.writeAttribute("x", QString::number(calibrationPoint.position_on_display_area.x));
        writer.writeAttribute("y", QString::number(calibrationPoint.position_on_display_area.y));

        for (size_t j = 0; j < calibrationPoint.calibration_sample_count; ++j) {
            TobiiResearchCalibrationSample& calibrationSample = calibrationPoint.calibration_samples[j];
            writer.writeEmptyElement("sample");
            writer.writeAttribute("left_x", QString::number(calibrationSample.left_eye.position_on_display_area.x));
            writer.writeAttribute("left_y", QString::number(calibrationSample.left_eye.position_on_display_area.y));
            writer.writeAttribute("left_validity", QString::number(calibrationSample.left_eye.validity));
            writer.writeAttribute("right_x", QString::number(calibrationSample.right_eye.position_on_display_area.x));
            writer.writeAttribute("right_y", QString::number(calibrationSample.right_eye.position_on_display_area.y));
            writer.writeAttribute("right_validity", QString::number(calibrationSample.right_eye.validity));
        }
        writer.writeEndElement();
    }
    writer.writeEndElement();
    writer.writeEndDocument();
    calibrationOutputFile.close();
}
