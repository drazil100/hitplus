using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class ScoreField : Panel
{
	private Label name = new Label ();
	private NumericTextBox score = new NumericTextBox ();

	public string Level
	{
		get { return name.Text; }
	}

	public string Score
	{
		get { return score.Text; }
	}

	public ScoreField(KeyValuePair<string, string> il)
	{
		name.Text = il.Key;
		score.Text = il.Value;

		Controls.Add(name);
		Controls.Add(score);

		Height = 20;

		Resize += delegate { DoLayout(); };

		DoLayout();
	}

	public void DoLayout()
	{
		score.Width = 60;
		score.Height = Height;
		name.Width = Width - score.Width;
		name.Height = Height;
		score.Left = name.Width;
	}
}

public class OptionsWindow : Form
{
	
	private FileReader config;
	private FileReader ils;

	private TabControl tabs = new TabControl();

	private TabPage individual_levels = new TabPage();
	private List<ScoreField> scores = new List<ScoreField>();
	private Button saveILs = new Button();

	//private TabPage dislayConfig = new TabPage ();
	//private Button saveDisplayConfig = new Button ();

	private int w = 300;
	private int h = 400;

	public OptionsWindow()
	{
		Text = "Options";
		config = ScoreTracker.config;
		ils = ScoreTracker.individualLevels;

		Controls.Add (tabs);
		individual_levels.Text = "Level PBs";
		tabs.TabPages.Add (individual_levels);
		saveILs.Text = "Save";
		saveILs.Click += new EventHandler(SaveILs);

		Size = new Size (w, h);
		MaximumSize = Size;
		MinimumSize = Size;

		Resize += delegate { DoLayout(); };

		ConfigueILsPage ();

		DoLayout ();
	}

	public void SaveILs(object sender, EventArgs e)
	{
		foreach (ScoreField s in scores)
		{
			ils [s.Level] = s.Score;
		}
		ils.Save ();
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

	public void DoLayout()
	{
		tabs.Width = GetWidth ();
		tabs.Height = GetHeight ();
		foreach (TabPage page in tabs.TabPages)
		{
			page.Width = tabs.Width;
			page.Height = tabs.Height - 25;
		}

		foreach (ScoreField s in scores)
		{
			s.Width = individual_levels.Width;
			//s.DoLayout ();
		}

		saveILs.Width = individual_levels.Width;
		saveILs.Height = 20;
		saveILs.Top = individual_levels.Height - 20;
	}

	private void ConfigueILsPage ()
	{
		individual_levels.Controls.Clear ();
		individual_levels.Controls.Add (saveILs);
		foreach (KeyValuePair<string, string> il in ils)
		{
			if (il.Key != "Easy Route" && il.Key != "Hard Route")
			{
				ScoreField newScore = new ScoreField (il);
				individual_levels.Controls.Add (newScore);
				if (scores.Count > 0)
				{
					newScore.Top = scores [scores.Count - 1].Top + scores [scores.Count - 1].Height;
				}
				newScore.Width = Width;
				scores.Add (newScore);
			}
		}
	}
}