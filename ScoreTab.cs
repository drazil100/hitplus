using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class ScoreSubTab : TabPage
{
	private FileReader file;
	private string section;
	private List<NumericField> scores = new List<NumericField>();

	private Label totalName = new Label();
	private Label total = new Label();

	public ScoreSubTab(FileReader file, string section)
	{
		this.file = file;
		this.section = section;
		Text = section;
		totalName.Text = "Total:";
		Controls.Add(totalName);
		Controls.Add(total);
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
		int tot = 0;
		foreach (NumericField score in scores)
		{
			score.Width = ClientRectangle.Width;
			tot += Int32.Parse(score.Number);
			//s.DoLayout ();
		}
		totalName.Top = scores[scores.Count - 1].Top + scores[scores.Count -1].Height;
		total.Top = totalName.Top;
		totalName.Width = ClientRectangle.Width/2;
		total.Width = totalName.Width;
		total.Left = totalName.Width;
		total.TextAlign = ContentAlignment.TopRight;

		total.Text = "" + tot;
	}
}

public class ScoreTab : Panel
{
	private FileReader file;

	private TabControl tabs = new TabControl();
	private List<ScoreSubTab> pages = new List<ScoreSubTab>();

	public ScoreTab(FileReader file)
	{
		this.file = file;
		TrackerData.ValidateFile(file);

		Text = "Scores";

		Controls.Add(tabs);

		pages.Add(ConfigureTab("Best Run"));
		pages.Add(ConfigureTab("Top Scores"));
		tabs.TabPages.Add(pages[0]);
		tabs.TabPages.Add(pages[1]);
		foreach (string section in file.Sections)
		{
			if (section == "Best Run" || section == "Top Scores" || section == "General")
				continue;

			pages.Add(ConfigureTab(section));
			tabs.TabPages.Add(pages[pages.Count - 1]);
		}
		Resize += delegate { DoLayout(); };

		DoLayout();
	}

	public void Save()
	{
		foreach (ScoreSubTab page in pages)
		{
			page.SaveScores();
		}
		file.Save();
	}

	public void DoLayout()
	{
		tabs.Width = ClientRectangle.Width;
		tabs.Height = ClientRectangle.Height;
		foreach (ScoreSubTab page in pages)
		{
			page.Width = tabs.ClientRectangle.Width;
			page.Height = tabs.ClientRectangle.Height - 25;
			page.DoLayout();
		}
	}

	private ScoreSubTab ConfigureTab (string section)
	{
		ScoreSubTab page = new ScoreSubTab(file, section);
		foreach (KeyValuePair<string, string> score in file.GetSection(section))
		{

			page.Add(new NumericField (score.Key, score.Value));

		}
		return page;
	}
}
