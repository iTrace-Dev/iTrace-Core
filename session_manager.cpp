#include "session_manager.hpp"

void SessionManager::sessionSetup(const std::string& study, const std::string& researcher,
                  const std::string& participant, const std::string& dataRoot) {
    studyName = study;
    researcherName = researcher;
    participantID = participant;
    dataRootDir = dataRoot;

    currentStudyDir = dataRootDir + QDir::separator().toLatin1() +
                  studyName + QDir::separator().toLatin1() +
                  participantID;
}

void SessionManager::startSession() {
    currentSessionID = std::to_string(long(std::time(nullptr)));
    currentSessionDir = currentStudyDir + QDir::separator().toLatin1() + currentSessionID;
    QDir dir;
    dir.mkpath(QString::fromStdString(currentStudyDir + QDir::separator().toLatin1() + "calibration"));
    dir.mkpath(QString::fromStdString(currentSessionDir));
}


