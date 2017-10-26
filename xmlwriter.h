#ifndef XMLWRITER_H
#define XMLWRITER_H

#include <fstream>
#include <string>
#include <iostream>


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

private:
    void xmlSetup();
    char *tracker_name;
    float screen_height;
    float screen_width;
};
#endif // XMLWRITER_H
