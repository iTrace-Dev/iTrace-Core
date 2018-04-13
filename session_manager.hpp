#ifndef SESSION_MANAGER_HPP
#define SESSION_MANAGER_HPP

#include <QDir>
#include <ctime>
#include <string>
#include <QDebug>

class SessionManager {
    public:
        SessionManager() {}

        void sessionSetup(const std::string& study, const std::string& researcher,
                          const std::string& participant, const std::string& dataRoot);

        void startSession();

        std::string getSessionPath() const { return currentSessionDir; }
        std::string getStudyPath() const { return currentStudyDir; }
        std::string getSessionID() const { return currentSessionID; }

    private:
        // Collected from Session Window
        std::string studyName;
        std::string researcherName;
        std::string participantID;
        std::string dataRootDir;

        // Current Session (timestamp)
        //   Generated at the start of each tracking session
        std::string currentSessionID;

        // Calculated Paths
        std::string currentStudyDir;
        std::string currentSessionDir;
};

#endif // SESSION_MANAGER_HPP
