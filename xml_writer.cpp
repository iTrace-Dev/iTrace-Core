#include "xml_writer.hpp"
#include "gaze_data.hpp"
#include <QString>
#include <chrono>
#include <ctime>

XMLWriter::XMLWriter(QObject *parent): QObject(parent) {
        outputFile.setFileName("test.xml");
        outputFile.open(QIODevice::WriteOnly);
        writer.setDevice(&outputFile);
        writer.setAutoFormatting(true); //Human readable formatting (can disable later)

        writer.writeStartDocument();
        writer.writeStartElement("core");
}

void XMLWriter::setEnvironment(const std::string& trackerID) {
        writer.writeStartElement("environment");

        writer.writeEmptyElement("eye-tracker");
        writer.writeAttribute("type", QString::fromStdString(trackerID));

        std::time_t t = std::time(nullptr);
        std::string startDateTime(ctime(&t));
        writer.writeTextElement("date", QString::fromStdString(startDateTime));
        writer.writeTextElement("time", QString::fromStdString(startDateTime));

        // Still need to get these
        writer.writeEmptyElement("session-id");
        writer.writeEmptyElement("calibration");


        writer.writeEndElement(); //Close "environment"
}

void XMLWriter::writeResponse(GazeData gaze) {
    writer.writeEmptyElement("response");
    writer.writeAttribute("x", QString::number(gaze.leftX));
    writer.writeAttribute("y", QString::number(gaze.leftY));
}

XMLWriter::~XMLWriter() {
    writer.writeEndDocument();
    outputFile.close();
}
