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
	public void ResetContent()
	{
		BackColor = ScoreTracker.colors["background_color"];
		InputWindow.topScoreName.ForeColor = ScoreTracker.colors["text_color_total"];
		InputWindow.sobScoreName.ForeColor = ScoreTracker.colors["text_color_total"];
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


	public DisplayWindowContent()
	{
		Font = new Font(ScoreTracker.config["font"], Int32.Parse(ScoreTracker.config["font_size"]), FontStyle.Bold);

		BackColor = ScoreTracker.colors["background_color"];
		InputWindow.topScoreName.ForeColor = ScoreTracker.colors["text_color_total"];
		InputWindow.sobScoreName.ForeColor = ScoreTracker.colors["text_color_total"];

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
		InputWindow.topScoreName.ForeColor = ScoreTracker.colors["text_color_total"];
		InputWindow.sobScoreName.ForeColor = ScoreTracker.colors["text_color_total"];

		totals.Controls.Clear();
		levels.Controls.Clear();
		panels.Clear();
		Controls.Clear();

		TrackerData run = ScoreTracker.Data;
		try
		{
			int total = run.GetScoreSet().GetComparisonTotal();
			int sob = run.GetScoreSet(1).GetComparisonTotal();
			foreach(ScoreEntry entry in run.GetScoreSet())
			{
				ScorePanel newScore = new ScorePanel(entry);
				levels.Controls.Add(newScore);
				panels.Add(newScore);
			}

			InputWindow.topScoreName.Text = "Top: ";
			InputWindow.topScore.Text = "" + total;
			if (ScoreTracker.config["layout"] == "1")
				totals.Controls.Add(InputWindow.currentScoreName);
			totals.Controls.Add(InputWindow.currentScore);
			totals.Controls.Add(InputWindow.topScoreName);
			totals.Controls.Add(InputWindow.topScore);
			if (ScoreTracker.config["layout"] == "0")
			{
				InputWindow.sobScoreName.Text = "SoB:";
				InputWindow.sobScore.Text = "" + sob;
			}
			else
			{
				InputWindow.sobScore.Text = "" + sob;
				InputWindow.sobScoreName.Text = "Sum of Best:";
			}
			totals.Controls.Add(InputWindow.sobScoreName);
			totals.Controls.Add(InputWindow.sobScore);

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
			totals.Top = 0;
			totals.Width = 310;
			levels.Width = GetWidth () - totals.Width;
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

	public void DoTotalsLayoutHorizontal()
	{
		InputWindow.topScoreName.Top = 0;
		InputWindow.topScore.Top = 0;
		//InputWindow.topScoreName.Left = 0;
		//InputWindow.topScore.Left = 0;
		InputWindow.sobScoreName.Top = 0;
		InputWindow.sobScore.Top = 0;
		//InputWindow.currentScore.Left = 0;
		//InputWindow.currentScore.Top = 0;

		InputWindow.topScoreName.Width = 75;
		InputWindow.topScore.Left = InputWindow.topScoreName.Width;
		InputWindow.topScore.Width = 155 - InputWindow.topScoreName.Width;
		InputWindow.topScoreName.Height = GetHeight() / 2;
		InputWindow.topScore.Height = GetHeight() / 2;
		InputWindow.sobScoreName.Left = InputWindow.topScore.Left + InputWindow.topScore.Width;
		InputWindow.sobScore.Left = InputWindow.sobScoreName.Left + InputWindow.sobScoreName.Width;
		InputWindow.sobScoreName.Width = 75;
		InputWindow.sobScore.Width = 155 - InputWindow.sobScoreName.Width;
		InputWindow.sobScoreName.Height = GetHeight() / 2;
		InputWindow.sobScore.Height = GetHeight() / 2;
		InputWindow.currentScore.Top = InputWindow.topScore.Top + InputWindow.topScore.Height;
		InputWindow.currentScore.Left = InputWindow.topScore.Left;
		InputWindow.currentScore.Height = GetHeight() / 2;
		InputWindow.currentScore.Width = InputWindow.topScore.Width;
		InputWindow.topScore.TextAlign = ContentAlignment.TopLeft;
		InputWindow.sobScore.TextAlign = ContentAlignment.TopLeft;
		InputWindow.currentScore.TextAlign = ContentAlignment.TopLeft;

	}

	public void DoLevelsLayoutHorizontal()
	{
		foreach (ScorePanel panel in panels)
		{
			panel.Height = GetHeight();
			panel.Width = levels.Width / 7;
			panel.UpdatePanel();
		}

		for (int i = 1; i < panels.Count; i++)
		{
			panels[i].Left = panels[i-1].Left + levels.Width / 7;
		}
	}

	public void DoTotalsLayoutVertical()
	{
		//InputWindow.topScoreName.Top = 0;
		//InputWindow.topScore.Top = 0;
		InputWindow.topScoreName.Left = 0;
		//InputWindow.topScore.Left = 0;

		//InputWindow.sobScore.Top = 0;
		InputWindow.sobScoreName.Left = 0;
		//InputWindow.sobScore.Left = 0;
		//InputWindow.currentScore.Left = 0;
		InputWindow.currentScore.Top = 0;

		InputWindow.topScore.Top = 30;
		InputWindow.topScoreName.Top = 30;
		InputWindow.topScoreName.Width = 220;
		InputWindow.topScore.Width = GetWidth() - InputWindow.topScoreName.Width;
		InputWindow.topScore.Height = 30;
		InputWindow.topScoreName.Height = InputWindow.topScore.Height;
		InputWindow.topScore.Left = InputWindow.topScoreName.Width;
		InputWindow.sobScoreName.Width = 220;
		InputWindow.sobScoreName.Top = 60;
		InputWindow.sobScore.Top = 60;
		InputWindow.sobScore.Width = GetWidth() - InputWindow.sobScoreName.Width;
		InputWindow.sobScore.Height = 30;
		InputWindow.sobScore.Left = InputWindow.sobScoreName.Width;
		InputWindow.sobScoreName.Height = InputWindow.sobScore.Height;
		InputWindow.currentScoreName.Width = 220;
		InputWindow.currentScore.Width = GetWidth() - InputWindow.currentScoreName.Width;
		InputWindow.currentScore.Left = InputWindow.currentScoreName.Width;
		InputWindow.currentScoreName.Height = 30;
		InputWindow.currentScore.Height = 30;
		InputWindow.topScore.TextAlign = ContentAlignment.TopRight;
		InputWindow.sobScore.TextAlign = ContentAlignment.TopRight;
		InputWindow.currentScore.TextAlign = ContentAlignment.TopRight;
	}

	public void DoLevelsLayoutVertical()
	{
		foreach (ScorePanel panel in panels)
		{
			panel.Height = levels.Height / 7;
			panel.Width = GetWidth();
			panel.UpdatePanel();
		}

		for (int i = 1; i < panels.Count; i++)
		{
			panels[i].Top = panels[i-1].Top + panels[i-1].Height;
		}
	}
}

