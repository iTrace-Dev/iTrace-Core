#include <xmlwriter.h>
#include <fstream>
#include <sstream>
#include <string>
#include <QDebug>
#include <QString>
#include <iostream>
#include <time.h>
#include <stdio.h>
#include <inttypes.h>
#include <QXmlStreamWriter>
#include <QIODevice>
#include <QtXml/qdom.h>
#include <tobii_research_streams.h>

using namespace std;

void xmlWriter::process(){
   /* QDomDocument document;
    QDomElement root = document.createElement("Response");
    document.appendChild(root);
    QFile file("test.xml");
    if(!file.open(QIODevice::WriteOnly | QIODevice::Text)){
        qDebug() << "Failed to open file for writing!";
    }
    else{
        QTextStream stream(&file);
        stream << document.toString();
        file.close();
        qDebug() << "Finshed";
    }*/

}

void xmlWriter::setFile()
{
    fs.open("test.xml"/*,ios::out*/);
    if(!fs){
        qDebug() << "File not loaded" << endl;
    }
    xmlSetup();
    //process();
}
void xmlWriter::xmlSetup()
{
    fs << "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" << endl;
    fs << "<iTrace>" << endl;
    fs << "<environment>" << endl;
}
void xmlWriter::writeEnvironment(){
    tracker_name = TobiiEyeTracker::tobiiEyeTrackers[0]->getName();
    //serial = TobiiEyeTracker::tobiiEyeTrackers[0]->getSerialNumber();
    //model = TobiiEyeTracker::tobiiEyeTrackers[0]->getModel();
    //version = TobiiEyeTracker::tobiiEyeTrackers[0]->getFirmwareVersion();
    fs << "       <eye-tracker name=" << tracker_name << "/>" << endl;
    fs << "         <date & time = " << currentDateTime() << "/>" << endl;
    fs << "         <session-id" << "/>" << endl;
    fs << "</environment>" << endl;
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
    int64_t trackerTime = 0;
    int64_t systemTime = 0;
    int i=0;
    while(data[i] != ',') i++;
    string xstring = ss.str().substr(0,i);
    newx = stoi(xstring);
    if(newx < 0) return;
    string ystring = ss.str().substr(i+1);
    newy= stoi(ystring);
    if (newy < 0) return;

   dmeter = pupilData.diameter;
   system_time = gazeData.system_time_stamp;
   tracker_time = gazeData.device_time_stamp;

    fs <<"response <y=\"" << ystring << "\" x=\"" << xstring << "\"";
    fs << "   left validation=\"" << "\"     right validation=\"" << "\"";
    fs << "   tracker time=\"" << tracker_time << "\"";
    fs << "   system time=\"" << system_time <<  "\"";
    fs << "   left-pupil diameter=\"" << dmeter << "\"";
    fs << "   right-pupil diameter=\"  />";
    fs << endl;
}

void xmlWriter::closeFile()
{
    fs << "</itrace>" << endl;
    fs.close();
}

const std::string xmlWriter::currentDateTime(){
    time_t now = time(0);
    struct tm tstruct;
    char buf[80];
    tstruct = *localtime(&now);
    strftime(buf,sizeof(buf), "%Y-%m-%d.%X", &tstruct);
    return buf;
}


