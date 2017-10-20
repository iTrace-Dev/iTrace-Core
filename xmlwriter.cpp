#include <xmlwriter.h>
#include <fstream>
#include <sstream>
#include <string>

using namespace std;



void xmlWriter::setFile()
{
    fs.open("data/testXML.xml",ios::out);
    xmlSetup();
}


void xmlWriter::closeFile()
{
    fs << "</gazes>" << endl;
    fs << "</itrace-records>" << endl;
    fs.close();
}

void xmlWriter::writeGaze(char * data)
{
    std::stringstream ss;
    ss << data;
    int newx;
    int newy;
    int i=0;
    while(data[i] != ',') i++;
    string xstring = ss.str().substr(0,i);
    newx = stoi(xstring);
    string ystring = ss.str().substr(i+1);
    newy = stoi(ystring);

    // Temporary solution, need to move this to a parser when all the additional data is to be included.


    fs <<"<gaze";
    fs <<" y=\"" << newy << "\" x=\"" << newx << "\">" << endl;
    fs <<"</gaze>" << endl;
}

void xmlWriter::writeSessionTime(char * data)
{

}


void xmlWriter::xmlSetup()
{
    fs << "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" << endl;
    fs << "<environment>" << endl;
    fs << "<tracker-name>" <<endl;
    fs << "<application-type>" <<endl;
    fs << "<itrace-records>" << endl;
    fs << "<gazes>" << endl;
    fs << "<sessiontime>" << endl;
    fs << "<screen-width>" << endl;
    fs << "<screen-height>" << endl;
}
