using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

public delegate void Action();

public class ComparisonSelector : Panel
{
	private Button back = new Button();
	private Button next = new Button();
	private ComboBox options = new ComboBox();

	private bool reloading = false;

	//private Action n;
	//private Action b;

	private bool justComparisons = false;

	public ComparisonSelector()
	{
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
				count = ScoreTracker.Data.GetComparisonNames().Count;
			}
			else
			{
				count = ScoreTracker.Data.GetNames().Count;
			}
			return count;
		}
	}
	public Action Changed
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

	public void SetItems()
	{
		List<string> items;
		if (!JustComparisons)
		{
			items = ScoreTracker.Data.GetNames();
			items[1] = "Sum of Best";
		}
		else
		{
			items = ScoreTracker.Data.GetComparisonNames();
		}
		int oldIndex = Index;
		options.Items.Clear();
		this.options.Items.AddRange(items.ToArray());
		if (oldIndex >= items.Count) oldIndex = 0;
		Index = oldIndex;
	}

	public void Reload()
	{
		reloading = true;
		SetItems();
		reloading = false;
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
}
