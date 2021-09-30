using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    public class EventRecorder
    {
        public bool IsRecordInProgress { get; private set; }

        KeyboardHook keyPressListener = new KeyboardHook();
        MouseHook mouseListener = new MouseHook();
        CoreClient gazeListener = new CoreClient();
        ComputerEventWriter eventWriter;

        public EventRecorder(ComputerEventWriter writer)
        {
            eventWriter = writer;

            mouseListener.OnMouseEvent += ComputerEventListener;
            mouseListener.HookMouse();

            keyPressListener.OnKeyboardEvent += ComputerEventListener;
            keyPressListener.HookKeyboard();
            
            gazeListener.OnCoreMessage += ComputerEventListener;
        }

        private void ComputerEventListener(object sender, ComputerEvent e)
        {
            if (IsRecordInProgress)
                eventWriter.Write(e);
        }

        public void StartRecording()
        {
            IsRecordInProgress = true;
        }

        public void StopRecording()
        {
            IsRecordInProgress = false;
        }

        public void ConnectToCore()
        {
            gazeListener.Connect();
        }

        public void Dispose()
        {
            keyPressListener.Dispose();
            mouseListener.Dispose();
            eventWriter.Close();
        }
    }
}
