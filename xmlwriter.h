#ifndef XMLWRITER_H
#define XMLWRITER_H

#include <fstream>
#include <string>
#include <iostream>
#include <QString>
#include <QXmlStreamWriter>
#include <QFile>
#include <tobiiEyetracker.h>
#include <tobii_research_streams.h>

using namespace std;

class xmlWriter{

public:
    ofstream fs;
    void setFile();
    void closeFile();
    void writeGaze(char*);
    void writeResponse(char*);
    void writeEnvironment();
    void setScreenRes(float ,float );
    void setTrackerName(char*);
    void writeSessionTime(char*);
    const string currentDateTime();
    void process();

private:
    void xmlSetup();
    char *tracker_name;
    char *serial;
    char *model;
    char *version;
    float screen_height;
    float screen_width;
    float dmeter;
    int64_t system_time;
    int64_t tracker_time;
    TobiiResearchPupilData pupilData;
    TobiiResearchEyeData left_eye;
    TobiiResearchEyeData right_eye;
    TobiiResearchEyeData* currEye;
    TobiiResearchGazeData gazeData;

};
#endif // XMLWRITER_H
