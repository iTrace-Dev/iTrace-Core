#ifndef SESSION_MANAGER_HPP
#define SESSION_MANAGER_HPP

#include <string>

class SessionManager {
    public:
        SessionManager();

        void setStudyName(const std::string& study) { studyName = study; }
        void setResearcherName(const std::string& researcher) { researcherName = researcher; }
        void setParticipantID(const std::string& participant) { participantID = participant; }
        void setDataRootDirectory(const std::string& dataRoot) { dataRootDir = dataRoot; }

        std::string getCurrentSessionPath();

    private:
        std::string studyName;
        std::string researcherName;
        std::string participantID;
        std::string dataRootDir;
        std::string currentSessionDir;
};

#endif // SESSION_MANAGER_HPP
