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
	private FileReader ils;

	private TabControl tabs = new TabControl();

	private TabPage individual_levels = new TabPage();
	private List<NumericField> scores = new List<NumericField>();
	private Button save = new Button();

	private TabPage displayConfig = new TabPage ();
	private List<OptionField> displayOptions = new List<OptionField>();

	private int w = 300;
	private int h = 400;

	public OptionsWindow()
	{
		Text = "Options";
		config = ScoreTracker.config;
		ils = ScoreTracker.individualLevels;

		Controls.Add (tabs);
		Controls.Add (save);

		individual_levels.Text = "Edit PBs";
		tabs.TabPages.Add (individual_levels);
		save.Text = "Save All Pages";
		save.Click += new EventHandler(Save);

		displayConfig.Text = "Display";
		tabs.TabPages.Add (displayConfig);

		Size = new Size (w, h);
		MaximumSize = Size;
		MinimumSize = Size;

		Resize += delegate { DoLayout(); };

		ConfigureILsPage ();
		ConfigDisplayConfig ();

		DoLayout ();
	}

	public void Save(object sender, EventArgs e)
	{
		foreach (NumericField s in scores)
		{
			ils [s.Name] = s.Number;
		}
		ils.Save ();
		config ["hard_route"]                = displayOptions[0].ToString();
		config ["layout"]                    = displayOptions[1].ToString();
		config ["highlight_current"]         = displayOptions[2].ToString();
		config ["start_highlighted"]         = displayOptions[3].ToString();
		config ["sums_horizontal_alignment"] = displayOptions[4].ToString();
		config ["font"]                      = displayOptions[5].GetOption();
		config ["font_size"]                 = displayOptions[6].ToString();
		config ["vertical_scale_mode"]       = displayOptions[7].ToString();
		//Console.WriteLine (font.Text);

		config.Save ();
	}

	private int GetWidth()
	{
		return (
			Width - (2 * SystemInformation.FrameBorderSize.Width)
		);
	}

	private int GetHeight()
	{
		return (
			Height - (2 * SystemInformation.FrameBorderSize.Height +
				SystemInformation.CaptionHeight)
		);
	}

	public void DoLayout()
	{
		tabs.Width = GetWidth ();
		tabs.Height = GetHeight () - 20;

		foreach (TabPage page in tabs.TabPages)
		{
			page.Width = tabs.Width;
			page.Height = tabs.Height - 25;
		}

		DoILsLayout ();
		DoDisplayConfigLayout ();

		save.Width = GetWidth();
		save.Height = 20;
		save.Top = tabs.Bottom;
	}

	private void DoDisplayConfigLayout()
	{
		/*
		routeL.Height = 20;
		route.Left = Width - route.Width - 21;
		routeL.Width = Width - route.Width - 21;

		layoutL.Width = routeL.Width;
		layoutL.Height = routeL.Height;
		layoutL.Top = routeL.Top + routeL.Height;
		layout.Top = layoutL.Top;
		layout.Left = route.Left;

		highlight.Height = 20;
		//highlight.Width = 20;
		highlightL.Width = routeL.Width;
		highlightL.Height = routeL.Height;
		highlightL.Top = layoutL.Top + layoutL.Height;
		highlight.Top = highlightL.Top;
		highlight.Left = Width - highlight.Width;

		startHighlighted.Height = 20;
		//startHighlighted.Width = 20;
		startHighlightedL.Width = routeL.Width;
		startHighlightedL.Height = routeL.Height;
		startHighlightedL.Top = highlightL.Top + highlightL.Height;
		startHighlighted.Top = startHighlightedL.Top;
		startHighlighted.Left = Width - startHighlighted.Width;

		sumsAlignmentL.Width = routeL.Width;
		sumsAlignmentL.Height = routeL.Height;
		sumsAlignmentL.Top = startHighlightedL.Top + startHighlightedL.Height;
		sumsAlignment.Top = sumsAlignmentL.Top;
		sumsAlignment.Left = route.Left;

		fontL.Width = routeL.Width;
		fontL.Height = routeL.Height;
		fontL.Top = sumsAlignmentL.Top + sumsAlignmentL.Height;
		font.Top = fontL.Top;
		font.Left = route.Left;

		fontSizeL.Width = Width - fontSize.Width;
		fontSizeL.Height = routeL.Height;
		fontSizeL.Top = fontL.Top + fontL.Height;
		fontSize.Top = fontSizeL.Top;
		fontSize.Left = fontSizeL.Width;

		vScaleModeL.Width = routeL.Width;
		vScaleModeL.Height = routeL.Height;
		vScaleModeL.Top = fontSizeL.Top + fontSizeL.Height;
		vScaleMode.Top = vScaleModeL.Top;
		vScaleMode.Left = route.Left;
		*/
	}

	private void DoILsLayout()
	{
		foreach (NumericField s in scores)
		{
			s.Width = individual_levels.Width;
			//s.DoLayout ();
		}


	}

	private void ConfigDisplayConfig()
	{
		displayOptions.Add(new DropdownField("Route:", config["hard_route"], "Easy", "Hard"));
		displayOptions.Add(new DropdownField("Layout:", config["layout"], "Horizontal", "Vertical"));
		displayOptions.Add(new CheckField("Highlight Current:", config["highlight_current"]));
		displayOptions.Add(new CheckField("Start Highlighted:", config["start_highlighted"]));
		displayOptions.Add(new DropdownField("Horizontal Splits Alignment:", config["sums_horizontal_alignment"], "Left", "Right"));

		List<string> fonts = new List<string>();
		int count = 0;
		int ind = 0;

		foreach (FontFamily f in System.Drawing.FontFamily.Families)
		{
			fonts.Add(f.Name);

			if (f.Name == config ["font"])
				ind = count;
			count++;
		}

		displayOptions.Add(new DropdownField("Font:", "" + ind, fonts.ToArray()));
		displayOptions.Add(new NumericField("Font Size:", config["font_size"]));
		displayOptions.Add(new DropdownField("Vertical Scaling Mode:", config["vertical_scale_mode"], "Space", "Split"));

		for (int i = 0; i < displayOptions.Count; i++)
		{
			if (i > 0)
			{
				displayOptions[i].Top = displayOptions[i-1].Top + displayOptions[i-1].Height;
			}
			displayOptions[i].Width = Width;
			displayConfig.Controls.Add(displayOptions[i]);
		}
	}

	private void ConfigureILsPage ()
	{
		individual_levels.Controls.Clear ();
		foreach (KeyValuePair<string, string> il in ils)
		{
			if (il.Key != "Easy Route" && il.Key != "Hard Route")
			{
				NumericField newScore = new NumericField (il.Key, il.Value);
				individual_levels.Controls.Add (newScore);
				if (scores.Count > 0)
				{
					newScore.Top = scores [scores.Count - 1].Top + scores [scores.Count - 1].Height;
				}
				newScore.Width = Width;
				scores.Add (newScore);
			}
		}
	}
}

public class OptionField : Panel
{
	public string SettingName { get; set; }
	public virtual string GetOption()
	{
		return "";
	}
}

public class NumericField : OptionField
{
	private Label name = new Label ();
	private NumericTextBox score = new NumericTextBox ();

	public new string Name
	{
		get { return name.Text; }
	}

	public string Number
	{
		get { return score.Text; }
	}

	public NumericField(string name, string number)
	{
		this.name.Text = name;
		this.score.Text = number;

		Controls.Add(this.name);
		Controls.Add(this.score);

		Height = 20;

		Resize += delegate { DoLayout(); };

		DoLayout();
	}

	public void DoLayout()
	{
		score.Width = 60;
		score.Height = Height;
		name.Width = Width - score.Width;
		name.Height = Height;
		score.Left = name.Width;
	}

	public override string ToString()
	{
		return Number;
	}
}

public class DropdownField : OptionField
{
	public Label name = new Label();
	public ComboBox options = new ComboBox();

	public DropdownField (string name, string current, params Object[] options)
	{
		this.name.Text = name;
		this.options.Items.AddRange (options);

		this.options.SelectedIndex = Int32.Parse(current);
		this.options.DropDownStyle = ComboBoxStyle.DropDownList;
		Controls.Add(this.name);
		Controls.Add(this.options);

		Resize += delegate { DoLayout(); };

		DoLayout();
	}
	public void DoLayout()
	{
		Height = 20;

		name.Height = 20;
		options.Left = Width - options.Width - 21;
		name.Width = Width - options.Width - 21;
	}
	public override string ToString()
	{
		return "" + options.SelectedIndex;
	}

	public override string GetOption()
	{
		return options.Text;
	}

}

public class CheckField : OptionField
{
	public Label name = new Label();
	public CheckBox option = new CheckBox(); 

	public CheckField(string name, string current)
	{
		this.name.Text = name;
		option.Checked = (current == "0") ? false : true;

		Controls.Add(this.name);
		Controls.Add(this.option);

		Resize += delegate { DoLayout(); };

		DoLayout();
	}

	public void DoLayout()
	{
		Height = 20;

		option.Height = 20;
		name.Height = 20;
		option.Left = Width - option.Width;
		name.Width = Width - option.Width - 21;
	}

	public override string ToString()
	{
		return (option.Checked) ? "1" : "0";
	}
}
