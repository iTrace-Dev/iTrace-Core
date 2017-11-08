#include "tobiiEyetracker.h"
#include <string.h>
#include <sstream>
#include <QObject>
#include <QApplication>
#include <QDesktopWidget>

using namespace std;

CALIBRATION_OPERATION(calibrationOperationStub){
    return (TobiiResearchStatus)1;
}
GET_TRACKER_ATTRIBUTE(GetTrackerAttributeStub){
    //*attribute = "Tobii Pro SDK not found!";
    //C++ error, trying to convert char[] to a char *
    return (TobiiResearchStatus)1;
}

void gazeDataCallback(TobiiResearchGazeData* gazeData, void* userData){
    memcpy(userData, gazeData, sizeof(*gazeData));
    stringstream ss;
    QRect screen= QApplication::desktop()->screenGeometry();
    ss << (int)(((float)screen.width())*gazeData->left_eye.gaze_point.position_on_display_area.x)
       << ','
       << (int)(((float)screen.height())*gazeData->left_eye.gaze_point.position_on_display_area.y)
       << '\n';
    GazeServer::getGazeServer()->sendGazeData(ss.str().c_str(), ss.str().length()+1);
}


vector<TobiiEyeTracker*> TobiiEyeTracker::tobiiEyeTrackers;

TobiiEyeTracker::TobiiEyeTracker(TobiiResearchEyeTracker* eyetracker){
    this->eyetracker = eyetracker;
    tobiiPro = TobiiPro::getTobiiPro();
    tobiiLibrary = tobiiPro->getTobiiLibrary();
    if(!tobiiLibrary) SetFunctionsToStubs();
    else ExtractTobiiFunctions();
    getTrackerName(eyetracker, &this->name);
    timer = new QTimer(this);
    connect(timer, SIGNAL(timeout()), this, SLOT(retrieveData()));
    gazeServer = GazeServer::getGazeServer();
    QRect screen= QApplication::desktop()->screenGeometry();
    screenWidth = screen.width();
    screenHeight = screen.height();
}

void TobiiEyeTracker::ExtractTobiiFunctions(){
    getTrackerAddress = (GetTrackerAddress *)GetProcAddress(tobiiLibrary,"tobii_research_get_address");
    getTrackerName = (GetTrackerName *)GetProcAddress(tobiiLibrary, "tobii_research_get_device_name");
    getTrackerSerialNumber = (GetTrackerSerialNumber *)GetProcAddress(tobiiLibrary,"tobii_reseach_get_serial_number");
    getTrackerModel = (GetTrackerModel *)GetProcAddress(tobiiLibrary, "tobii_research_get_model");
    getTrackerFirmwareVersion = (GetTrackerFirmwareVersion*)GetProcAddress(tobiiLibrary, "tobii_research_get_firmware_version");
    enterCalibrationMode = (EnterCalibrationMode*)GetProcAddress(tobiiLibrary,"tobii_research_screen_based_calibration_enter_calibration_mode");
    leaveCalibrationMode = (LeaveCalibrationMode*)GetProcAddress(tobiiLibrary,"tobii_research_screen_based_calibration_leave_calibration_mode");
    collectCalibrationData = (CollectCalibrationData*)GetProcAddress(tobiiLibrary,"tobii_research_screen_based_calibration_collect_data");
    discardCalibrationData = (DiscardCalibrationData*)GetProcAddress(tobiiLibrary,"tobii_research_screen_based_calibration_discard_data");
    computeCalibration = (ComputeCalibration*)GetProcAddress(tobiiLibrary,"tobii_research_screen_based_calibration_compute_and_apply");
    subscribeToData = (SubscribeToData*)GetProcAddress(tobiiLibrary,"tobii_research_subscribe_to_gaze_data");
    qDebug() << computeCalibration << " " << leaveCalibrationMode;
}

void TobiiEyeTracker::SetFunctionsToStubs(){

}

void TobiiEyeTracker::enterCalibration(){
    enterCalibrationMode(eyetracker);
}

void TobiiEyeTracker::leaveCalibration(){
    TobiiResearchCalibrationResult* result = 0;
    computeCalibration(eyetracker, &result);
    leaveCalibrationMode(eyetracker);
}

void TobiiEyeTracker::useCalibrationPoint(float x, float y){
    collectCalibrationData(eyetracker, x, y);
}

void TobiiEyeTracker::discardCalibrationPoint(float x, float y){
    discardCalibrationData(eyetracker, x, y);
}

void TobiiEyeTracker::startTracker(){
    subscribeToData(this->eyetracker, gazeDataCallback, &this->gazeData);
    //timer->start(16);
}

char* TobiiEyeTracker::getName(){
    return name;
}

char* TobiiEyeTracker::getAddress(){
    return address;
}

char* TobiiEyeTracker::getSerialNumber(){
    return serialNumber;
}

char* TobiiEyeTracker::getModel(){
    return model;
}

char* TobiiEyeTracker::getFirmwareVersion(){
    return firmwareVersion;
}


void TobiiEyeTracker::retrieveData(){
    stringstream ss;
    QRect screen= QApplication::desktop()->screenGeometry();
    //qDebug() << screen.width() << "," << screen.height();
    ss << (int)(((float)screen.width())*gazeData.left_eye.gaze_point.position_on_display_area.x)
       << ','
       << (int)(((float)screen.width())*gazeData.left_eye.gaze_point.position_on_display_area.y)
       << '\n';
    writer->setScreenRes(screenHeight, screenWidth);
    gazeServer->sendGazeData(ss.str().c_str(), ss.str().length()+1);
}


