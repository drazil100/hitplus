using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;



public class OptionsWindow : Form
{

	private FileReader config;
	private ColorFileReader colorTheme;

	private TabControl tabs = new TabControl();
	private List<OptionsTab> oTabs = new List<OptionsTab>();

	private Button save = new Button();
	private Button saveClose = new Button();

	private OptionsTab scoreTab = new OptionsScoreTab();
	private OptionsTab generalTab = new OptionsGeneralTab ();
	private OptionsTab colorTab = new OptionsColorsTab();
	private OptionsTab aboutTab = new OptionsAboutTab();

	private static int tabIndex = 0;

	private int w = 300;
	private int h = 400;

	public OptionsWindow()
	{
		Text = "Options";

		config = ScoreTracker.config;

		Controls.Add (tabs);
		Panel p = new Panel();
		p.Height = 20;
		p.Controls.Add (save);
		p.Controls.Add (saveClose);
		p.Dock = DockStyle.Bottom;
		Controls.Add(p);

		scoreTab.Configure(new FileReader(ScoreTracker.Data.File.FileName));

		tabs.TabPages.Add (scoreTab);
		tabs.TabPages.Add (generalTab);
		tabs.TabPages.Add (colorTab);
		tabs.TabPages.Add (aboutTab);

		oTabs.Add (scoreTab);
		oTabs.Add (generalTab);
		oTabs.Add (colorTab);
		oTabs.Add (aboutTab);

		save.Text = "Save All Changes";
		save.Dock = DockStyle.Left;
		save.Click += new EventHandler(Save);

		saveClose.Text = "Save and Close";
		saveClose.Dock = DockStyle.Right;
		saveClose.Click += new EventHandler(SaveClose);

		Size = new Size (w, h);
		MaximumSize = Size;
		MinimumSize = Size;

		Layout += new LayoutEventHandler((object sender, LayoutEventArgs e) => DoLayout());
		tabs.SelectedIndexChanged += delegate { CacheTabIndex(); };

		tabs.SelectedIndex = tabIndex;

		DoLayout ();
	}

	public void CacheTabIndex()
	{
		tabIndex = tabs.SelectedIndex;
	}

	public void SaveClose(object sender, EventArgs e)
	{
		Save(true);
		this.Close();
	}

	public void Save(object sender, EventArgs e)
	{
		Save();
	}
	public void Save(bool closing = false)
	{
		foreach (OptionsTab page in oTabs)
		{
			page.Save();
		}


		ScoreTracker.FileIndex = Int32.Parse(config["file_index"]);
		TrackerData data = new TrackerData(new FileReader(ScoreTracker.files[ScoreTracker.FileIndex], SortingStyle.Validate));
		ScoreTracker.Data = data;
		if (!closing)
			scoreTab.Configure(new FileReader(ScoreTracker.Data.File.FileName));
	}

	private int GetWidth()
	{
		return (
			ClientRectangle.Width
		);
	}

	private int GetHeight()
	{
		return (
			ClientRectangle.Height
		);
	}

	public void DoLayout()
	{
		tabs.Dock = DockStyle.Fill;
		foreach (OptionsTab page in oTabs)
		{
			page.Dock = DockStyle.Fill;
			page.DoLayout();
		}

		save.Width = ClientRectangle.Width/2;
		saveClose.Width = ClientRectangle.Width/2;
	}
}
