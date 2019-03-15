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
		this.options.SelectedIndex = ScoreTracker.Data.GetComparisonIndex();
		this.options.DropDownStyle = ComboBoxStyle.DropDownList;
		Controls.Add(options);
		Controls.Add(back);
		Controls.Add(next);



		next.Click += delegate { if (Next != null) Next(); UpdateDropdown(); };
		back.Click += delegate { if (Back != null) Back(); UpdateDropdown(); };

		options.SelectedIndexChanged += delegate { DropdownChanged(); };

	}
	
	public bool JustComparisons
	{
		get { return justComparisons; }
		set { justComparisons = value; SetItems(); }
	}

	public Action Next
	{
		get; 
		set;
	}

	public Action Back
	{
		get; 
		set;
	}

	public Action Changed
	{
		get; 
		set;
	}

	public int Index
	{
		get { return this.options.SelectedIndex; }
		set { this.options.SelectedIndex = value; }
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
		options.Items.Clear();
		this.options.Items.AddRange(items.ToArray());
	}

	public void UpdateDropdown()
	{
		SetItems();
		this.options.SelectedIndex = ScoreTracker.Data.GetComparisonIndex();
	}

	public void DropdownChanged()
	{
		if (Changed != null) Changed();
	}
}
