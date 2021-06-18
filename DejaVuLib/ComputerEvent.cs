using System;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;



namespace DejaVuLib
{
    public abstract class ComputerEvent : EventArgs
    {
        public IPauseStrategy PauseStrategy { get; set; }

        protected ComputerEvent()
        {
            EventTime = PreciseSystemTime.GetTime();
        }

        public long EventTime { get; protected set; }

        public abstract void Replay();
        public abstract string Serialize();
        public abstract void DeserializeFrom(string msg);

        public void Pause()
        {
            if (PauseStrategy != null)
            {
                PauseStrategy.Pause(this);
            }
        }
    }

    public class KeyDown : ComputerEvent
    {
        private byte virtualKeyCode;

        public KeyDown() { }

        public KeyDown(byte vk) : base()
        {
            virtualKeyCode = vk;
        }

        public override void Replay()
        {
            Win32.keybd_event(virtualKeyCode, 0, 0, 0);
        }

        public override string Serialize()
        {
            return "KeyDown," + EventTime.ToString() + "," + virtualKeyCode + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
            virtualKeyCode = Convert.ToByte(split[2]);
        }
    }

    public class KeyUp : ComputerEvent
    {
        byte virtualKeyCode;

        public KeyUp() { }

        public KeyUp(byte vk) : base()
        {
            virtualKeyCode = vk;
        }

        public override void Replay()
        {
            Win32.keybd_event(virtualKeyCode, 0, Win32.KEYEVENTF_KEYUP, 0);
        }

        public override string Serialize()
        {
            return "KeyUp," + EventTime.ToString() + "," + virtualKeyCode + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
            virtualKeyCode = Convert.ToByte(split[2]);
        }
    }

    public class LeftMouseDown : ComputerEvent
    {
        public LeftMouseDown() : base() { }

        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "LeftMouseDown," + EventTime.ToString() + "\n";
        }
        
        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
        }
    }

    public class LeftMouseUp : ComputerEvent
    {
        public LeftMouseUp() : base() { }


        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "LeftMouseUp," + EventTime.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
        }
    }

    public class RightMouseDown : ComputerEvent
    {
        public RightMouseDown() : base() { }

        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "RightMouseDown," + EventTime.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
        }
    }

    public class RightMouseUp : ComputerEvent
    {
        public RightMouseUp() : base() { }

        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "RightMouseUp," + EventTime.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
        }
    }

    public class MiddleMouseDown : ComputerEvent
    {
        public MiddleMouseDown() : base() { }

        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "MiddleMouseDown," + EventTime.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
        }
    }

    public class MiddleMouseUp : ComputerEvent
    {
        public MiddleMouseUp() : base() { }

        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "MiddleMouseUp," + EventTime.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
        }
    }

    public class ForwardMouseDown : ComputerEvent
    {
        public ForwardMouseDown() : base() { }

        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_XDOWN, 0, 0, Win32.XBUTTON2, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "ForwardMouseDown," + EventTime.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
        }
    }

    public class ForwardMouseUp : ComputerEvent
    {
        public ForwardMouseUp() : base() { }

        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_XUP, 0, 0, Win32.XBUTTON2, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "ForwardMouseUp," + EventTime.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
        }
    }


    public class BackMouseDown : ComputerEvent
    {
        public BackMouseDown() : base() { }

        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_XDOWN, 0, 0, Win32.XBUTTON1, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "BackMouseDown," + EventTime.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
        }
    }

    public class BackMouseUp : ComputerEvent
    {
        public BackMouseUp() : base() { }

        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_XUP, 0, 0, Win32.XBUTTON1, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "BackMouseUp," + EventTime.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
        }
    }

    public class MouseMove : ComputerEvent
    {
        int x;
        int y;

        public MouseMove() { }

        public MouseMove(int newX, int newY) : base()
        {
            x = (newX < 0) ? -newX : newX;
            y = (newY < 0) ? -newY : newY;
        }
        public override void Replay()
        {
            float dpiX, dpiY;
            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
            dpiX = graphics.DpiX;
            dpiY = graphics.DpiY;

            uint defaultDPI = 96; // Considered default screen DPI? Check this
            //mouse_event wants position to be specified in a weird way - also need to account for screen's DPI
            uint computedX = Convert.ToUInt32((x / System.Windows.SystemParameters.PrimaryScreenWidth) * 65536.0 * (defaultDPI / dpiX));
            uint computedY = Convert.ToUInt32((y / System.Windows.SystemParameters.PrimaryScreenHeight) * 65536.0 * (defaultDPI / dpiY));

            Win32.mouse_event(Win32.MOUSEEVENTF_ABSOLUTE | Win32.MOUSEEVENTF_MOVE, computedX, computedY, 0, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "MouseMove," + EventTime.ToString() + "," + x.ToString() + "," + y.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
            x = Convert.ToInt32(split[2]);
            y = Convert.ToInt32(split[3].TrimEnd('\n'));
        }
    }

    public class MouseWheel : ComputerEvent
    {
        int scrollAmount;

        public MouseWheel() { }

        public MouseWheel(uint rawScrollAmount) : base()
        {
            scrollAmount = Convert.ToInt32((rawScrollAmount & 0xffff0000) >> 16);

            if (scrollAmount > SystemInformation.MouseWheelScrollDelta)
                scrollAmount = scrollAmount - (ushort.MaxValue + 1);
        }
        
        public override void Replay()
        {
            Win32.mouse_event(Win32.MOUSEEVENTF_WHEEL, 0, 0, (uint)scrollAmount, (UIntPtr)0);
        }

        public override string Serialize()
        {
            return "MouseWheel," + EventTime.ToString() + "," + scrollAmount.ToString() + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');

            EventTime = Convert.ToInt64(split[1]);
            scrollAmount = Convert.ToInt32(split[2]);
        }
    }

    public class CoreMessage : ComputerEvent
    {
        string message;

        public CoreMessage() { }

        public CoreMessage(string msg) : base()
        {
            message = msg;
        }

        public override void Replay()
        {
            string[] split = message.Split(',');
            string refreshedMessage;

            if (split[0] == "gaze" || split[0] == "session_start")
            {
                refreshedMessage = split[0] + "," + PreciseSystemTime.GetTime().ToString() + "," + split[2] + "," + split[3] + "\n";
            }
            else
            {
                refreshedMessage = message;
            }

            SocketServer.Instance().SendToClients(refreshedMessage);
            WebSocketServer.Instance().Send(refreshedMessage);
        }

        public override string Serialize()
        {
            return message + "\n";
        }

        public override void DeserializeFrom(string msg)
        {
            string[] split = msg.Split(',');
            EventTime = Convert.ToInt64(split[1]);

            message = msg;
        }

        public bool IsSessionStart()
        {
            return message.Split(',')[0] == "session_start";
        }
    }

    public class EmptyComputerEvent : ComputerEvent
    {
        public EmptyComputerEvent() : base() { }

        public override void Replay() { }

        public override string Serialize()
        {
            return "";
        }

        public override void DeserializeFrom(string msg) { }
    }
}
