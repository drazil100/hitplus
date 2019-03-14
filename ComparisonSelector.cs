using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class ComparisonSelector : Panel
{
	private Button back = new Button();
	private Button next = new Button();
	public ComboBox options = new ComboBox();
	public ComparisonSelector()
	{
		Height = 20;
		back.Text = "<-";
		next.Text = "->";
		back.Width = 40;
		next.Width = 40;
		back.Dock = DockStyle.Left;
		next.Dock = DockStyle.Right;
		options.Dock = DockStyle.Top;
		this.options.Items.AddRange(ScoreTracker.Data.GetComparisonNames().ToArray());
		this.options.SelectedIndex = ScoreTracker.Data.GetComparisonIndex();
		this.options.DropDownStyle = ComboBoxStyle.DropDownList;
		Controls.Add(options);
		Controls.Add(back);
		Controls.Add(next);

		next.Click += delegate { ScoreTracker.Data.NextComparison(); UpdateDropdown(); };
		back.Click += delegate { ScoreTracker.Data.PreviousComparison(); UpdateDropdown(); };

		options.SelectedIndexChanged += delegate { DropdownChanged(); };

	}

	public void UpdateDropdown()
	{
		this.options.SelectedIndex = ScoreTracker.Data.GetComparisonIndex();
		InputWindow.display.ResetContent(); 
	}

	public void DropdownChanged()
	{
		ScoreTracker.Data.SetComparisonIndex(this.options.SelectedIndex);
		InputWindow.display.ResetContent(); 
	}
}
