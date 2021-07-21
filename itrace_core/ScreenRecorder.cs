using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
//using SharpAvi;
//using SharpAvi.Codecs;
//using SharpAvi.Output;
using System.Windows.Forms;
using System.Collections.Generic;

using Accord.Video.FFMPEG;


//using static iTrace_Core.RecorderParams;

namespace iTrace_Core
{
	// Used to retrieve Mouse Info
	public static class MouseCursor
	{
		public const Int32 CURSOR_SHOWING = 0x00000001;

		[StructLayout(LayoutKind.Sequential)]
		public struct ICONINFO
		{
			public bool fIcon;
			public Int32 xHotspot;
			public Int32 yHotspot;
			public IntPtr hbmMask;
			public IntPtr hbmColor;
		}
		

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public Int32 x;
			public Int32 y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CURSORINFO
		{
			public Int32 cbSize;
			public Int32 flags;
			public IntPtr hCursor;
			public POINT ptScreenPos;
		}

		[DllImport("user32.dll")]
		public static extern bool GetCursorInfo(out CURSORINFO pci);

		[DllImport("user32.dll")]
		public static extern IntPtr CopyIcon(IntPtr hIcon);

		[DllImport("user32.dll")]
		public static extern bool DrawIcon(IntPtr hdc, int x, int y, IntPtr hIcon);

		[DllImport("user32.dll")]
		public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);
	}

	// Used to Configure the Recorder
	public class RecorderParams
	{
		public RecorderParams(string filename, int FrameRate, int Quality)
		{
			FileName = filename;
			FramesPerSecond = FrameRate;
			this.Quality = Quality;

			Height = Screen.PrimaryScreen.Bounds.Height;
			Width = Screen.PrimaryScreen.Bounds.Width;

		}

		public string FileName;
		public int FramesPerSecond, Quality;
		//FourCC Codec;

		public int Height { get; private set; }
		public int Width { get; private set; }

		public VideoFileWriter CreateAviWriter()
		{

			VideoFileWriter writer = new VideoFileWriter();
			//writer.Open(FileName, Width, Height, FramesPerSecond, VideoCodec.MPEG4);
			return writer;
		}

		/*public IAviVideoStream CreateVideoStream(AviWriter writer)
		{
			// Select encoder type based on FOURCC of codec
			if (Codec == KnownFourCCs.Codecs.Uncompressed)
				return writer.AddUncompressedVideoStream(Width, Height);
			else if (Codec == KnownFourCCs.Codecs.MotionJpeg)
				return writer.AddMotionJpegVideoStream(Width, Height, Quality);
			else
			{
				return writer.AddMpeg4VideoStream(Width, Height, (double)writer.FramesPerSecond,
					// It seems that all tested MPEG-4 VfW codecs ignore the quality affecting parameters passed through VfW API
					// They only respect the settings from their own configuration dialogs, and Mpeg4VideoEncoder currently has no support for this
					quality: Quality,
					codec: Codec,
					// Most of VfW codecs expect single-threaded use, so we wrap this encoder to special wrapper
					// Thus all calls to the encoder (including its instantiation) will be invoked on a single thread although encoding (and writing) is performed asynchronously
					forceSingleThreadedAccess: true);
			}
		}*/
	}

	public class Recorder : IDisposable
	{

		#region Fields
		VideoFileWriter writer;
		RecorderParams Params;
		//IAviVideoStream videoStream;
		Thread screenThread;
		ManualResetEvent stopThread = new ManualResetEvent(false);
		int count;
		Queue<Bitmap> queue;
		#endregion

		public Recorder(RecorderParams Params)
		{
			this.Params = Params;
			count = 0;
			queue = new Queue<Bitmap>();
			// Create AVI writer and specify FPS
			writer = Params.CreateAviWriter();
			writer.Open(Params.FileName, Params.Width, Params.Height, Params.FramesPerSecond, VideoCodec.MPEG4);
			//Console.WriteLine(writer.FramesPerSecond);

			screenThread = new Thread(RecordScreen)
			{
				Name = typeof(Recorder).Name + ".RecordScreen",
				IsBackground = true
			};

			screenThread.Start();
		}

		public void Dispose()
		{
			stopThread.Set();
			screenThread.Join();

			// Close writer: the remaining data is written to a file and file is closed
			writer.Close();

			stopThread.Dispose();
		}

		void RecordScreen()
		{
			Console.WriteLine(Params.FramesPerSecond);
			var frameInterval = TimeSpan.FromSeconds(1 / Params.FramesPerSecond); // FPS
			
			Task videoWriteTask = null;
			var timeTillNextFrame = TimeSpan.Zero;

			

			while (!stopThread.WaitOne(timeTillNextFrame))
			{
				var buffer = new byte[Params.Width * Params.Height * 4];
				var timeStamp = DateTime.Now;

				Screenshot();

				timeTillNextFrame = timeStamp + frameInterval - DateTime.Now;
				if (timeTillNextFrame < TimeSpan.Zero)
					timeTillNextFrame = TimeSpan.Zero;
			}
			while (queue.Count != 0)
			{	
				// TODO - Make Asychronous and insert in above loop
				var insert = queue.Dequeue();
				writer.WriteVideoFrame(insert);
				insert.Dispose();
			}

			// Wait for the last frame is written
			videoWriteTask?.Wait();
		}

		public void Screenshot()
		{
			var BMP = new Bitmap(Params.Width, Params.Height, PixelFormat.Format32bppRgb);
			using (var g = Graphics.FromImage(BMP))
			{
				g.CopyFromScreen(Point.Empty, Point.Empty, new Size(Params.Width, Params.Height), CopyPixelOperation.SourceCopy);
				MouseCursor.CURSORINFO cursorInfo;
				cursorInfo.cbSize = Marshal.SizeOf(typeof(MouseCursor.CURSORINFO));
				if (MouseCursor.GetCursorInfo(out cursorInfo))
				{
					if (cursorInfo.flags == MouseCursor.CURSOR_SHOWING)
					{
						var iconPointer = MouseCursor.CopyIcon(cursorInfo.hCursor);
						MouseCursor.ICONINFO iconInfo;
						int iconX, iconY;

						if (MouseCursor.GetIconInfo(iconPointer, out iconInfo))
						{
							iconX = cursorInfo.ptScreenPos.x - ((int)iconInfo.xHotspot);
							iconY = cursorInfo.ptScreenPos.y - ((int)iconInfo.yHotspot);

							MouseCursor.DrawIcon(g.GetHdc(), iconX, iconY, cursorInfo.hCursor);
							g.ReleaseHdc();
						}
					}
				}
				g.Flush();		
			}
			queue.Enqueue(BMP);
		}
	}
}
