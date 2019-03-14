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
	private Panel totalPanel = new Panel();

	public ScoreSubTab(FileReader file, string section)
	{
		this.file = file;
		this.section = section;
		Text = section;
		totalName.Text = "Total:";
		BorderStyle = BorderStyle.None;

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
		int tot = 0;
		foreach (NumericField score in scores)
		{
			score.Width = ClientRectangle.Width;
			tot += Int32.Parse(score.Number);
			//s.DoLayout ();
		}
		totalPanel.Top = scores[scores.Count - 1].Top + scores[scores.Count -1].Height + 10;
		totalPanel.Width = ClientRectangle.Width - 4;

		total.Text = "" + tot;
		total.TextAlign = ContentAlignment.TopRight;
	}
}

public class ScoreTab : Panel
{
	private FileReader file;

	private TabControl tabs = new TabControl();
	private List<ScoreSubTab> pages = new List<ScoreSubTab>();
	private TabPage comparisons = new TabPage();
	private TabControl comparisonsTabs = new TabControl();
	private ComparisonSelector selector;

	public ScoreTab(FileReader file)
	{
		this.file = file;
		TrackerData.ValidateFile(file);

		Text = "Scores";
		Dock = DockStyle.Fill;

		Controls.Add(tabs);

		pages.Add(ConfigureTab("Best Run"));
		pages.Add(ConfigureTab("Top Scores"));
		tabs.TabPages.Add(pages[0]);
		tabs.TabPages.Add(pages[1]);
		comparisons.Text = "Comparisons";
		selector = new ComparisonSelector();
		selector.Dock = DockStyle.Top;
		comparisons.Controls.Add(comparisonsTabs);
		//comparisons.Controls.Add(selector);
		comparisonsTabs.Dock = DockStyle.Fill;

		foreach (string section in file.Sections)
		{
			if (section == "Best Run" || section == "Top Scores" || section == "General")
				continue;

			pages.Add(ConfigureTab(section));
			comparisonsTabs.TabPages.Add(pages[pages.Count - 1]);
		}
		if (comparisonsTabs.TabPages.Count > 0)
			tabs.TabPages.Add(comparisons);



		Layout += new LayoutEventHandler((object sender, LayoutEventArgs e) => DoLayout());

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
		tabs.Dock = DockStyle.Fill;
		foreach (ScoreSubTab page in pages)
		{
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
