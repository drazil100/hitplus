using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;


public class ScoreTracker : Form
{
	public static string version = "12/22/2017";

	[DllImport("kernel32.dll")]
	static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	private Thread t;

	const int SW_HIDE = 0;
	const int SW_SHOW = 5;
	
	private bool closing = false;
	private bool reopening = false;

	public static FileReader config;
	public static FileReader pbEasy;
	public static FileReader pbHard;
	public static FileReader individualLevels;

	public DisplayWindow tracker;
	public OptionsWindow optionsWindow;
	public static ScoreTracker mainWindow;

	//  Declare and initilize UI elements
	//private LogBox chatLog = new LogBox(); //  LogBox is my own class that has a method for adding messages and built in thread safety
	public static Label topScore = new Label();
	public static Label sobScore = new Label();
	public static Label topScoreName = new Label();
	public static Label sobScoreName = new Label();

	private NumericTextBox inputBox = new NumericTextBox();
	private Button submit = new Button();
	private Button undo = new Button();
	private Button save = new Button();
	private Button switchRoute = new Button ();
	private Button options = new Button ();

	public int index = 0;

	private int w = 300;
	private int h = 134;

	//private Button sendButton = new Button();
	//private TabControl rooms = new TabControl();

	//  Declare colors
	public static Color text_color;
	public static Color background_color_highlighted;
	public static Color background_color;
	public static Color text_color_highlighted;
	public static Color text_color_ahead;
	public static Color text_color_behind;
	public static Color text_color_best;
	public static Color text_color_total;

	//private string[] pb_run;
	//private string[] ils;
	//private string peanut_butter;
	//private string individual_levels;

	//private List<Score> scores = new List<Score>();

	public ScoreTracker()
	{
		mainWindow = this;

		text_color = ColorTranslator.FromHtml(config["text_color"]);
		background_color_highlighted = ColorTranslator.FromHtml(config["background_color_highlighted"]);
		background_color = ColorTranslator.FromHtml(config["background_color"]);
		text_color_highlighted = ColorTranslator.FromHtml(config["text_color_highlighted"]);
		text_color_ahead = ColorTranslator.FromHtml(config["text_color_ahead"]);
		text_color_behind = ColorTranslator.FromHtml(config["text_color_behind"]);
		text_color_best = ColorTranslator.FromHtml(config["text_color_best"]);
		text_color_total = ColorTranslator.FromHtml(config["text_color_total"]);

		tracker = new DisplayWindow ();
		//this.peanut_butter = peanut_butter;
		//this.individual_levels = individual_levels;


		Text = "Input";
		FormClosing += new FormClosingEventHandler(ConfirmClose);

		Size = new Size(w, h);
		MinimumSize  = new Size(w, h);
		MaximumSize = new Size(w, h);


		submit.Text = "Keep";
		undo.Text = "Back";
		undo.Enabled = false;
		save.Text = "Save & Reset";
		save.Enabled = false;
		switchRoute.Text = "Switch Route";
		options.Text = "Options...";
		//options.Enabled = false;


		submit.Click += new EventHandler(OnSubmit);
		undo.Click += new EventHandler(OnUndo);
		save.Click += new EventHandler(OnReset);
		switchRoute.Click += new EventHandler (SwitchRoutes);
		options.Click += new EventHandler (OpenOptions);

		SwapControls(submit);
		


		//  Set colors
		topScore.ForeColor = text_color_total;
		sobScore.ForeColor = text_color_total;




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
			x = Int32.Parse(ScoreTracker.config["input_x"]);
			y = Int32.Parse(ScoreTracker.config["input_y"]);
		}
		catch(Exception)
		{
			
		}
		
		if (x != -10000 && y != -10000)
		{
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(x, y);
		}
		Show();
		
		ScoreTracker.config["input_x"] = "" + this.Location.X;
		ScoreTracker.config["input_y"] = "" + this.Location.Y;

		try 
		{
			t = new Thread(new ThreadStart(delegate { CheckVersion(config["sub_domain"]); }));
			t.Start();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}



		//  When the form is shown set the focus to the input box

		//  Close the network connection when the form is closed
		//  To prevent any hangups
		//FormClosing += delegate { CloseNetwork(); };
	}

	private void CheckVersion(string subDomain)
	{
		string latestVersion = HttpClient.ClientMain (subDomain + "coded-dragon.com/pagecontentdir/tracker_version.txt");
		string[] parts = latestVersion.Split(':');
		if (parts[0] == "CurrentTrackerVersion")
		{
			if (version != parts[1])
			{
				string whatsNew = HttpClient.ClientMain (subDomain + "coded-dragon.com/pagecontentdir/tracker_whats_new.txt");
				MessageBox.Show(whatsNew + "\r\n\r\n" + subDomain + "coded-dragon.com/ScoreTracker/", "Update Available: (" + parts[1] + ")");
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
		switchRoute.Width = GetWidth();
		options.Top = switchRoute.Top + switchRoute.Height;
		options.Height = switchRoute.Height;
		options.Width = switchRoute.Width;
	}

	public void SwapControls(Button b)
	{
		Controls.Clear();

		if (b != save)
		{
			Controls.Add(inputBox);
			inputBox.Enabled = true;

			if (index == 0)
			{
				undo.Enabled = false;
				options.Enabled = true;
				MaximumSize = new Size (w, h);
				MinimumSize = new Size (w, h);
			}
			else
			{
				undo.Enabled = true;
				options.Enabled = false;
				MinimumSize  = new Size(w, h - options.Height);
				MaximumSize = new Size(w, h - options.Height);
			}
		}
		else
		{
			inputBox.Enabled = false;
			undo.Enabled = true;
			options.Enabled = false;
			MinimumSize  = new Size(w, h - options.Height);
			MaximumSize = new Size(w, h - options.Height);
		}
		submit.Enabled = false;
		save.Enabled = false;
		switchRoute.Enabled = true;
		b.Enabled = true;
		AcceptButton = b;
		Controls.Add(b);
		Controls.Add(undo);
		Controls.Add (switchRoute);
		if (index == 0)
			Controls.Add (options);

		DoLayout();
	}



	public void OnSubmit(object sender, EventArgs e)
	{
		try
		{
			int s = Int32.Parse(inputBox.Text);
			if (s < 0 || s > 999)
				return;
			Score.scoresList[index].CurrentScore = s;
			inputBox.Text = "";
			Score.scoresList[index].Unhighlight();
			index++;
			if (index != Score.scoresList.Count)
				Score.scoresList[index].Highlight();

			if (index > 0)
			{
				SwapControls(submit);
			}

			inputBox.Focus();

			if (index == Score.scoresList.Count)
			{
				SwapControls(save);
				return;
			}
		}
		catch (Exception)
		{

		}
	}


	public void OnUndo(object sender, EventArgs e)
	{
		try
		{
			if (index < Score.scoresList.Count)
			{
				Score.scoresList[index].CurrentScore = -1;
				Score.scoresList[index].Unhighlight();
			}
			else
			{
				SwapControls(submit);
			}
			index--;
			Score.scoresList[index].CurrentScore = -1;
			if (index > 0 || ScoreTracker.config["start_highlighted"] == "1")
				Score.scoresList[index].Highlight();
			inputBox.Text = "";


			if (index < Score.scoresList.Count)
			{
				SwapControls(submit);
			}

			inputBox.Focus();
		}
		catch (Exception)
		{

		}
	}

	public void OnReset(object sender, EventArgs e)
	{
		Score.UpdateBestScores();
		Score.SaveRun();
		SwapControls(submit);
		submit.Enabled = true;
		index = 0;
		SwapControls (submit);
		if (ScoreTracker.config["start_highlighted"] == "1")
			Score.scoresList[index].Highlight();
		inputBox.Focus();
	}

	public void SwitchRoutes(object sender, EventArgs e)
	{
		try
		{
			reopening = true;
			tracker.Close ();
			reopening = false;

			if (tracker != null)
				return;
			
			if (config ["hard_route"] == "0")
			{
				config ["hard_route"] = "1";
			}
			else
			{
				config ["hard_route"] = "0";
			}
			config.Save ();
			tracker = new DisplayWindow ();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
	
	public void OpenOptions(object sender, EventArgs e)
	{
		reopening = true;
		tracker.Close ();
		reopening = false;

		inputBox.Enabled = false;
		submit.Enabled = false;
		undo.Enabled = false;
		save.Enabled = false;
		switchRoute.Enabled = false;
		options.Enabled = false;
		optionsWindow = new OptionsWindow ();
		optionsWindow.FormClosing += new FormClosingEventHandler(CloseOptions);
		optionsWindow.Show ();


	}

	public void CloseOptions(object sender, FormClosingEventArgs e)
	{
		if (!closing)
		{
			tracker = new DisplayWindow ();
			SwapControls (submit);
		}
	}
	
	public void ConfirmClose(object sender, FormClosingEventArgs e) 
	{
		
		ScoreTracker.config["input_x"] = "" + this.Location.X;
		ScoreTracker.config["input_y"] = "" + this.Location.Y;
		if (tracker != null)
		{
			ScoreTracker.config ["tracker_x"] = "" + tracker.Location.X;
			ScoreTracker.config ["tracker_y"] = "" + tracker.Location.Y;
			if (ScoreTracker.config ["layout"] == "0")
			{
				ScoreTracker.config ["horizontal_width"] = "" + tracker.Width;
				ScoreTracker.config ["horizontal_height"] = "" + tracker.Height;
			}
			else
			{
				ScoreTracker.config ["vertical_width"] = "" + tracker.Width;
				ScoreTracker.config ["vertical_height"] = "" + tracker.Height;			
			}
		}
		ScoreTracker.config.Save();
		
		if (!reopening)
		{
			if (index > 0 && !closing)
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
			
			if (index > 0 && !closing)
			{
				var confirmResult = MessageBox.Show ("Your run is incomplete! Are you sure you wish to exit?\n\n(Any unsaved gold scores will be lost)\n(To save gold scores on an incomplete run fill in the rest of the levels with 0)",
					"Continue Closing?",
					MessageBoxButtons.YesNo);
				if (confirmResult == DialogResult.Yes)
				{
					index = 0;
					SwapControls (submit);
					tracker.Controls.Clear ();
					tracker = null;
				}
				else
				{
					e.Cancel = true;
				}
			}
			else
			{
				index = 0;
				SwapControls (submit);
				tracker.Controls.Clear ();
				tracker = null;
			}	
		}
	}






	public static void Main(string[] args)
	{
		try
		{
			var handle = GetConsoleWindow();
			ShowWindow(handle, SW_HIDE);
		}
		catch (Exception)
		{

		}

		for (int i = 0; i < args.Length; i++)
		{
			/*if (args[i] == "-s")
			{
				//  If -s is found in the arguments check if the port is specified
				for (int j = 0; j < args.Length; j++)
				{
					if (args[j] == "-p" && j < args.Length - 1)
					{
						//  If -p is found set port to the next argument
						port = Convert.ToInt32(args[j+1]);
					}
				}
				//  Start server and return from Main() before client is started
				StartServer(port);
				return;
			}*/
		}


		//  Start the client if -s was not found


		config = new FileReader("config.txt", SortingStyle.Sort);
		config.AddNewItem("hard_route", "0");
		config.AddNewItem("layout", "0");
		config.AddNewItem("include_route_pbs_in_individuals_file", "0");
		config.AddNewItem("sums_horizontal_alignment", "0");
		config.AddNewItem("vertical_scale_mode", "0");
		config.AddNewItem("font", "Segoe UI");
		config.AddNewItem("font_size", "18");
		config.AddNewItem("highlight_current", "0");
		config.AddNewItem("start_highlighted", "1");
		config.AddNewItem("background_color", "#0F0F0F");
		config.AddNewItem("background_color_highlighted", "#3373F4");
		config.AddNewItem("text_color", "#FFFFFF");
		config.AddNewItem("text_color_highlighted", "#FFFFFF");
		config.AddNewItem("text_color_ahead", "#00CC36");
		config.AddNewItem("text_color_behind", "#CC1200");
		config.AddNewItem("text_color_best", "#D8AF1F");
		config.AddNewItem("text_color_total", "#FFFFFF");
		config.AddNewItem("horizontal_width", "1296");
		config.AddNewItem("horizontal_height", "99");
		config.AddNewItem("vertical_width", "316");
		config.AddNewItem("vertical_height", "309");

		if (config ["debug"] == "1")
		{
			try
			{
				var handle = GetConsoleWindow ();
				ShowWindow (handle, SW_SHOW);
			}
			catch (Exception)
			{

			}
		}

		if (config ["layout"] == "horizontal")
		{
			config ["layout"] = "0";
		}
		if (config ["layout"] == "vertical")
		{
			config ["layout"] = "1";
		}
		if (config ["layout"] != "0" && config ["layout"] != "1")
		{
			config ["layout"] = "0";
		}

		if (config ["sums_horizontal_alignment"] == "right")
		{
			config ["sums_horizontal_alignment"] = "0";
		}
		if (config ["sums_horizontal_alignment"] == "left")
		{
			config ["sums_horizontal_alignment"] = "1";
		}
		if (config ["sums_horizontal_alignment"] != "0" && config ["sums_horizontal_alignment"] != "1")
		{
			config ["sums_horizontal_alignment"] = "0";
		}

		pbEasy = new FileReader("pb_easy.txt", SortingStyle.Validate);
		pbEasy.AddNewItem("Corneria", "0");
		pbEasy.AddNewItem("Meteo", "0");
		pbEasy.AddNewItem("Katina", "0");
		pbEasy.AddNewItem("Sector X", "0");
		pbEasy.AddNewItem("Macbeth", "0");
		pbEasy.AddNewItem("Area 6", "0");
		pbEasy.AddNewItem("Venom", "0");

		pbHard = new FileReader("pb_hard.txt", SortingStyle.Validate);
		pbHard.AddNewItem("Corneria", "0");
		pbHard.AddNewItem("Sector Y", "0");
		pbHard.AddNewItem("Aquas", "0");
		pbHard.AddNewItem("Zoness", "0");
		pbHard.AddNewItem("Macbeth", "0");
		pbHard.AddNewItem("Area 6", "0");
		pbHard.AddNewItem("Venom", "0");

		individualLevels = new FileReader("pb_individuals.txt", SortingStyle.Unsort);
		individualLevels.RemoveKey("Easy Route");
		individualLevels.RemoveKey("Hard Route");

		if (config["include_route_pbs_in_individuals_file"] == "1")
		{
			int total = 0;
			foreach (KeyValuePair<string, string> pair in pbEasy)
			{
				total += Int32.Parse(pair.Value);
			}
			if (total > 0)
			{
				individualLevels.AddNewItem("Easy Route", "" + total);
				individualLevels["Easy Route"] = "" + total;
			}
			total = 0;
			foreach (KeyValuePair<string, string> pair in pbHard)
			{
				total += Int32.Parse(pair.Value);
			}
			if (total > 0)
			{
				individualLevels.AddNewItem("Hard Route", "" + total);
				individualLevels["Hard Route"] = "" + total;
			}
		}

		try
		{
			config.Save();
			pbEasy.Save();
			pbHard.Save();
			individualLevels.Save();
		}
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);

			//peanut_butter = "CO:0\nME:0\nKA:0\nSX:0\nMA:0\na6:0\nVE:0";
			//individual_levels = "CO:0\nME:0\nKA:0\nSX:0\nSY:0\nAQ:0\nZO:0\nMA:0\na6:0\nVE:0";
		}

		try
		{
			Application.Run(new ScoreTracker());
		}
		catch (Exception) {}
		/*
		try
		{
			var sw = new StreamWriter("foundElements.txt");

			int total = match(elementArray, blankList, sw);

			sw.WriteLine("\nTotal combinations: {0}", total);
			Console.WriteLine("\nTotal combinations: {0}", total);

			sw.Close();
		}
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);
		}
		*/
	}
}

public class NumericTextBox : TextBox
{
	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		base.OnKeyPress(e);
		// Check if the pressed character was a backspace or numeric.
		if (e.KeyChar != (char)8  && !char.IsNumber(e.KeyChar) && e.KeyChar != '\n')
		{
			e.Handled = true;
		}
	}
}
