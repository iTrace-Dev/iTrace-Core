﻿using System;

namespace iTrace_Core
{
    class SessionManager
    {
        static SessionManager instance;
        
        public string StudyName { get; private set; }
        public string ResearcherName { get; private set; }
        public string ParticipantID { get; private set; }
        public string DataRootDir { get; private set; }
        
        // TimeStamp in ticks
        public string CurrentSessionID { get; private set; }

        // Unix UTC TimeStamp
        public string CurrentSessionTimeStamp { get; private set; }

        public string CurrentCalibrationTimeStamp { get; private set; }
        
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

        public void SetScreenDimensions(double width, double height)
        {
            /* 
             * C# provided screen size is double
             *   If conversion causes issues, we can always revert to double 
            */
            ScreenWidth = Convert.ToInt32(width);
            ScreenHeight = Convert.ToInt32(height);
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
            CurrentSessionID = Convert.ToString(DateTime.UtcNow.Ticks);
            CurrentSessionTimeStamp = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public void GenerateCalibrationTimeStamp()
        {
            CurrentCalibrationTimeStamp = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); 
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
