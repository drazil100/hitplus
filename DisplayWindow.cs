using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class DisplayWindow : Form
{
	public DisplayWindowContent dispContent;

	private int w = 0;
	private int h = 0;
	private int w2;
	private int h2;

	public DisplayWindow(DisplayWindowContent cont)
	{
		//this.peanut_butter = peanut_butter;
		//this.individual_levels = individual_levels;
		Text = "Star Fox 64 Score Tracker";


		FormClosing += new FormClosingEventHandler(InputWindow.mainWindow.ConfirmClose);

		//  Set colors
		BackColor = ScoreTracker.colors["background_color"];



		Initialize(cont);

		//  Redraw the form if the window is resized
		Resize += delegate { DoLayout(); };
		//Move += delegate { DoLayout(); };

		//  Draw the form
		DoLayout();

		
		int x = -10000;
		int y = -10000;
		
		try
		{
			x = Int32.Parse(ScoreTracker.config["tracker_x"]);
			y = Int32.Parse(ScoreTracker.config["tracker_y"]);
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


		//  When the form is shown set the focus to the input box

		//  Close the network connection when the form is closed
		//  To prevent any hangups
		//FormClosing += delegate { CloseNetwork(); };
	}

	public void DoResize()
	{
		MinimumSize  = new Size(0, 0);
		if (ScoreTracker.config["layout"] == "0")
		{
			w = 1296;
			h = 99;
			try
			{
				w2 = Int32.Parse (ScoreTracker.config ["horizontal_width"]);
				h2 = Int32.Parse (ScoreTracker.config ["horizontal_height"]);
			}
			catch (Exception)
			{
				w2 = w;
				h2 = h;
			}
			Size = new Size(w2, h2);
		}
		else
		{
			w = 316;
			h = 339;
			try
			{
			w2 = Int32.Parse (ScoreTracker.config ["vertical_width"]);
			h2 = Int32.Parse (ScoreTracker.config ["vertical_height"]);
			}
			catch (Exception)
			{
				w2 = w;
				h2 = h;
			}
			Size = new Size(w2, h2);
		}
		MinimumSize  = new Size(w, h);
	}

	public void Initialize(DisplayWindowContent window)
	{

		dispContent = window;
		Controls.Clear();
		Controls.Add(window);

		DoResize();

		DoLayout();
	}

	public void UpdateContent()
	{
		dispContent.DoLayout();
	}

	public void UpdateScores()
	{
		dispContent.UpdateScores();
	}

	public void ResetContent()
	{
		BackColor = ScoreTracker.colors["background_color"];
		DoResize();
		DoLayout(true);
		dispContent.SetControls();
	}

	public void DoLayout(bool skip = false)
	{
		if (dispContent != null)
		{
			dispContent.Width = Width;
			dispContent.Height = Height;
			if (!skip)
				dispContent.DoLayout();
		}
	}

	public int GetWidth()
	{
		return (
			Width - (2 * SystemInformation.FrameBorderSize.Width)
		);
	}

	public int GetHeight()
	{
		return (
			Height - (2 * SystemInformation.FrameBorderSize.Height +
				SystemInformation.CaptionHeight)
		);
	}
}


public class DisplayWindowContent : Panel
{

	private Panel totals = new Panel();
	private Panel levels = new Panel();
	private List<ScorePanel> panels = new List<ScorePanel>();

	public Label topScore = new Label();
	public Label sobScore = new Label();
	public Label currentScore = new Label();
	public Label topScoreName = new Label();
	public Label sobScoreName = new Label();
	public Label currentScoreName = new Label();

	public DisplayWindowContent()
	{
		Font = new Font(ScoreTracker.config["font"], Int32.Parse(ScoreTracker.config["font_size"]), FontStyle.Bold);

		BackColor = ScoreTracker.colors["background_color"];
		topScoreName.ForeColor = ScoreTracker.colors["text_color_total"];
		sobScoreName.ForeColor = ScoreTracker.colors["text_color_total"];

		topScore.ForeColor = ScoreTracker.colors["text_color_total"];
		sobScore.ForeColor = ScoreTracker.colors["text_color_total"];
		currentScore.ForeColor = ScoreTracker.colors["text_color"];
		currentScoreName.ForeColor = ScoreTracker.colors["text_color"];


		currentScoreName.Text = "Total:";


		SetControls();
		DoLayout();
	}

	public int GetWidth()
	{
		return (
			Width - (2 * SystemInformation.FrameBorderSize.Width)
		);
	}

	public int GetHeight()
	{
		return (
			Height - (2 * SystemInformation.FrameBorderSize.Height +
				SystemInformation.CaptionHeight)
		);
	}

	public void SetControls()
	{
		Font = new Font(ScoreTracker.config["font"], Int32.Parse(ScoreTracker.config["font_size"]), FontStyle.Bold);
		BackColor = ScoreTracker.colors["background_color"];
		topScoreName.ForeColor = ScoreTracker.colors["text_color_total"];
		sobScoreName.ForeColor = ScoreTracker.colors["text_color_total"];
		topScore.ForeColor = ScoreTracker.colors["text_color_total"];
		sobScore.ForeColor = ScoreTracker.colors["text_color_total"];

		totals.Controls.Clear();
		levels.Controls.Clear();
		panels.Clear();
		Controls.Clear();

		TrackerData run = ScoreTracker.Data;
		try
		{
			foreach(ScoreEntry entry in run.GetScoreSet())
			{
				ScorePanel newScore = new ScorePanel(entry);
				levels.Controls.Add(newScore);
				panels.Add(newScore);
			}

			if (ScoreTracker.config["layout"] == "1")
				totals.Controls.Add(currentScoreName);
			totals.Controls.Add(currentScore);
			totals.Controls.Add(topScoreName);
			totals.Controls.Add(topScore);
			totals.Controls.Add(sobScoreName);
			totals.Controls.Add(sobScore);

			if (ScoreTracker.config["sums_horizontal_alignment"] == "1" && ScoreTracker.config["layout"] == "0")
			{
				Controls.Add(totals);
				Controls.Add(levels);
			}
			else
			{
				Controls.Add(levels);
				Controls.Add(totals);
			}
			UpdateScores();

		}
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);
		}
		DoLayout();
	}

	public void DoLayout()
	{
		if (ScoreTracker.config ["layout"] == "0")
		{
			totals.Top = 0;
			totals.Width = 310;
			totals.Height = GetHeight();
			levels.Width = GetWidth () - totals.Width;
			levels.Height = GetHeight();
			DoTotalsLayoutHorizontal ();
			DoLevelsLayoutHorizontal ();

			if (ScoreTracker.config ["sums_horizontal_alignment"] == "1")
			{
				totals.Left = 0;
				levels.Left = totals.Width;
			}
			else
			{
				levels.Left = 0;
				totals.Left = levels.Width;
			}
		}
		else
		{
			//totals.Top = 0;
			levels.Left = 0;
			totals.Width = GetWidth ();
			levels.Width = GetWidth ();
			totals.Height = 90;
			levels.Height = GetHeight () - totals.Height;
			totals.Top = levels.Height;
			DoTotalsLayoutVertical ();
			DoLevelsLayoutVertical ();
		}

		//Refresh ();
	}

	public void UpdateScores()
	{
		ScoreSet set = ScoreTracker.Data.GetScoreSet();
		for (int i = 0; i < panels.Count; i++)
		{
			panels[i].entry = set[i];
			panels[i].UpdatePanel();
		}
		TrackerData run = ScoreTracker.Data;
		int total = run.GetScoreSet(0).GetComparisonTotal();
		if (ScoreTracker.config["casual_mode"] == "0")
		{
			total = run.GetScoreSet().GetComparisonTotal();
		}
		int sob = run.GetScoreSet(1).GetComparisonTotal();
		if (ScoreTracker.config["layout"] == "0")
		{
			sobScoreName.Text = "SoB:";
			if (ScoreTracker.Data.GetComparisonIndex() == 0 || ScoreTracker.config["casual_mode"] == "1")
			{
				topScoreName.Text = "Top:";
			}
			else if (ScoreTracker.Data.GetComparisonIndex() == 1)
			{
				topScoreName.Text = "-";
			}
			else
			{
				topScoreName.Text = "Cmp:";
			}
		}
		else
		{
			sobScoreName.Text = "Sum of Best:";
			if (ScoreTracker.Data.GetComparisonIndex() == 0 || ScoreTracker.config["casual_mode"] == "1")
			{
				topScoreName.Text = "Top:";
			}
			else if (ScoreTracker.Data.GetComparisonIndex() == 1)
			{
				topScoreName.Text = "-";
			}
			else
			{
				topScoreName.Text = "Comparison:";
			}
		}

		if (ScoreTracker.Data.GetComparisonIndex() != 1)
		{
			topScore.Text = "" + total;
		}
		else
		{
			topScore.Text = "-";
		}
		sobScore.Text = "" + sob;
		sobScore.Text = "" + ScoreTracker.Tracker.GetSOB();


		if (!ScoreTracker.Tracker.IsRunning() && ScoreTracker.config["layout"] == "0")
		{
			currentScore.Text = "";
		}
		else
		{
			currentScore.Text = "" + ScoreTracker.Tracker.GetTotalScore();
			if (ScoreTracker.Tracker.GetTotalScore() == 0)
				currentScore.Text = "-";
		}
		Color p = ScoreTracker.colors["text_color"];
		if (ScoreTracker.Tracker.IsRunning() && ScoreTracker.config["casual_mode"] == "0")
			p = GetPaceColor(ScoreTracker.Tracker.GetCurrentPace());
		currentScore.ForeColor = p;
		currentScoreName.Text = "Total:";
	}

	public Color GetPaceColor(PaceStatus status)
	{
		Color tmp = ScoreTracker.colors["text_color"];
		switch (status)
		{
			case PaceStatus.Ahead:   tmp = ScoreTracker.colors["text_color_ahead"]; break;
			case PaceStatus.Behind:  tmp = ScoreTracker.colors["text_color_behind"]; break;
		}
		return tmp;
	}


	public void DoTotalsLayoutHorizontal()
	{
		topScoreName.Top = 0;
		topScore.Top = 0;
		//topScoreName.Left = 0;
		//topScore.Left = 0;
		sobScoreName.Top = 0;
		sobScore.Top = 0;
		//currentScore.Left = 0;
		//currentScore.Top = 0;

		topScoreName.Width = 75;
		topScore.Left = topScoreName.Width;
		topScore.Width = 155 - topScoreName.Width;
		topScoreName.Height = GetHeight() / 2;
		topScore.Height = GetHeight() / 2;
		sobScoreName.Left = topScore.Left + topScore.Width;
		sobScore.Left = sobScoreName.Left + sobScoreName.Width;
		sobScoreName.Width = 75;
		sobScore.Width = 155 - sobScoreName.Width;
		sobScoreName.Height = GetHeight() / 2;
		sobScore.Height = GetHeight() / 2;
		currentScore.Top = topScore.Top + topScore.Height;
		currentScore.Left = topScore.Left;
		currentScore.Height = GetHeight() / 2;
		currentScore.Width = topScore.Width;
		topScore.TextAlign = ContentAlignment.TopLeft;
		sobScore.TextAlign = ContentAlignment.TopLeft;
		currentScore.TextAlign = ContentAlignment.TopLeft;

	}

	public void DoLevelsLayoutHorizontal()
	{
		foreach (ScorePanel panel in panels)
		{
			panel.Height = GetHeight();
			panel.Width = levels.Width / ScoreTracker.Data.Count;
			panel.UpdatePanel();
		}

		for (int i = 1; i < panels.Count; i++)
		{
			panels[i].Left = panels[i-1].Left + levels.Width / ScoreTracker.Data.Count;
		}
	}

	public void DoTotalsLayoutVertical()
	{
		//topScoreName.Top = 0;
		//topScore.Top = 0;
		topScoreName.Left = 0;
		//topScore.Left = 0;

		//sobScore.Top = 0;
		sobScoreName.Left = 0;
		//sobScore.Left = 0;
		//currentScore.Left = 0;
		currentScore.Top = 0;

		topScore.Top = 30;
		topScoreName.Top = 30;
		topScoreName.Width = 220;
		topScore.Width = GetWidth() - topScoreName.Width;
		topScore.Height = 30;
		topScoreName.Height = topScore.Height;
		topScore.Left = topScoreName.Width;
		sobScoreName.Width = 220;
		sobScoreName.Top = 60;
		sobScore.Top = 60;
		sobScore.Width = GetWidth() - sobScoreName.Width;
		sobScore.Height = 30;
		sobScore.Left = sobScoreName.Width;
		sobScoreName.Height = sobScore.Height;
		currentScoreName.Width = 220;
		currentScore.Width = GetWidth() - currentScoreName.Width;
		currentScore.Left = currentScoreName.Width;
		currentScoreName.Height = 30;
		currentScore.Height = 30;
		topScore.TextAlign = ContentAlignment.TopRight;
		sobScore.TextAlign = ContentAlignment.TopRight;
		currentScore.TextAlign = ContentAlignment.TopRight;
	}

	public void DoLevelsLayoutVertical()
	{
		foreach (ScorePanel panel in panels)
		{
			panel.Height = levels.Height / ScoreTracker.Data.Count;
			panel.Width = GetWidth();
			panel.UpdatePanel();
		}

		for (int i = 1; i < panels.Count; i++)
		{
			panels[i].Top = panels[i-1].Top + panels[i-1].Height;
		}
	}
}

