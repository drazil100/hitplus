using System.Text;
using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Globalization;


public delegate void ShowUpdateDialogCallback(string version, string whatsNew);

public class InputWindow : Form
{
	private Thread t;

	private bool closing = false;
	private bool reopening = false;

	public static DisplayWindow display;
	public TrackerCore tracker;
	public OptionsWindow optionsWindow;
	public static InputWindow mainWindow;
	
	public static FileReader config;
	public static FileReader individualLevels;
	public static ColorFileReader colors;


	//  Declare and initilize UI elements
	private NumericTextBox inputBox = new NumericTextBox();
	private Button submit = new Button();
	private Button undo = new Button();
	private Button save = new Button();
	private Button switchRoute = new Button ();
	private Button casualMode = new Button ();
	private Button options = new Button ();

	private ComparisonSelector selector = new ComparisonSelector();

	private int w = 300;
	private int h = 134;

	public InputWindow()
	{
		mainWindow = this;
		tracker = ScoreTracker.Tracker;
		display = new DisplayWindow (new DisplayWindowContent());

		config = ScoreTracker.config;
		colors = ScoreTracker.colors;

		Text = tracker.Data.Name;
		FormClosing += new FormClosingEventHandler(ConfirmClose);

		Size = new Size(w, h);
		MinimumSize  = new Size(w, h);
		MaximumSize = new Size(w, h);


		submit.Text = "Keep";
		undo.Text = "Back";
		undo.Enabled = false;
		save.Text = "Save && Reset";
		save.Enabled = false;
		switchRoute.Text = "Switch File";
		casualMode.Text = (config["casual_mode"] == "0") ? "Casual Mode" : "Tracking Mode";
		options.Text = "Options...";
		//options.Enabled = false;


		submit.Click += new EventHandler(OnSubmit);
		undo.Click += new EventHandler(OnUndo);
		save.Click += new EventHandler(OnReset);
		switchRoute.Click += new EventHandler (SwitchRoutes);
		casualMode.Click += new EventHandler (ToggleCasualMode);
		options.Click += new EventHandler (OpenOptions);

		selector.Index = tracker.Data.GetComparisonIndex();
		selector.Changed = OnDropdownChanged;

		SwapControls(submit);
		


		//SetControls();


		//  Redraw the form if the window is resized
		Resize += delegate { DoLayout(); };
		//Move += delegate { DoLayout();};

		//  Draw the form
		DoLayout();


		int x = -10000;
		int y = -10000;
		
		try
		{
			x = Int32.Parse(ScoreTracker.config["System", "input_x"]);
			y = Int32.Parse(ScoreTracker.config["System", "input_y"]);
		}
		catch(Exception)
		{
			
		}
		
		if (x != -10000 || y != -10000)
		{
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(x, y);
		}
		Show();
		
		try 
		{
			//CheckVersion();
			t = new Thread(new ThreadStart(delegate { CheckVersion(); }));
			t.Start();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}

	private void CheckVersion()
	{
		try
		{
			using (WebClient client = new WebClient()) 
			{
				string latestVersion = client.DownloadString("https://drazil100.bitbucket.io/tracker_version.txt");

				string[] parts = latestVersion.Split(':');
				if (parts[0] == "CurrentTrackerVersion")
				{
					string checkVersion = ScoreTracker.version;
					if (ScoreTracker.config.ContainsKey("skip_version") && ScoreTracker.config["skip_version"] != ScoreTracker.version)
					{
						int skipVersion = ScoreTracker.DateToNumber(ScoreTracker.config["skip_version"]);
						if (skipVersion != -1)
						{
							
							if (skipVersion > ScoreTracker.DateToNumber(ScoreTracker.version))
							{
								checkVersion = ScoreTracker.config["skip_version"];
								Console.WriteLine("Skip Version:" + checkVersion);
							}
						}
						else
						{
							ScoreTracker.config.RemoveKey("skip_version");
						}
					}
					if (ScoreTracker.DateToNumber(checkVersion) < ScoreTracker.DateToNumber(parts[1]))
					{
						string whatsNew = client.DownloadString("https://drazil100.bitbucket.io/tracker_whats_new.txt");
						ShowUpdateDialog(parts[1], whatsNew);
					}
				}
				Console.WriteLine(String.Format("This Version: {0}, Version Check: {1}", ScoreTracker.version, parts[1]));
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
		}
	}

	public void ShowUpdateDialog(string version, string whatsNew)
	{
		if (this.InvokeRequired)
		{
			ShowUpdateDialogCallback d = new ShowUpdateDialogCallback(ShowUpdateDialog);
			this.Invoke(d, new object[] { version, whatsNew });
		}
		else
		{
			whatsNew = whatsNew.Replace("\n-", "\n\u2022");
			var dialog = new WhatsNewDialog(version, whatsNew);
			dialog.ShowDialog();
			if (dialog.close)
			{
				this.Close();
			}
		}
	}

	//  Just ripped the following 2 methods from dumas's code
	private int GetWidth()
	{
		return (
			Width - (2 * SystemInformation.FrameBorderSize.Width)
		);
	}

	private int GetHeight()
	{
		return (
			Height - (2 * SystemInformation.FrameBorderSize.Height +
				SystemInformation.CaptionHeight)
		);
	}

	private void SetHeight()
	{
		if (!tracker.IsRunning())
		{
			MaximumSize = new Size (w, options.Top + options.Height + selector.Height + GetCaptionSize());
			MinimumSize = new Size (w, options.Top + options.Height + selector.Height + GetCaptionSize());
		}
		else
		{
			MinimumSize  = new Size(w, options.Top  + selector.Height + GetCaptionSize());
			MaximumSize = new Size(w, options.Top + selector.Height + GetCaptionSize());
		}
	}
	
	private int GetCaptionSize()
	{
		return (2 * SystemInformation.FrameBorderSize.Height + SystemInformation.CaptionHeight);
	}

	private void DoLayout()
	{
		inputBox.Height = 25;
		inputBox.Width = GetWidth();
		submit.Top = 20;
		submit.Width = GetWidth ()/2;
		submit.Height = 25;
		undo.Top = submit.Top;
		undo.Left = submit.Left + submit.Width;
		undo.Width = submit.Width;
		undo.Height = submit.Height;
		save.Top = submit.Top;
		save.Width = submit.Width;
		save.Height = submit.Height;
		switchRoute.Top = submit.Top + submit.Height;
		switchRoute.Height = submit.Height;
		switchRoute.Width = GetWidth() / 2;
		casualMode.Top = switchRoute.Top;
		casualMode.Left = switchRoute.Left + switchRoute.Width;
		casualMode.Height = submit.Height;
		casualMode.Width = submit.Width;
		options.Top = casualMode.Top + casualMode.Height;
		options.Height = casualMode.Height;
		options.Width = GetWidth();

		selector.Dock = DockStyle.Bottom;

		SetHeight();
	}

	public void SwapControls(Button b)
	{
		Controls.Clear();

		if (b != save)
		{
			Controls.Add(inputBox);
			inputBox.Enabled = true;

			if (!tracker.IsRunning())
			{
				undo.Enabled = false;
				options.Enabled = true;
			}
			else
			{
				undo.Enabled = true;
				options.Enabled = false;
			}
		}
		else
		{
			inputBox.Enabled = false;
			undo.Enabled = true;
			options.Enabled = false;
		}
		submit.Enabled = false;
		save.Enabled = false;
		switchRoute.Enabled = true;
		b.Enabled = true;
		casualMode.Enabled = true;
		selector.Enabled = true;
		AcceptButton = b;
		Controls.Add(b);
		Controls.Add(undo);
		Controls.Add (switchRoute);
		Controls.Add (casualMode);
		Controls.Add (selector);
		if (!tracker.IsRunning())
			Controls.Add (options);

		DoLayout();
	}

	public void OnSubmit(object sender, EventArgs e)
	{
		try
		{
			int s = Int32.Parse(inputBox.Text);
			if (s < 0 || s > 999)
			{
				inputBox.Focus();
				return;
			}
			inputBox.Text = "";
			tracker.Submit(s);

			if (tracker.IsRunning())
			{
				SwapControls(submit);
			}

			display.UpdateScores();

			if (tracker.IsFinished())
			{
				SwapControls(save);
				save.Focus();
				return;
			}
			else
			{
				inputBox.Focus();
			}
		}
		catch (Exception)
		{
			inputBox.Text = "";
			inputBox.Focus();
		}
	}

	public void OnUndo(object sender, EventArgs e)
	{
		try
		{
			tracker.Undo();
			if (!tracker.IsFinished())
			{
				SwapControls(submit);
			}
			inputBox.Text = "";


			inputBox.Focus();
			display.UpdateScores();
		}
		catch (Exception e2)
		{
			Console.WriteLine(e2.Message);
		}
	}

	public void OnReset(object sender, EventArgs e)
	{
		tracker.SaveAndReset();
		selector.Reload();
		tracker.Data.SetComparisonIndex(selector.Index);
		SwapControls(submit);
		submit.Enabled = true;
		SwapControls (submit);
		inputBox.Focus();
		display.UpdateScores();
	}

	public void SwitchRoutes(object sender, EventArgs e)
	{
		try
		{
			if (tracker.IsRunning())
			{
				var confirmResult = MessageBox.Show ("Your run is incomplete! Are you sure you wish to exit?\n\n(Any unsaved gold scores will be lost)\n(To save gold scores on an incomplete run fill in the rest of the levels with 0)",
						"Continue Closing?",
						MessageBoxButtons.YesNo);
				if (confirmResult == DialogResult.Yes)
				{
					tracker.Reset();
					SwapControls (submit);
				}
				else
				{
					return;
				}
			}
			else
			{
				tracker.Reset();
				SwapControls (submit);
			}	

			if (tracker.IsRunning())
				return;
			
			ScoreTracker.FileIndex += 1;
			config["file_index"] = "" + ScoreTracker.FileIndex;
			config.Save ();

			ScoreTracker.Data = new TrackerData(new FileReader(ScoreTracker.files[ScoreTracker.FileIndex], SortingStyle.Validate));

			Text = tracker.Data.Name;

			selector.Reload();
			selector.Index = tracker.Data.GetComparisonIndex();
			display.UpdateScores();

		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		inputBox.Focus();
		display.UpdateScores();

	}
	
	public void ToggleCasualMode(object sender, EventArgs e)
	{
		if (config["casual_mode"] == "0")
		{
			config["casual_mode"] = "1";
			casualMode.Text = "Tracking Mode";
		}
		else
		{
			config["casual_mode"] = "0";
			casualMode.Text = "Casual Mode";
		}
		config.Save();
		
		//ScorePanel.ToggleCasualMode();
		inputBox.Focus();
		display.UpdateScores();
	}
	
	public void OpenOptions(object sender, EventArgs e)
	{
		SaveBounds();
		inputBox.Enabled = false;
		submit.Enabled = false;
		undo.Enabled = false;
		save.Enabled = false;
		switchRoute.Enabled = false;
		casualMode.Enabled = false;
		options.Enabled = false;
		selector.Enabled = false;
		optionsWindow = new OptionsWindow ();
		optionsWindow.FormClosing += new FormClosingEventHandler(CloseOptions);
		optionsWindow.Show ();


	}

	public void CloseOptions(object sender, FormClosingEventArgs e)
	{
		if (!closing)
		{
			display.ResetContent();
			SwapControls (submit);
		}
		display.ResetContent();
		selector.Reload();
		selector.Index = tracker.Data.GetComparisonIndex();
		Text = tracker.Data.Name;
	}

	public void OnDropdownChanged()
	{
		tracker.Data.SetComparisonIndex(selector.Index);
		display.UpdateScores();
		inputBox.Focus();
	}

	public void SaveBounds()
	{
		// Check if the window is minimized or maximized
		if ((this.WindowState == FormWindowState.Minimized) ||
				(this.WindowState == FormWindowState.Maximized))
		{
			// Use the restored state values
			ScoreTracker.config["System", "input_x"] = "" + this.RestoreBounds.Left;
			ScoreTracker.config["System", "input_y"] = "" + this.RestoreBounds.Top;
		}
		else
		{
			// Use the normal state values
			ScoreTracker.config["System", "input_x"] = "" + this.Left;
			ScoreTracker.config["System", "input_y"] = "" + this.Top;
		}
		if (display != null)
		{
			if ((display.WindowState == FormWindowState.Minimized) ||
					(display.WindowState == FormWindowState.Maximized))
			{
				// Use the restored state values
				ScoreTracker.config["System", "tracker_x"] = "" + display.RestoreBounds.Left;
				ScoreTracker.config["System", "tracker_y"] = "" + display.RestoreBounds.Top;
				if (ScoreTracker.config ["layout"] == "0")
				{
					ScoreTracker.config ["System", "horizontal_width"] = "" + display.RestoreBounds.Width;
					ScoreTracker.config ["System", "horizontal_height"] = "" + display.RestoreBounds.Height;
				}
				else
				{
					ScoreTracker.config ["System", "vertical_width"] = "" + display.RestoreBounds.Width;
					ScoreTracker.config ["System", "vertical_height"] = "" + display.RestoreBounds.Height;			
				}
			}
			else
			{
				// Use the restored state values
				ScoreTracker.config["System", "tracker_x"] = "" + display.Left;
				ScoreTracker.config["System", "tracker_y"] = "" + display.Top;
				if (ScoreTracker.config ["layout"] == "0")
				{
					ScoreTracker.config ["System", "horizontal_width"] = "" + display.Width;
					ScoreTracker.config ["System", "horizontal_height"] = "" + display.Height;
				}
				else
				{
					ScoreTracker.config ["System", "vertical_width"] = "" + display.Width;
					ScoreTracker.config ["System", "vertical_height"] = "" + display.Height;			
				}
			}
		}
	}

	public void ConfirmClose(object sender, FormClosingEventArgs e) 
	{
		if (!closing)
		{
			SaveBounds();
			config.Save();
			
			if (!reopening)
			{
				if (tracker.IsRunning())
				{
					var confirmResult = MessageBox.Show ("Your run is incomplete! Are you sure you wish to exit?\n\n(Any unsaved gold scores will be lost)\n(To save gold scores on an incomplete run fill in the rest of the levels with 0)",
							"Continue Closing?",
							MessageBoxButtons.YesNo);
					if (confirmResult == DialogResult.Yes)
					{
						closing = true;
						Application.Exit ();
					}
					else
					{
						e.Cancel = true;
					}
				}
				else
				{
					closing = true;
					Application.Exit ();
				}	
			}
			else
			{

			}
		}
	}






}
