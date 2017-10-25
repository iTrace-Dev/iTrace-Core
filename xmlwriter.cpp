#include <xmlwriter.h>
#include <fstream>
#include <sstream>
#include <string>
#include <QDebug>

using namespace std;
void xmlWriter::setFile()
{
    fs.open("testXML.xml"/*,ios::out*/);
    if(!fs){
        qDebug() << "File not loaded" << endl;
    }
    xmlSetup();
}


void xmlWriter::closeFile()
{

    fs << "</itrace>" << endl;

    fs.close();
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
    fs <<" y=\"" << newy << "\" x=\"" << newx << "\"" << endl;
}

void xmlWriter::writeEnvironment(){

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


void xmlWriter::xmlSetup()
{
    fs << "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" << endl;
    fs << "<iTrace>" << endl;
    fs << "<environment>" << endl;

}
