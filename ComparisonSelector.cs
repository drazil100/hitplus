using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

public delegate void Action();

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

	private bool justComparisons = false;

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

	public bool JustComparisons
	{
		get { return justComparisons; }
		set { justComparisons = value; SetItems(); }
	}

	public int Count
	{
		get 
		{
			int count = 0;
			if (JustComparisons)
			{
				count = GetComparisonNames().Count;
			}
			else
			{
				count = GetNames().Count;
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
		if (!JustComparisons)
		{
			items = GetNames();
			items[1] = "Sum of Best";
		}
		else
		{
			items = GetComparisonNames();
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
			if (section == "General" || section == "Sum of Best") continue;
			toReturn.Add(section);
		}
		return toReturn;
	}

	public List<string> GetComparisonNames()
	{
		List<string> toReturn = new List<string>();
		foreach (string section in File.Sections)
		{
			if (section == "Best Run" || section == "Top Scores" || section == "General" || section == "Sum of Best")
				continue;
			toReturn.Add(section);
		}
		return toReturn;
	}
}
