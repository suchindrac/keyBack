using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyBack
{
   	public partial class KeyBackForm : Form
        {
		private static IntPtr _hookID = IntPtr.Zero;
		private static String keysHooked = String.Empty;
        
        	private static string win_name = "Untitled - Notepad";
        	private static IntPtr target_handle = IntPtr.Zero;
        	private static IntPtr edit_window_handle = IntPtr.Zero;

   		private static int shift_down = 0;

		[DllImport("user32.dll", SetLastError = true)]
		 static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        	
		[System.Runtime.InteropServices.DllImport("user32.dll")]
	  	 private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		 private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
		 static extern System.IntPtr FindWindowByCaption(int ZeroOnly, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		 static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll")] 
		 static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId); 

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		 private static extern IntPtr WindowFromPoint(Point pnt);

		[DllImport("user32.dll")]
		 static extern int MapVirtualKey(int uCode, uint uMapType);

		[DllImport("user32.dll")]
		 public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
		 
		[DllImport("User32.dll")]
    		 private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
    		 
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")] 
		 static extern bool UnhookWindowsHookEx(IntPtr hInstance); 

        	[System.Runtime.InteropServices.DllImport("user32.dll")]
        	 static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

		[DllImport("kernel32.dll")] 
		 static extern IntPtr LoadLibrary(string lpFileName); 
		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam); 
   
		const int WH_KEYBOARD_LL = 13;
		const int WM_KEYDOWN = 0x100; 
		const int WM_KEYUP = 0x101;
   		const int WM_CHAR = 0x102;
   		
		private LowLevelKeyboardProc _proc = hookProc; 

		private static IntPtr hhook = IntPtr.Zero; 
   
   
	 	int screen_width = 0;
	 	int screen_height = 0;
		int id = 0;
		
		enum KeyModifier
		{
		    None = 0,
		    Alt = 1,
		    Control = 2,
		    Shift = 4,
		    WinKey = 8
		}


		public KeyBackForm(string[] args)
		{
			if (args.Length > 1)
			{
				KeyBackForm.win_name = args[1];
				KeyBackForm.target_handle = FindWindowByCaption(0, KeyBackForm.win_name);
				if (KeyBackForm.target_handle == IntPtr.Zero)
				{
					MessageBox.Show("Handle not found for window");
					this.Close();
					Application.Exit();
				}
				else
				{
					KeyBackForm.edit_window_handle = FindWindowEx(KeyBackForm.target_handle, IntPtr.Zero, "edit", null);

					if(KeyBackForm.edit_window_handle == IntPtr.Zero)
					{
						MessageBox.Show("Handle not found for edit window");
						this.Close();
						Application.Exit();			
					}
				}

			}
			else
			{
				MessageBox.Show("No Window name input, select a window and click Shift+Alt+L");
				
			}

			InitializeComponent();

			RegisterHotKey(this.Handle, this.id, (int)KeyModifier.Shift | (int)KeyModifier.Alt, Keys.Q.GetHashCode());
			RegisterHotKey(this.Handle, this.id, (int)KeyModifier.Shift | (int)KeyModifier.Alt, Keys.L.GetHashCode());
   			SetHook();
   						
		}


		#region Windows Form Designer generated code

		public void SetHook() 
		{ 
			IntPtr hInstance = LoadLibrary("User32"); 
			hhook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, hInstance, 0); 
		} 
   
		public static void UnHook() 
		{ 
             		UnhookWindowsHookEx(hhook); 
         	}

		public static Keys ConvertFromString(string keystr) {
		    return (Keys)Enum.Parse(typeof(Keys), keystr);
		}
		
		public static IntPtr hookProc(int code, IntPtr wParam, IntPtr lParam) 
		{ 
			uint WM_SYSKEYDOWN = 0x104;
			int vkCode = Marshal.ReadInt32(lParam);
			int scanCode = MapVirtualKey(vkCode, (uint) 0x00);
			char c = (char)MapVirtualKey(vkCode, (uint)2);				

			if (scanCode == 42)
			{
				if (KeyBackForm.shift_down == 0)
				{
					keybd_event(0xa0, 0, 0x0001, 0);
					KeyBackForm.shift_down = 1;
				}
				else
				{
					keybd_event(0xa0, 0, 0x0002, 0);
					KeyBackForm.shift_down = 0;
				}
				return (IntPtr)1;
			}
			if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN) 
			{ 
			

				if (KeyBackForm.edit_window_handle != IntPtr.Zero)
				{
					PostMessage(KeyBackForm.edit_window_handle, WM_KEYDOWN, (IntPtr)vkCode, IntPtr.Zero);
					return (IntPtr)1; 
				}
				else
				{
					return CallNextHookEx(hhook, code, (int)wParam, lParam); 
				}
			} 
			else
			{
				return CallNextHookEx(hhook, code, (int)wParam, lParam); 
			}
		} 

		/*
		 * This function is run whenever a message is sent to a window
		 *
		 */
		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (m.Msg == 0x0312)
			{
				/*
				 * Note that the three lines below are not needed if you only want to register one hotkey.
				 * The below lines are useful in case you want to register multiple keys, which you can use a switch with the id as argument, or if you want to know which key/modifier was pressed for some particular reason
				 *
				 */

				Keys key_in = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
				KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
				int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.

				// MessageBox.Show(key.ToString());
				/*
				 * Below sleep is important for the hotkeys to function
				 *
				 */
				if (key_in.ToString() == "Q")
				{
					this.Close();
					Application.Exit();
				}
				if (key_in.ToString() == "L")
				{
					IntPtr hWnd = WindowFromPoint(Control.MousePosition);
					if (hWnd != IntPtr.Zero)
					{
     						MessageBox.Show("Window numbered " + hWnd.ToString() + " selected");
     						KeyBackForm.target_handle = hWnd;
						KeyBackForm.edit_window_handle = hWnd;
					}
				}
			}
		}

		public void InitializeComponent()
		{
			this.BackColor = Color.White;
			this.TransparencyKey = Color.White;
			this.FormBorderStyle = FormBorderStyle.None;
			this.Bounds = Screen.PrimaryScreen.Bounds;
			this.TopMost = true;
			this.screen_width = SystemInformation.VirtualScreen.Width;
			this.screen_height = SystemInformation.VirtualScreen.Height;

			this.Size = new Size(screen_width, screen_height);
			this.Name = "keyBack";
			this.Text = "keyBack";

			this.FormClosing += this.KeyBackForm_FormClosing;
			this.Show();

		}
		/*
		 * Unregister HotKey when form closes
		 *
		 */
		private void KeyBackForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			UnregisterHotKey(this.Handle, 0);       // Unregister hotkey with id 0 before closing the form. You might want to call this more than once with different id values if you are planning to register more than one hotkey.
			UnHook();
		}

		#endregion
		
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			string [] args = Environment.GetCommandLineArgs();
			
			Application.Run(new KeyBackForm(args));
			
					
		}
    	
	}
}