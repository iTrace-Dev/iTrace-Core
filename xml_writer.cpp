#include <QDir>
#include <QString>
#include <ctime>
#include "xml_writer.hpp"
#include "gaze_data.hpp"
#include "session_manager.hpp"

XMLWriter::XMLWriter(QObject *parent): QObject(parent) {
    SessionManager& session = SessionManager::Instance();
    outputFile.setFileName(QString::fromStdString(session.getSessionPath() + QDir::separator().toLatin1() + "session_" + session.getSessionID() + ".xml"));
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
    std::string startDateTime(std::to_string(long(t)));
    writer.writeTextElement("date", QString::fromStdString(startDateTime));
    writer.writeTextElement("time", QString::fromStdString(startDateTime));

    SessionManager& session = SessionManager::Instance();
    writer.writeEmptyElement("session");
    writer.writeAttribute("id", QString::fromStdString(session.getSessionID()));
    writer.writeEmptyElement("calibration");
    writer.writeAttribute("id", QString::fromStdString(session.getCalibrationID()));

    writer.writeEndElement(); //Close "environment"
}

void XMLWriter::writeResponse(GazeData gaze) {
    writer.writeEmptyElement("response");
    writer.writeAttribute("x", QString::number(gaze.leftX));
    writer.writeAttribute("y", QString::number(gaze.leftY));
    writer.writeAttribute("left_validation", QString::number(gaze.leftValidity));
    writer.writeAttribute("right_validation", QString::number(gaze.rightValidity));
    writer.writeAttribute("tracker_time", QString::number(gaze.trackerTime));
    writer.writeAttribute("system_time", QString::number(gaze.systemTime));
}

XMLWriter::~XMLWriter() {
    writer.writeEndDocument();
    outputFile.close();
}
