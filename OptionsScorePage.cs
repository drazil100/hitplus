using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class OptionsScorePage : Panel
{
	private FileReader file;
	private string section;
	private List<NumericField> scores = new List<NumericField>();

	private Label totalName = new Label();
	private Label total = new Label();
	private Panel totalPanel = new Panel();

	public OptionsScorePage(FileReader file, string section)
	{
		this.file = file;
		this.section = section;
		Text = section;
		totalName.Text = "Total:";
		Dock = DockStyle.Fill;

		totalPanel.Height = 25;
		Controls.Add(totalPanel);
		totalPanel.Controls.Add(totalName);
		totalPanel.Controls.Add(total);
		totalName.Dock = DockStyle.Fill;
		total.Dock = DockStyle.Right;

		//Resize += delegate { DoLayout(); };
		Layout += new LayoutEventHandler((object sender, LayoutEventArgs e) => DoLayout());
	}

	public void Add(NumericField score)
	{
		if (scores.Count > 0)
		{
			score.Top = scores [scores.Count - 1].Top + scores [scores.Count - 1].Height;
		}
		score.Width = ClientRectangle.Width;
		scores.Add(score);
		Controls.Add(score);
		score.Changed = DoLayout;
	}

	public void SaveScores()
	{
		foreach (NumericField score in scores)
		{
			file [section, score.Name] = score.Number;
		}
	}

	public void DoLayout()
	{
		Dock = DockStyle.Fill;
		int scrollPadding = 0;
		if (scores.Count*20+25 > ClientRectangle.Height) scrollPadding = SystemInformation.VerticalScrollBarWidth + 2;

		int tot = 0;
		foreach (NumericField score in scores)
		{
			score.Width = ClientRectangle.Width - scrollPadding;
			tot += Int32.Parse(score.Number);
			//s.DoLayout ();
		}
		totalPanel.Top = scores[scores.Count - 1].Top + scores[scores.Count -1].Height + 10;
		totalPanel.Width = ClientRectangle.Width - 4 - scrollPadding;

		total.Text = "" + tot;
		total.TextAlign = ContentAlignment.TopRight;
		
		AutoScroll = true;
	}
}

