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
        public string CurrentCalibrationID { get; private set; }
        
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public CalibrationResult CurrentCalibration { get; private set; }

        public string TrackerType { get; private set; }
        public string TrackerSerialNumber { get; private set; }

        public string ScreenRecordingStart { get; private set; }

        private SessionManager() { ScreenRecordingStart = "0"; }

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

            CurrentCalibration = new EmptyCalibrationResult();
        }

        public void StartSession() 
        {
            CurrentSessionID = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public void GenerateCalibrationID()
        {
            CurrentCalibrationID = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); 
        }

        public void SetCalibration(CalibrationResult r)
        {
            CurrentCalibration = r;
        }

        public void SetTrackerData(string trackerType, string trackerSerialNumber)
        {
            TrackerType = trackerType;
            TrackerSerialNumber = trackerSerialNumber;
        }

        public void GenerateScreenRecordingStart()
        {
            ScreenRecordingStart = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }
}
