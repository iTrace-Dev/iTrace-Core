/********************************************************************************************************************************************************
* @file SessionManager.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System;

namespace iTrace_Core
{
    class SessionManager
    {
        static SessionManager instance;

        public string TaskName { get; private set; }
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
        public string CalibratedTrackerID { get; private set; }

        public string TrackerType { get; private set; }
        public string TrackerSerialNumber { get; private set; }

        public string ScreenRecordingStart { get; private set; }

        public bool Active { get; private set; }

        private SessionManager() { ScreenRecordingStart = "0"; Active = false; }

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

        public void SetupSession(string task, string researcher, string participant, string dataRoot)
        {
            TaskName = task;
            ResearcherName = researcher;
            ParticipantID = participant;
            DataRootDir = dataRoot;

            if (CalibratedTrackerID == "")
                CurrentCalibration = new EmptyCalibrationResult();
        }

        public void StartSession()
        {
            CurrentSessionID = Convert.ToString(DateTime.UtcNow.Ticks);
            CurrentSessionTimeStamp = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            Active = true;
        }

        public void StopSession()
        {
            Active = false;
        }

        public void GenerateCalibrationTimeStamp()
        {
            CurrentCalibrationTimeStamp = Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        public void SetCalibration(CalibrationResult r, ITracker calibratedTracker)
        {
            CurrentCalibration = r;
            CalibratedTrackerID = calibratedTracker.GetTrackerName() + calibratedTracker.GetTrackerSerialNumber();
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

        public void ClearCalibration()
        {
            CurrentCalibration = new EmptyCalibrationResult();
            CurrentCalibrationTimeStamp = "0";
            CalibratedTrackerID = "";
        }

        public string Serialize()
        {
            return "session_start," + CurrentSessionID + "," + CurrentSessionTimeStamp + "," + DataRootDir + '\n';
        }
    }
}
