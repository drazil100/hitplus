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

		Font = new Font(ScoreTracker.config["font"], Int32.Parse(ScoreTracker.config["font_size"]), FontStyle.Bold);
		Text = "Star Fox 64 Score Tracker";


		FormClosing += new FormClosingEventHandler(ScoreTracker.mainWindow.ConfirmClose);

		//  Set colors
		BackColor = ScoreTracker.background_color;
		ScoreTracker.topScoreName.ForeColor = ScoreTracker.text_color_total;
		ScoreTracker.sobScoreName.ForeColor = ScoreTracker.text_color_total;

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

	public void Initialize(DisplayWindowContent window)
	{
		BackColor = ScoreTracker.background_color;
		ScoreTracker.topScoreName.ForeColor = ScoreTracker.text_color_total;
		ScoreTracker.sobScoreName.ForeColor = ScoreTracker.text_color_total;

		dispContent = window;
		Controls.Clear();
		Controls.Add(window);

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
		DoLayout();
	}

	public void DoLayout()
	{
		if (dispContent != null)
		{
			dispContent.Width = Width;
			dispContent.Height = Height;
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



	public DisplayWindowContent()
	{
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
		totals.Controls.Clear();
		levels.Controls.Clear();
		Controls.Clear();
		Score.ClearScores ();

		FileReader run = ScoreTracker.pbEasy;
		if (ScoreTracker.config["hard_route"] == "1")
		{
			run = ScoreTracker.pbHard;
		}
		try
		{
			int total = 0;
			int sob = 0;
			foreach(KeyValuePair<string, string> level in run)
			{
				int sc = Int32.Parse(level.Value);
				total += sc;
				Score newScore = new Score(level.Key, sc);
				levels.Controls.Add(newScore);

			}

			int i = 0;
			foreach(KeyValuePair<string, string> level in ScoreTracker.individualLevels)
			{
				Score.SetBest(level.Key, Int32.Parse(level.Value), i);
				i++;
			}

			foreach(Score s in Score.scoresList)
			{
				sob += s.best;
			}
			ScoreTracker.topScoreName.Text = "Top: ";
			ScoreTracker.topScore.Text = "" + total;
			if (ScoreTracker.config["layout"] == "1")
				totals.Controls.Add(ScoreTracker.currentScoreName);
			totals.Controls.Add(ScoreTracker.currentScore);
			totals.Controls.Add(ScoreTracker.topScoreName);
			totals.Controls.Add(ScoreTracker.topScore);
			if (ScoreTracker.config["layout"] == "0")
			{
				ScoreTracker.sobScoreName.Text = "SoB:";
				ScoreTracker.sobScore.Text = "" + sob;
			}
			else
			{
				ScoreTracker.sobScore.Text = "" + sob;
				ScoreTracker.sobScoreName.Text = "Sum of Best:";
			}
			totals.Controls.Add(ScoreTracker.sobScoreName);
			totals.Controls.Add(ScoreTracker.sobScore);

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
			totals.Width = 310;
			levels.Width = GetWidth () - totals.Width;
			DoTotalsLayoutHorizontal ();
			DoLevelsLayoutHorizontal ();

			if (ScoreTracker.config ["sums_horizontal_alignment"] == "1")
			{
				levels.Left = totals.Width;
			}
			else
			{
				totals.Left = levels.Width;
			}
		}
		else
		{
			totals.Width = GetWidth ();
			levels.Width = GetWidth ();
			totals.Height = 90;
			levels.Height = GetHeight () - totals.Height;
			totals.Top = levels.Height;
			DoTotalsLayoutVertical ();
			DoLevelsLayoutVertical ();
		}

		Refresh ();
	}

	public void DoTotalsLayoutHorizontal()
	{
		ScoreTracker.topScoreName.Top = 0;
		ScoreTracker.topScore.Top = 0;
		ScoreTracker.topScoreName.Left = 0;
		ScoreTracker.topScore.Left = 0;
		ScoreTracker.sobScoreName.Top = 0;
		ScoreTracker.sobScore.Top = 0;
		ScoreTracker.sobScoreName.Left = 0;
		ScoreTracker.sobScore.Left = 0;
		ScoreTracker.currentScore.Left = 0;
		ScoreTracker.currentScore.Top = 0;

		ScoreTracker.topScoreName.Width = 75;
		ScoreTracker.topScore.Left = ScoreTracker.topScoreName.Width;
		ScoreTracker.topScore.Width = 155 - ScoreTracker.topScoreName.Width;
		ScoreTracker.topScoreName.Height = GetHeight() / 2;
		ScoreTracker.topScore.Height = GetHeight() / 2;
		ScoreTracker.sobScoreName.Left = ScoreTracker.topScore.Left + ScoreTracker.topScore.Width;
		ScoreTracker.sobScore.Left = ScoreTracker.sobScoreName.Left + ScoreTracker.sobScoreName.Width;
		ScoreTracker.sobScoreName.Width = 75;
		ScoreTracker.sobScore.Width = 155 - ScoreTracker.sobScoreName.Width;
		ScoreTracker.sobScoreName.Height = GetHeight() / 2;
		ScoreTracker.sobScore.Height = GetHeight() / 2;
		ScoreTracker.currentScore.Top = ScoreTracker.topScore.Top + ScoreTracker.topScore.Height;
		ScoreTracker.currentScore.Left = ScoreTracker.topScore.Left;
		ScoreTracker.currentScore.Height = GetHeight() / 2;
		ScoreTracker.currentScore.Width = ScoreTracker.topScore.Width;
		ScoreTracker.topScore.TextAlign = ContentAlignment.TopLeft;
		ScoreTracker.sobScore.TextAlign = ContentAlignment.TopLeft;

	}

	public void DoLevelsLayoutHorizontal()
	{
		List<Score> sList = Score.scoresList;
		foreach (Score s in sList)
		{
			s.Height = GetHeight();
			s.Width = levels.Width / 7;
		}

		for (int i = 1; i < sList.Count; i++)
		{
			sList[i].Left = sList[i-1].Left + levels.Width / 7;
		}
	}

	public void DoTotalsLayoutVertical()
	{
		ScoreTracker.topScoreName.Top = 0;
		ScoreTracker.topScore.Top = 0;
		ScoreTracker.topScoreName.Left = 0;
		ScoreTracker.topScore.Left = 0;
		ScoreTracker.sobScoreName.Top = 0;
		ScoreTracker.sobScore.Top = 0;
		ScoreTracker.sobScoreName.Left = 0;
		ScoreTracker.sobScore.Left = 0;
		ScoreTracker.currentScore.Left = 0;
		ScoreTracker.currentScore.Top = 0;

		ScoreTracker.topScore.Top = 30;
		ScoreTracker.topScoreName.Top = 30;
		ScoreTracker.topScoreName.Width = 220;
		ScoreTracker.topScore.Width = GetWidth() - ScoreTracker.topScoreName.Width;
		ScoreTracker.topScore.Height = 30;
		ScoreTracker.topScoreName.Height = ScoreTracker.topScore.Height;
		ScoreTracker.topScore.Left = ScoreTracker.topScoreName.Width;
		ScoreTracker.sobScoreName.Width = 220;
		ScoreTracker.sobScoreName.Top = 60;
		ScoreTracker.sobScore.Top = 60;
		ScoreTracker.sobScore.Width = GetWidth() - ScoreTracker.sobScoreName.Width;
		ScoreTracker.sobScore.Height = 30;
		ScoreTracker.sobScore.Left = ScoreTracker.sobScoreName.Width;
		ScoreTracker.sobScoreName.Height = ScoreTracker.sobScore.Height;
		ScoreTracker.currentScoreName.Width = 220;
		ScoreTracker.currentScore.Width = GetWidth() - ScoreTracker.currentScoreName.Width;
		ScoreTracker.currentScore.Left = ScoreTracker.currentScoreName.Width;
		ScoreTracker.currentScoreName.Height = 30;
		ScoreTracker.currentScore.Height = 30;
		ScoreTracker.topScore.TextAlign = ContentAlignment.TopRight;
		ScoreTracker.sobScore.TextAlign = ContentAlignment.TopRight;
		ScoreTracker.currentScore.TextAlign = ContentAlignment.TopRight;
	}

	public void DoLevelsLayoutVertical()
	{
		List<Score> sList = Score.scoresList;
		foreach (Score s in sList)
		{
			if (ScoreTracker.config ["vertical_scale_mode"] == "1")
			{	
				s.Height = 30;
			}
			else
			{
				s.Height = levels.Height / 7;
			}
			s.Width = GetWidth();
		}

		for (int i = 1; i < sList.Count; i++)
		{
			sList[i].Top = sList[i-1].Top + sList[i-1].Height;
		}
	}
}

