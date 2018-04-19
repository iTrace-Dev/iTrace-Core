#ifndef SESSION_MANAGER_HPP
#define SESSION_MANAGER_HPP

#include <QDir>
#include <string>
#include <QDebug>

class SessionManager {

    public:
        static SessionManager& Instance();

        void sessionSetup(const std::string& study, const std::string& researcher,
                          const std::string& participant, const std::string& dataRoot);

        void generateCalibrationID();

        void startSession();

        std::string getSessionPath() const { return currentSessionDir; }
        std::string getStudyPath() const { return currentStudyDir; }
        std::string getSessionID() const { return currentSessionID; }
        std::string getCalibrationID() const { return currentCalibrationID; }

    private:
        // Singleton Protection
        SessionManager()=default;
        ~SessionManager()=default;
        SessionManager(SessionManager const&)=delete;
        SessionManager& operator=(SessionManager const&)=delete;

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

        // Current Calibration Infrmation
        std::string currentCalibrationID;

};

#endif // SESSION_MANAGER_HPP
