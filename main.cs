using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class Entry
{
	public string name = "";
	public int score = 0;
	
	public Entry(string name, int score)
	{
		this.name = name;
		this.score = score;
	}
	
	public override string ToString()
	{
		return String.Format("{0}:{1}", name, score);
	}
}

public class Score : Panel
{
	private static Dictionary<string, Score> scores = new Dictionary<string, Score>();
	public static List<Score> scoresList = new List<Score>();
	public static List<Entry> topData = new List<Entry>();
	
	private Label nameLabel = new Label();
	private Label scoreLabel = new Label();
	
	
	private Color text_default = ColorTranslator.FromHtml("#99dfff");
	private Color bgColor = ColorTranslator.FromHtml("#052e51");
	private Color current = ColorTranslator.FromHtml("#FF8800");
	private Color ahead = ColorTranslator.FromHtml("#00CC36");
	private Color behind = ColorTranslator.FromHtml("#CC1200");
	private Color bestColor = ColorTranslator.FromHtml("#D8AF1F");

	private int score = 0;
	public int pbScore = 0;
	public int best = 0;
	public string name = "";
	private int index = 0;
	public int arrayIndex = 0;
	
	public override string ToString()
	{
		return String.Format("{0}:{1}", name, CurrentScore);
	}
	
	public int CurrentScore {
		get {return score;}
		set 
		{
			score = value;
			int runScore = 0;
			int oldScore = 0;
			for (int i = 0; (i < scoresList.Count && i <= index); i++)
			{
				oldScore += scoresList[i].pbScore;
				runScore += scoresList[i].CurrentScore;
			}
			if (runScore >= oldScore)
			{
				scoreLabel.Text = String.Format("{0}\n+{1}", score, (runScore - oldScore));
				scoreLabel.ForeColor = ahead;
			}
			else
			{
				scoreLabel.Text = String.Format("{0}\n-{1}", score, (oldScore - runScore));
				scoreLabel.ForeColor = behind;
			}
			if (score > best)
			{
				scoreLabel.ForeColor = bestColor;
			}
			
			if (score == 0)
			{
				scoreLabel.ForeColor = text_default;
				scoreLabel.Text = ""+pbScore;
				
			}
		}
	}
	
	public static void UpdateBestScores()
	{
		bool updated = false;
		for(int i = 0; i < scoresList.Count; i++)
		{
			if (scoresList[i].CurrentScore > scoresList[i].best)
			{
				scoresList[i].best = scoresList[i].CurrentScore;
				updated = true;
				foreach(Entry entry in topData)
				{
					if (entry.name == scoresList[i].name)
					{
						entry.score = scoresList[i].CurrentScore;
					}
				}
			}
		}
		
		if (updated)
		{
			string temp = topData[0].ToString();
		
			for(int i = 1; i < topData.Count; i++)
			{
				temp += "\n" + topData[i].ToString();
			}
			
			try
			{
				string file = @"pb_individuals.txt";
				var sw = new StreamWriter(file);
				sw.Write(temp);
				sw.Close();
			}
			catch (Exception e2)
			{
				Console.WriteLine("Error: " + e2.Message);
			}
		}
	}
	
	public static void SaveRun()
	{
		
		string temp = "";
		int sob = 0;
		int tot = GetTotal();
		bool doSave = false;
		
		if (GetTotal() > GetOldScore())
			doSave = true;
		
		try
		{
			temp = scoresList[0].ToString();
			
			for(int i = 0; i < scoresList.Count; i++)
			{
				if (i > 0)
					temp += "\n" + scoresList[i].ToString();
				
				sob += scoresList[i].best;
				if (doSave)
					scoresList[i].pbScore = scoresList[i].CurrentScore;
				scoresList[i].CurrentScore = 0;
			}
		}
		catch (Exception e3)
		{
			Console.WriteLine("Error: " + e3.Message);
			return;
		}
		
		ScoreWindow.sobScore.Text = "SoB: " + sob;
		string config = "";
		
		if (!doSave)
			return;
		
		ScoreWindow.topScore.Text = "Top: " + tot;
		
		try
		{
			config = File.ReadAllText(@"config.txt", Encoding.UTF8);
			
			try
			{
				string file = "";
				
				if (config == "Hard Route: 0")
					file = @"pb_easy.txt";
				else
					file = @"pb_hard.txt";
				
				var sw = new StreamWriter(file);
				sw.Write(temp);
				sw.Close();
			}
			catch (Exception e2)
			{
				Console.WriteLine("Error: " + e2.Message);
			}
			
		}
		catch (Exception)
		{
			try
			{
				var sw = new StreamWriter(@"emergency_save.txt");
				sw.Write(temp);
				sw.Close();
			}
			catch (Exception e2)
			{
				Console.WriteLine("Error: " + e2.Message);
			}
		}
	}
	
	private Score()
	{
		
	}
	
	public Score(string name, int score)
	{
		if (scores.ContainsKey(name))
			return;
		scores.Add(name, this);
		index = scoresList.Count;
		scoresList.Add(this);
		nameLabel.ForeColor = text_default;
		if (index == 0)
			Highlight();
		scoreLabel.ForeColor = text_default;
		this.name = name;
		this.pbScore = score;
		scoreLabel.Text = String.Format("{0}", pbScore);
		nameLabel.Text = String.Format("{0}:", name);
		
		Controls.Add(nameLabel);
		Controls.Add(scoreLabel);
		
		Resize += delegate { DoLayout(); };
		
		DoLayout();
		
	}
	
	~Score()
	{
		scores.Remove(this.name);
		scoresList.Remove(this);
	}
	
	public static Score GetScore(string name)
	{
		if (scores.ContainsKey(name))
			return scores[name];
		return null;
	}
	
	public static void SetBest(string name, int score, int arrayIndex = -1)
	{
		if (scores.ContainsKey(name))
		{
			scores[name].best = score;
		
			if (arrayIndex > -1)
				scores[name].arrayIndex = arrayIndex;
		}
		topData.Add(new Entry(name, score));
	}
	
	public static int GetTotal()
	{
		int t = 0;
		foreach (Score s in scoresList)
		{
			t += s.CurrentScore;
		}
		return t;
	}
	
	public static int GetOldScore()
	{
		int t = 0;
		foreach (Score s in scoresList)
		{
			t += s.pbScore;
		}
		return t;
	}
	
	public void Highlight()
	{
		nameLabel.ForeColor = current;
	}
	
	public void Unhighlight()
	{
		nameLabel.ForeColor = text_default;
	}
	
	private void DoLayout()
	{
		nameLabel.Height = Height;
		scoreLabel.Height = Height;
		nameLabel.Width = 60;
		scoreLabel.Left = 60;
		scoreLabel.Width = Width - 60;
	}
	
}

public class ScoreInput : Form
{
	private TextBox inputBox = new TextBox();
	private Button submit = new Button();
	private Button undo = new Button();
	private Button save = new Button();
	
	public int index = 0;
	
	public ScoreInput()
	{
		Size = new Size(300, 89);
		
		submit.Text = "Keep";
		undo.Text = "Back";
		undo.Enabled = false;
		save.Text = "Save & Reset";
		save.Enabled = false;
	
	
		submit.Click += new EventHandler(OnSubmit);
		undo.Click += new EventHandler(OnUndo);
		save.Click += new EventHandler(OnReset);
		
		SwapControls(submit);
		
		//Resize += delegate { DoLayout(); };
		
	}
	
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
		submit.Top = 25;
		submit.Width = GetWidth()/2;
		submit.Height = 25;
		undo.Top = submit.Top;
		undo.Width = GetWidth() - submit.Width;
		undo.Left = submit.Width;
		undo.Height = submit.Height;
		save.Top = submit.Top;
		save.Width = submit.Width;
		save.Height = submit.Height;
	}
	
	public void SwapControls(Control b)
	{
		Controls.Clear();
		
		if (b != save)
		{
			Controls.Add(inputBox);
			inputBox.Enabled = true;
		}
		else
		{
			inputBox.Enabled = true;
		}
		Controls.Add(b);
		Controls.Add(undo);
		
		DoLayout();
	}
	
	
	
	public void OnSubmit(object sender, EventArgs e)
	{
		try
		{
			Score.scoresList[index].CurrentScore = Int32.Parse(inputBox.Text);
			inputBox.Text = "";
			Score.scoresList[index].Unhighlight();
			index++;
			if (index != Score.scoresList.Count)
				Score.scoresList[index].Highlight();
			
			if (index > 0)
			{
				undo.Enabled = true;
			}
			
			inputBox.Focus();
			
			if (index == Score.scoresList.Count)
			{
				submit.Enabled = false;
				save.Enabled = true;
				SwapControls(save);
				return;
			}
			else
			{
				submit.Enabled = true;
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
				Score.scoresList[index].CurrentScore = 0;
				Score.scoresList[index].Unhighlight();
			}
			else
			{
				SwapControls(submit);
				save.Enabled = false;
			}
			index--;
			Score.scoresList[index].CurrentScore = 0;
			Score.scoresList[index].Highlight();
			inputBox.Text = "";
			
			
			if (index < Score.scoresList.Count)
			{
				submit.Enabled = true;
			}
			
			inputBox.Focus();
			
			if (index == 0)
			{
				undo.Enabled = false;
				return;
			}
			else
			{
				undo.Enabled = true;
			}
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
		Score.scoresList[index].Highlight();
		inputBox.Focus();
	}
}

public class ScoreWindow : Form
{
	
	[DllImport("kernel32.dll")]
	static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	
	const int SW_HIDE = 0;
	const int SW_SHOW = 5;

	//  Declare and initilize UI elements
	//private LogBox chatLog = new LogBox(); //  LogBox is my own class that has a method for adding messages and built in thread safety
	private TextBox inputBox = new TextBox();
	public static Label topScore = new Label();
	public static Label sobScore = new Label();
	public ScoreInput sInput = new ScoreInput();
	
	//private Button sendButton = new Button();
	//private TabControl rooms = new TabControl();

	//  Declare colors
	private Color text = ColorTranslator.FromHtml("#99dfff");
	private Color windowColor = ColorTranslator.FromHtml("#052e51");
	private Color bgColor = ColorTranslator.FromHtml("#082045");
	private Color current = ColorTranslator.FromHtml("#FF8800");
	private Color ahead = ColorTranslator.FromHtml("#00CC36");
	private Color behind = ColorTranslator.FromHtml("#CC1200");
	private Color best = ColorTranslator.FromHtml("#D8AF1F");
	
	private string[] pb_run;
	private string[] ils;
	private string peanut_butter;
	private string individual_levels;
	
	//private List<Score> scores = new List<Score>();
	
	public ScoreWindow(string peanut_butter, string individual_levels)
	{
		this.peanut_butter = peanut_butter;
		this.individual_levels = individual_levels;
		
		Font = new Font("Tahoma", 20);
		Text = "Star Fox 64 Score Tracker";
		sInput.Text = "Input";

		Size = new Size(1296, 99);

		//  Set colors
		BackColor = bgColor;
		topScore.ForeColor = text;
		sobScore.ForeColor = text;
		//BackColor = lightColor;
		//rooms.BackColor = bgColor;
		//ForeColor = lightColor;
		//chatLog.BackColor = lightColor;
		//sendButton.BackColor = Color.FromArgb(160, 255, 140);
		//sendButton.Font = new Font("Courier", 9, FontStyle.Bold);
		//inputBox.AutoSize = false;
		//rooms.BackColor = darkColor;
		//rooms.ForeColor = darkColor;
		//inputBox.BackColor = lightColor;

		//  Set up the send button
		//sendButton.Text = "send";
		//sendButton.Click += new EventHandler(HandleInput);

		//  Add controls to the form
		
		
		
		
		
		
		SetControls();
		

		//  Display documentation so user knows how to use the program
		//chatLog.AddMessage("use /connect host port nickname");
		//chatLog.AddMessage("or /host port nickname");

		//  Redraw the form if the window is resized
		Resize += delegate { DoLayout(); };

		//  Draw the form
		DoLayout();
		
		sInput.Show();
		Show();
		
		//  When the form is shown set the focus to the input box

		//  Close the network connection when the form is closed
		//  To prevent any hangups
		//FormClosing += delegate { CloseNetwork(); };
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
	
	private void SetControls()
	{
		Controls.Clear();

		pb_run = peanut_butter.Split('\n');
		ils = individual_levels.Split('\n');
		try
		{
			int total = 0;
			int sob = 0;
			foreach(string s in pb_run)
			{
				string[] split = s.Split(':');
				string n = split[0];
				int sc = Int32.Parse(split[1]);
				total += sc;
				Score newScore = new Score(n, sc);
				Controls.Add(newScore);
				
			}
			
			for(int i = 0; i < ils.Length; i++)
			{
				string[] split = ils[i].Split(':');
				string n = split[0];
				int sc = Int32.Parse(split[1]);
				Score.SetBest(n, sc, i);
				
			}
			
			foreach(Score s in Score.scoresList)
			{
				sob += s.best;
			}
			topScore.Text = "Top: " + total;
			Controls.Add(topScore);
			sobScore.Text = "SoB: " + sob;
			Controls.Add(sobScore);
			
		}
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);
		}
		DoLayout();
	}
	
	public void DoLayout()
	{
		List<Score> sList = Score.scoresList;
		foreach (Score s in sList)
		{
			s.Height = GetHeight();
			s.Width = 135;
		}
		
		for (int i = 1; i < sList.Count; i++)
		{
			sList[i].Left = sList[i-1].Left + 135;
		}
		
		topScore.Left = sList[sList.Count - 1].Left + 135;
		topScore.Width = 150;
		topScore.Height = GetHeight();
		sobScore.Left = topScore.Left + 150;
		sobScore.Width = 150;
		sobScore.Height = GetHeight();
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
		
		
		string peanut_butter = "";
		string individual_levels = ""; 
		string config = "";
		try
		{
			config = File.ReadAllText(@"config.txt", Encoding.UTF8);
			
		}
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);
			string peanut_butter_easy = "CO:0\nME:0\nKA:0\nSX:0\nMA:0\na6:0\nVE:0";
			string peanut_butter_hard = "CO:0\nSY:0\nAQ:0\nZO:0\nMA:0\na6:0\nVE:0";
			individual_levels = "CO:0\nME:0\nKA:0\nSX:0\nSY:0\nAQ:0\nZO:0\nMA:0\na6:0\nVE:0";
			
			try
			{
				var sw = new StreamWriter(@"config.txt");
				sw.Write("Hard Route: 0");
				sw.Close();
				
				sw = new StreamWriter(@"pb_easy.txt");
				sw.Write(peanut_butter_easy);
				sw.Close();
				
				sw = new StreamWriter(@"pb_hard.txt");
				sw.Write(peanut_butter_hard);
				sw.Close();
				
				sw = new StreamWriter(@"pb_individuals.txt");
				sw.Write(individual_levels);
				sw.Close();
			}
			catch (Exception e2)
			{
				Console.WriteLine("Error: " + e2.Message);
			}
		}
		
		try
		{
			if (config == "Hard Route: 0")
			{
				peanut_butter = File.ReadAllText(@"pb_easy.txt", Encoding.UTF8);
			}
			else
			{
				peanut_butter = File.ReadAllText(@"pb_hard.txt", Encoding.UTF8);
			}
			individual_levels = File.ReadAllText(@"pb_individuals.txt", Encoding.UTF8);
			
		}
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);
			peanut_butter = "CO:0\nME:0\nKA:0\nSX:0\nMA:0\na6:0\nVE:0";
			individual_levels = "CO:0\nME:0\nKA:0\nSX:0\nSY:0\nAQ:0\nZO:0\nMA:0\na6:0\nVE:0";
		}
		
		Application.Run(new ScoreWindow(peanut_butter, individual_levels));
		
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