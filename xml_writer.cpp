#include <QDir>
#include <QString>
#include <ctime>
#include <cstdint> //provides int64_t
#include "xml_writer.hpp"
#include "gaze_data.hpp"
#include "session_manager.hpp"

XMLWriter::XMLWriter(QObject *parent): QObject(parent) {
    SessionManager& session = SessionManager::Instance();
    outputFile.setFileName(QString::fromStdString(session.getSessionPath() + QDir::separator().toLatin1() + "core_" + session.getSessionID() + ".xml"));
    outputFile.open(QIODevice::WriteOnly);
    writer.setDevice(&outputFile);
    writer.setAutoFormatting(true); //Human readable formatting (can disable later)

    writer.writeStartDocument();
    writer.writeStartElement("core");
}

void XMLWriter::setEnvironment(const std::string& trackerID) {
    SessionManager& session = SessionManager::Instance();

    writer.writeStartElement("environment");

    writer.writeEmptyElement("screen-size");
    writer.writeAttribute("width", QString::number(session.getScreenWidth()));
    writer.writeAttribute("height", QString::number(session.getScreenHeight()));

    writer.writeEmptyElement("eye-tracker");
    writer.writeAttribute("type", QString::fromStdString(trackerID));

    std::string startDateTime(std::to_string(std::time(nullptr)));
    writer.writeTextElement("date", QString::fromStdString(startDateTime));
    writer.writeTextElement("time", QString::fromStdString(startDateTime));

    writer.writeEmptyElement("session");
    writer.writeAttribute("id", QString::fromStdString(session.getSessionID()));
    writer.writeEmptyElement("calibration");
    writer.writeAttribute("id", QString::fromStdString(session.getCalibrationID()));

    writer.writeEndElement(); //Close "environment"
}

void XMLWriter::writeResponse(GazeData gaze) {
    writer.writeEmptyElement("response");
    writer.writeAttribute("x", QString::number(gaze.getCalculatedX()));
    writer.writeAttribute("y", QString::number(gaze.getCalculatedY()));
    writer.writeAttribute("left_x", QString::number(gaze.leftX));
    writer.writeAttribute("left_y", QString::number(gaze.leftY));
    writer.writeAttribute("left_pupil_diameter", QString::number(gaze.leftDiameter));
    writer.writeAttribute("left_validation", QString::number(gaze.leftValidity));
    writer.writeAttribute("right_x", QString::number(gaze.rightX));
    writer.writeAttribute("right_y", QString::number(gaze.leftY));
    writer.writeAttribute("right_pupil_diameter", QString::number(gaze.rightDiameter));
    writer.writeAttribute("right_validation", QString::number(gaze.rightValidity));
    writer.writeAttribute("tracker_time", QString::number(gaze.trackerTime));
    writer.writeAttribute("system_time", QString::number(gaze.systemTime));
    writer.writeAttribute("event_time", QString::number(gaze.eventTime));
    writer.writeAttribute("fixation_id", QString("NA"));
}

XMLWriter::~XMLWriter() {
    writer.writeEndDocument();
    outputFile.close();
}
