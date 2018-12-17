using System;

namespace iTrace_Core
{
    class SessionManager
    {
        static SessionManager instance;
        
        public string StudyName { get; private set; }
        public string ResearcherName { get; private set; }
        public string ParticipantID { get; private set; }
        public string DataRootDir { get; private set; }
        
        // Timestamp generated at the start of each tracking session
        public string CurrentSessionID { get; private set; }

        // Calculated Paths
        public string CurrentStudyDir { get; private set; }
        public string CurrentSessionDir { get; private set; }
        
        public string CurrentCalibrationID { get; private set; }
        
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

                
        private SessionManager() { }

        public static SessionManager GetInstance()
        {
            if (instance == null)
                instance = new SessionManager();

            return instance;
        }

        public void SetupSession(string study, string researcher, string participant, string dataRoot)
        {
            StudyName = study;
            ResearcherName = researcher;
            ParticipantID = participant;
            DataRootDir = dataRoot;
                
            CurrentStudyDir = DataRootDir + "\\" + StudyName + "\\" + ParticipantID;
        }

        public void StartSession() 
        {
            CurrentSessionID = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            CurrentSessionDir = CurrentStudyDir + "\\" + CurrentSessionID;

            // Create path?
            //
        }

        public void GenerateCalibrationID()
        {
            CurrentCalibrationID = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); 

            // Create the path?
            //
        }
    }
}
