﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        static SessionManager GetInstance()
        {
            if (instance == null)
                instance = new SessionManager();

            return instance;
        }

        void SetupSession(string study, string researcher, string participant, string dataRoot)
        {
            StudyName = study;
            ResearcherName = researcher;
            ParticipantID = participant;
            DataRootDir = dataRoot;
                
            CurrentStudyDir = DataRootDir + "\\" + StudyName + "\\" + ParticipantID;
        }

        void StartSession() 
        {
            CurrentSessionID = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            CurrentSessionDir = CurrentStudyDir + "\\" + CurrentSessionID;

            // Create path?
            //
        }

        void GenerateCalibrationID()
        {
            CurrentCalibrationID = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); 

            // Create the path?
            //
        }
    }
}