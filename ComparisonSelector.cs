using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

public delegate void Action();

public enum ScoresetType
{
	All,
	TopScores,
	Comparison,
	Record
}

public class ComparisonSelector : Panel
{
	private FileReader file;
	private Button back = new Button();
	private Button next = new Button();
	private ComboBox options = new ComboBox();

	private bool reloading = false;

	private bool getFromTrackerData = false;

	//private Action n;
	//private Action b;

	private ScoresetType justComparisons = ScoresetType.All;

	public ComparisonSelector() : this(ScoreTracker.Data.File)
	{
		getFromTrackerData = true;
	}

	public ComparisonSelector(FileReader file)
	{
		this.file = file;
		Height = 20;
		back.Text = "\u2190";
		next.Text = "\u2192";
		back.Width = 40;
		next.Width = 40;
		back.Dock = DockStyle.Left;
		next.Dock = DockStyle.Right;
		options.Dock = DockStyle.Top;
		SetItems();
		Index = 0;
		this.options.DropDownStyle = ComboBoxStyle.DropDownList;
		Controls.Add(options);
		Controls.Add(back);
		Controls.Add(next);



		next.Click += delegate { NextComparison(); SetItems(); };
		back.Click += delegate { PreviousComparison(); SetItems(); };

		options.SelectedIndexChanged += delegate { DropdownChanged(); };
	}

	public FileReader File
	{
		get { 
			if (getFromTrackerData) 
			{
				return ScoreTracker.Data.File;
			}
			else
			{
				return file; 
			}
		}
	}

	public ScoresetType SetType
	{
		get { return justComparisons; }
		set { justComparisons = value; SetItems(); }
	}

	public int Count
	{
		get 
		{
			int count = 0;
			switch (SetType)
			{
				case ScoresetType.Comparison: count = GetComparisonNames().Count; break;
				case ScoresetType.Record: count = GetRecordNames().Count; break;
				default: count = GetNames().Count; break;
			}
			return count;
		}
	}

	public Action Changed
	{
		get; 
		set;
	}

	public Action Reloaded
	{
		get; 
		set;
	}
	public Action TextEdited
	{
		get; 
		set;
	}

	public int Index
	{
		get { return options.SelectedIndex; }
		set 
		{ 
			if (value >= 0 && value < Count)
				options.SelectedIndex = value; 
		}
	}

	public string Comparison
	{
		get { return ((options.Text == "Sum of Best") ? "Top Scores" : options.Text); }
	}

	public void SetItems()
	{
		List<string> items;
		switch (SetType)
		{
			case ScoresetType.Comparison:
				items = GetComparisonNames();
				break;
			case ScoresetType.Record:
				items = GetRecordNames();
				break;
			default:
				items = GetNames();
				items[1] = "Sum of Best";
				break;
		}
		int oldIndex = Index;
		options.Items.Clear();
		this.options.Items.AddRange(items.ToArray());
		if (oldIndex >= items.Count || oldIndex == -1) oldIndex = 0;
		Index = oldIndex;
	}

	public int GetIndexOfComparison(string name)
	{
		for (int i = 0; i < options.Items.Count; i++)
		{
			string item = (options.Items[i] as string);
			if (((item == "Sum of Best") ? "Top Scores" : item) == name)
			{
				return i;
			}
		}
		return -1;
	}

	public void Reload()
	{
		reloading = true;
		SetItems();
		reloading = false;
		if (Reloaded != null) Reloaded();
	}

	public void DropdownChanged()
	{
		if (!reloading && Changed != null) Changed();
	}

	public void NextComparison()
	{
		int i = Index + 1;
		if (i >= Count)
			i = 0;
		Index = i;
		return;
	}

	public void PreviousComparison()
	{
		int i = Index - 1;
		if (i < 0)
			i = Count - 1;
		Index = i;
		return;
	}
	public List<string> GetNames()
	{
		List<string> toReturn = new List<string>();
		foreach (string section in File.Sections)
		{
			if (!File.ContainsKey(section, "Scoreset Type") || (File.ContainsKey(section, "Show In Comparisons") && File[section, "Show In Comparisons"] != "yes")) 
				continue;
			toReturn.Add(section);
		}
		return toReturn;
	}

	public List<string> GetComparisonNames()
	{
		List<string> toReturn = new List<string>();
		foreach (string section in File.Sections)
		{
			if (!File.ContainsKey(section, "Scoreset Type") || File[section, "Scoreset Type"] != "Comparison")
				continue;
			toReturn.Add(section);
		}
		return toReturn;
	}

	public List<string> GetRecordNames()
	{
		List<string> toReturn = new List<string>();
		foreach (string section in File.Sections)
		{
			if (!File.ContainsKey(section, "Scoreset Type") || File[section, "Scoreset Type"] != "Record" || section == "Best Run")
				continue;
			toReturn.Add(section);
		}
		return toReturn;
	}
}
