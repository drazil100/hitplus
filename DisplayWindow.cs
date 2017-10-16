using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class DisplayWindow : Form
{

	private Panel totals = new Panel();
	private Panel levels = new Panel();
	public Label topScoreName = new Label();
	public Label sobScoreName = new Label();

	public DisplayWindow()
	{
		//this.peanut_butter = peanut_butter;
		//this.individual_levels = individual_levels;

		Font = new Font(ScoreTracker.config["font"], Int32.Parse(ScoreTracker.config["font_size"]), FontStyle.Bold);
		Text = "Star Fox 64 Score Tracker";

		if (ScoreTracker.config["layout"] == "horizontal")
		{
			Size = new Size(1296, 99);
		}
		else
		{
			Size = new Size(316, 309);
		}

		//  Set colors
		BackColor = ScoreTracker.background_color;
		topScoreName.ForeColor = ScoreTracker.text_color_total;
		sobScoreName.ForeColor = ScoreTracker.text_color_total;



		SetControls();


		//  Redraw the form if the window is resized
		Resize += delegate { DoLayout(); };
		Move += delegate { DoLayout();};

		//  Draw the form
		DoLayout();

		Show();

		//  When the form is shown set the focus to the input box

		//  Close the network connection when the form is closed
		//  To prevent any hangups
		//FormClosing += delegate { CloseNetwork(); };
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

	private void SetControls()
	{
		Controls.Clear();

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
			topScoreName.Text = "Top: ";
			ScoreTracker.topScore.Text = "" + total;
			totals.Controls.Add(topScoreName);
			totals.Controls.Add(ScoreTracker.topScore);
			if (ScoreTracker.config["layout"] == "horizontal")
			{
				sobScoreName.Text = "SoB:";
				ScoreTracker.sobScore.Text = "" + sob;
			}
			else
			{
				ScoreTracker.sobScore.Text = "" + sob;
				sobScoreName.Text = "Sum of Best:";
			}
			totals.Controls.Add(sobScoreName);
			totals.Controls.Add(ScoreTracker.sobScore);

			if (ScoreTracker.config["sums_horizontal_alignment"] == "left" && ScoreTracker.config["layout"] == "horizontal")
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
		if (ScoreTracker.config ["layout"] == "horizontal")
		{
			totals.Width = 310;
			levels.Width = GetWidth () - totals.Width;
			DoTotalsLayoutHorizontal ();
			DoLevelsLayoutHorizontal ();

			if (ScoreTracker.config ["sums_horizontal_alignment"] == "left")
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
			totals.Height = 60;
			levels.Height = GetHeight () - totals.Height;
			totals.Top = levels.Height;
			DoTotalsLayoutVertical ();
			DoLevelsLayoutVertical ();
		}

		Refresh ();
	}

	public void DoTotalsLayoutHorizontal()
	{
		topScoreName.Width = 75;
		ScoreTracker.topScore.Left = topScoreName.Width;
		ScoreTracker.topScore.Width = 155 - topScoreName.Width;
		topScoreName.Height = GetHeight();
		ScoreTracker.topScore.Height = GetHeight();
		sobScoreName.Left = ScoreTracker.topScore.Left + ScoreTracker.topScore.Width;
		ScoreTracker.sobScore.Left = sobScoreName.Left + sobScoreName.Width;
		sobScoreName.Width = 75;
		ScoreTracker.sobScore.Width = 155 - sobScoreName.Width;
		sobScoreName.Height = GetHeight();
		ScoreTracker.sobScore.Height = GetHeight();

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
		topScoreName.Width = 220;
		ScoreTracker.topScore.Width = GetWidth() - topScoreName.Width;
		ScoreTracker.topScore.Height = 30;
		topScoreName.Height = ScoreTracker.topScore.Height;
		ScoreTracker.topScore.Left = topScoreName.Width;
		sobScoreName.Width = 220;
		sobScoreName.Top = 30;
		ScoreTracker.sobScore.Top = 30;
		ScoreTracker.sobScore.Width = GetWidth() - sobScoreName.Width;
		ScoreTracker.sobScore.Height = 30;
		ScoreTracker.sobScore.Left = sobScoreName.Width;
		sobScoreName.Height = ScoreTracker.sobScore.Height;
		ScoreTracker.topScore.TextAlign = ContentAlignment.TopRight;
		ScoreTracker.sobScore.TextAlign = ContentAlignment.TopRight;
	}

	public void DoLevelsLayoutVertical()
	{
		List<Score> sList = Score.scoresList;
		foreach (Score s in sList)
		{
			s.Height = 30;
			s.Width = GetWidth();
		}

		for (int i = 1; i < sList.Count; i++)
		{
			sList[i].Top = sList[i-1].Top + 30;
		}
	}
}

