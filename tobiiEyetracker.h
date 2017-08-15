#ifndef TOBIIEYETRACKER_H
#define TOBIIEYETRACKER_H
#include "tobii_research.h"
#include "tobii_research_eyetracker.h"
#include "tobii_research_streams.h"
#include "tobiipro.h"
#include "tracker.h"
#include <windows.h>
#include <vector>
#include <QTimer>
#include "gazeserver.h"

using namespace std;

class TobiiEyeTracker: Tracker
{
    Q_OBJECT
    TobiiPro* tobiiPro;
    HMODULE tobiiLibrary;
    void ExtractTobiiFunctions();
    void SetFunctionsToStubs();
    TobiiResearchEyeTracker* eyetracker;
    GetTrackerAddress* getTrackerAddress;
    GetTrackerSerialNumber* getTrackerSerialNumber;
    GetTrackerName* getTrackerName;
    GetTrackerModel* getTrackerModel;
    GetTrackerFirmwareVersion* getTrackerFirmwareVersion;
    char* name;
    char* address;
    char* serialNumber;
    char* model;
    char* firmwareVersion;
    EnterCalibrationMode* enterCalibrationMode;
    LeaveCalibrationMode* leaveCalibrationMode;
    CollectCalibrationData* collectCalibrationData;
    DiscardCalibrationData* discardCalibrationData;
    ComputeCalibration* computeCalibration;
    SubscribeToData* subscribeToData;
    QTimer* timer;
    GazeServer* gazeServer;
    float screenWidth;
    float screenHeight;
public:
    static vector<TobiiEyeTracker*> tobiiEyeTrackers;
    TobiiEyeTracker(TobiiResearchEyeTracker* eyetracker);
    void enterCalibration() override;
    void leaveCalibration() override;
    void useCalibrationPoint(float x, float y) override;
    void discardCalibrationPoint(float x, float y) override;
    void startTracker() override;
    char* getName();
    char* getAddress();
    char* getSerialNumber();
    char* getModel();
    char* getFirmwareVersion();
    TobiiResearchGazeData gazeData;

public slots:
    void retrieveData();
};

#endif // TOBIIEYETRACKER_H
