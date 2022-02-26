using System;
using System.Collections.Generic;
using System.IO;


namespace iTrace_Core
{
    public class ComputerEventReader
    {
        string filename;
        StreamReader streamReader;
        EventFactoryMap eventFactory;
        List<string> events;
        int current;

        bool skipPause = false;

        public ComputerEventReader(string filename, EventFactoryMap eventFactoryMap)
        {
            this.filename = filename;
            streamReader = new StreamReader(filename);
            eventFactory = eventFactoryMap;
            events = new List<string>();
            string line;
            while((line = streamReader.ReadLine()) != null)
			{
                events.Add(line);
			}
            current = 0;
            streamReader.Close();
        }

        public bool Finished()
        {
            //return streamReader.EndOfStream;
            return current == events.Count;
        }

        public ComputerEvent ReadEvent()
        {
            //string line = streamReader.ReadLine();
            string line = events[current];

            ComputerEvent result = eventFactory.GenerateFromCSV(line);
            if(skipPause)
			{
                skipPause = false;
                //result.PauseStrategy = new FixedLengthPause(0);
                result.PauseStrategy = new EmptyPause();
            }


            // If the event is a left mouse click, we need to check for a double click
            if(result.Serialize().Contains("LeftMouseDown,") && current < events.Count - 3) 
            {
                // Check the next 2 (3?) events to see if a double click is possible
                string check1 = events[current + 1],
                       check2 = events[current + 2];

                if (check1.Contains("LeftMouseUp,") && check2.Contains("LeftMouseDown,"))
				{
                    // Next, make sure the last mouse event happened within the click interval
                    long changeInNS = (long.Parse(check2.Split(',')[1]) - long.Parse(events[current].Split(',')[1])) * 100;
                    long doubleClickTimeInNS = System.Windows.Forms.SystemInformation.DoubleClickTime * 1000000;
                    if(changeInNS <= doubleClickTimeInNS)
					{
                        skipPause = true;
                        //result.PauseStrategy = new FixedLengthPause(0);
                        result.PauseStrategy = new EmptyPause();
					}
				}
            }

            ++current;
            return result;
        }

        public void Close()
        {
            streamReader.Close();
        }
    }
}
