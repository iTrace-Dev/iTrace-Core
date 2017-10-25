#ifndef XMLWRITER_H
#define XMLWRITER_H

#include <fstream>


using namespace std;

class xmlWriter{

public:
    ofstream fs;
    void setFile();
    void closeFile();
    void writeGaze(char*);
    void writeResponse(char*);
    void writeEnvironment();
    void writeSessionTime(char*);

private:
    void xmlSetup();
};
#endif // XMLWRITER_H
