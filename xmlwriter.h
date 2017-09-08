#ifndef XMLWRITER_H
#define XMLWRITER_H

#include <fstream>


using namespace std;

class xmlWriter{

public:
    fstream fs;
    void setFile();
    void closeFile();
    void writeGaze(char*);

private:
    void xmlSetup();
};
#endif // XMLWRITER_H
