#include <xmlwriter.h>
#include <fstream>
#include <sstream>
#include <string>
#include <QDebug>
#include <iostream>
#include <time.h>
#include <stdio.h>

using namespace std;
void xmlWriter::setFile()
{
    fs.open("testXML.xml"/*,ios::out*/);
    if(!fs){
        qDebug() << "File not loaded" << endl;
    }
    xmlSetup();
}
void xmlWriter::xmlSetup()
{
    fs << "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" << endl;
    fs << "<iTrace>" << endl;
    fs << "<environment>" << endl;
}
void xmlWriter::writeEnvironment(){
    //fs << "       <eye-tracker name=" << tracker_name <<  "/>" << endl;
    fs << "         <date & time = " << currentDateTime() << "/>" << endl;
    fs << "         <session-id" << "/>" << endl;
    fs << "</environment>" << endl;
    fs << "<response>" << endl;
}
void xmlWriter::setScreenRes(float x, float y){
    screen_height = x;
    screen_width = y;
}

void xmlWriter::writeResponse(char * data){
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
    fs <<"<y=\"" << newy << "\" x=\"" << newx << "\"";
    fs << "   left validation=\"" << "\"     right validation=\"" << "\"";
    fs << "   tracker time=\"" << "\"";
    fs << "   system time=\"" << "\"";
    fs << "   left-pupil diameter=\"";
    fs << "   right-pupil diameter=\"  />";
    fs << endl;
}

void xmlWriter::closeFile()
{
    fs << "</response>" << endl;
    fs << "</itrace>" << endl;

    fs.close();
}




/*void xmlWriter::writeGaze(char * data)
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


    //fs <<"<gaze";
    fs <<" y=\"" << newy << "\" x=\"" << newx << "\">" << endl;
    //fs <<"</gaze>" << endl;
}*/

void xmlWriter::setTrackerName(char *t){
    tracker_name=t;
}

const std::string xmlWriter::currentDateTime(){
    time_t now = time(0);
    struct tm tstruct;
    char buf[80];
    tstruct = *localtime(&now);
    strftime(buf,sizeof(buf), "%Y-%m-%d.%X", &tstruct);
    return buf;
}


