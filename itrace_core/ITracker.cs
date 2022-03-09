/********************************************************************************************************************************************************
* @file ITracker.cs
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
    interface ITracker
    {
        void EnterCalibration();
        void LeaveCalibration();
        void StartTracker();
        void StopTracker();
        String GetTrackerName();
        String GetTrackerSerialNumber();
        void ShowEyeStatusWindow();
    }
}
