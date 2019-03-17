using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class ScoreTab : Panel
{
	private FileReader file;

	private TabControl tabs = new TabControl();
	private List<ScorePage> pages = new List<ScorePage>();
	private TabPage comparisons = new TabPage();
	private TabControl comparisonsTabs = new TabControl();
	private ComparisonSelector selector;
	private Panel currentComparison = new Panel();

	public ScoreTab(FileReader file)
	{
		this.file = file;
		TrackerData.ValidateFile(file);

		Text = "Scores";
		Dock = DockStyle.Fill;

		Controls.Add(tabs);

		pages.Add(ConfigureTab("Best Run"));
		pages.Add(ConfigureTab("Top Scores"));
		tabs.TabPages.Add(ToTabPage(pages[0]));
		tabs.TabPages.Add(ToTabPage(pages[1]));
		comparisons.Text = "Comparisons";
		selector = new ComparisonSelector();
		selector.JustComparisons = true;
		selector.Dock = DockStyle.Top;
		currentComparison.Dock = DockStyle.Fill;
		comparisons.Controls.Add(currentComparison);
		comparisons.Controls.Add(selector);

		selector.Changed = ReloadComparisons;
		selector.Reloaded = ReloadComparisons;

		ReloadComparisons();

		tabs.TabPages.Add(comparisons);

		Layout += new LayoutEventHandler((object sender, LayoutEventArgs e) => DoLayout());

		DoLayout();
	}

	public void Save()
	{
		foreach (ScorePage page in pages)
		{
			page.SaveScores();
		}
		file.Save();
	}

	public void DoLayout()
	{
		tabs.Dock = DockStyle.Fill;
		foreach (ScorePage page in pages)
		{
			page.DoLayout();
		}
	}

	private TabPage ToTabPage(ScorePage page)
	{
		TabPage toReturn = new TabPage();
		toReturn.Text = page.Text;
		toReturn.Controls.Add(page);
		return toReturn;
	}

	private void ReloadComparisons()
	{
		while (pages.Count > 2)
		{
			pages.RemoveAt(2);
		}
		foreach (string section in file.Sections)
		{
			if (section == "Best Run" || section == "Top Scores" || section == "General")
				continue;

			pages.Add(ConfigureTab(section));
		}
		currentComparison.Controls.Clear();
		currentComparison.Controls.Add(pages[selector.Index + 2]);
	}

	private ScorePage ConfigureTab (string section)
	{
		ScorePage page = new ScorePage(file, section);
		foreach (KeyValuePair<string, string> score in file.GetSection(section))
		{

			page.Add(new NumericField (score.Key, score.Value));

		}
		return page;
	}
}
