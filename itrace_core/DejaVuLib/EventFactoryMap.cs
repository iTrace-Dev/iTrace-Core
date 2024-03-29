﻿/********************************************************************************************************************************************************
* @file EventFactoryMap.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System.Collections.Generic;

namespace iTrace_Core
{
    public class EventFactoryMap
    {
        protected Dictionary<string, IEventFactory> factories;

        public ComputerEvent GenerateFromCSV(string line)
        {
            string type = line.Split(',')[0];

            return factories.TryGetValue(type, out var factory) ? factory.Create(line) : new EmptyComputerEvent();
        }
    }

    public class FixedPauseEventFactoryMap : EventFactoryMap
    {
        public FixedPauseEventFactoryMap(int pause)
        {
            FixedLengthPause strategy = new FixedLengthPause(pause);

            factories = new Dictionary<string, IEventFactory>
            {
                ["KeyDown"] = new EventFactory<KeyDown>(strategy),
                ["KeyUp"] = new EventFactory<KeyUp>(strategy),
                ["LeftMouseDown"] = new EventFactory<LeftMouseDown>(strategy),
                ["LeftMouseUp"] = new EventFactory<LeftMouseUp>(strategy),
                ["RightMouseDown"] = new EventFactory<RightMouseDown>(strategy),
                ["RightMouseUp"] = new EventFactory<RightMouseUp>(strategy),
                ["MouseMove"] = new EventFactory<MouseMove>(strategy),
                ["MouseWheel"] = new EventFactory<MouseWheel>(strategy),
                ["gaze"] = new EventFactory<CoreMessage>(strategy),
                ["session_start"] = new EventFactory<CoreMessage>(strategy),
                ["session_end"] = new EventFactory<CoreMessage>(strategy),
                ["MiddleMouseDown"] = new EventFactory<MiddleMouseDown>(strategy),
                ["MiddleMouseUp"] = new EventFactory<MiddleMouseUp>(strategy),
                ["ForwardMouseDown"] = new EventFactory<ForwardMouseDown>(strategy),
                ["ForwardMouseUp"] = new EventFactory<ForwardMouseUp>(strategy),
                ["BackMouseDown"] = new EventFactory<BackMouseDown>(strategy),
                ["BackMouseUp"] = new EventFactory<BackMouseUp>(strategy)
            };
        }
    }

    public class BidirectionalCommunicationFactoryMap : EventFactoryMap
    {
        public BidirectionalCommunicationFactoryMap(int pause)
        {
            FixedLengthPause fixedLengthPause = new FixedLengthPause(pause);
            WaitForClientPause waitForClientsPause = new WaitForClientPause();

            factories = new Dictionary<string, IEventFactory>
            {
                ["KeyDown"] = new EventFactory<KeyDown>(fixedLengthPause),
                ["KeyUp"] = new EventFactory<KeyUp>(fixedLengthPause),
                ["LeftMouseDown"] = new EventFactory<LeftMouseDown>(fixedLengthPause),
                ["LeftMouseUp"] = new EventFactory<LeftMouseUp>(fixedLengthPause),
                ["RightMouseDown"] = new EventFactory<RightMouseDown>(fixedLengthPause),
                ["RightMouseUp"] = new EventFactory<RightMouseUp>(fixedLengthPause),
                ["MouseMove"] = new EventFactory<MouseMove>(fixedLengthPause),
                ["MouseWheel"] = new EventFactory<MouseWheel>(fixedLengthPause),
                ["gaze"] = new EventFactory<CoreMessage>(waitForClientsPause),
                ["session_start"] = new EventFactory<CoreMessage>(waitForClientsPause),
                ["session_end"] = new EventFactory<CoreMessage>(waitForClientsPause),
                ["MiddleMouseDown"] = new EventFactory<MiddleMouseDown>(fixedLengthPause),
                ["MiddleMouseUp"] = new EventFactory<MiddleMouseUp>(fixedLengthPause),
                ["ForwardMouseDown"] = new EventFactory<ForwardMouseDown>(fixedLengthPause),
                ["ForwardMouseUp"] = new EventFactory<ForwardMouseUp>(fixedLengthPause),
                ["BackMouseDown"] = new EventFactory<BackMouseDown>(fixedLengthPause),
                ["BackMouseUp"] = new EventFactory<BackMouseUp>(fixedLengthPause)
            };

        }
    }


    public class ProportionalFactoryMap : EventFactoryMap
    {
        public ProportionalFactoryMap(int scale)
        {
            ProportionalLengthPause strategy = new ProportionalLengthPause(scale);

            factories = new Dictionary<string, IEventFactory>
            {
                ["KeyDown"] = new EventFactory<KeyDown>(strategy),
                ["KeyUp"] = new EventFactory<KeyUp>(strategy),
                ["LeftMouseDown"] = new EventFactory<LeftMouseDown>(strategy),
                ["LeftMouseUp"] = new EventFactory<LeftMouseUp>(strategy),
                ["RightMouseDown"] = new EventFactory<RightMouseDown>(strategy),
                ["RightMouseUp"] = new EventFactory<RightMouseUp>(strategy),
                ["MouseMove"] = new EventFactory<MouseMove>(strategy),
                ["MouseWheel"] = new EventFactory<MouseWheel>(strategy),
                ["gaze"] = new EventFactory<CoreMessage>(strategy),
                ["session_start"] = new EventFactory<CoreMessage>(strategy),
                ["session_end"] = new EventFactory<CoreMessage>(strategy),
                ["MiddleMouseDown"] = new EventFactory<MiddleMouseDown>(strategy),
                ["MiddleMouseUp"] = new EventFactory<MiddleMouseUp>(strategy),
                ["ForwardMouseDown"] = new EventFactory<ForwardMouseDown>(strategy),
                ["ForwardMouseUp"] = new EventFactory<ForwardMouseUp>(strategy),
                ["BackMouseDown"] = new EventFactory<BackMouseDown>(strategy),
                ["BackMouseUp"] = new EventFactory<BackMouseUp>(strategy)
            };
        }
    }
}
