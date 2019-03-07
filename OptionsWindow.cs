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

	private Button save = new Button();
	private Button saveClose = new Button();

	private TabPage individual_levels = new TabPage();
	private List<NumericField> scores = new List<NumericField>();

	private TabPage displayConfig = new TabPage ();
	private List<OptionField> displayOptions = new List<OptionField>();

	private TabPage colorConfig = new TabPage();
	private List<ColorField> colors = new List<ColorField>();

	private int w = 300;
	private int h = 400;

	public ColorField text_color;
	public ColorField background_color_highlighted;
	public ColorField background_color;
	public ColorField text_color_highlighted;
	public ColorField text_color_ahead;
	public ColorField text_color_behind;
	public ColorField text_color_best;
	public ColorField text_color_total;

	public OptionsWindow()
	{
		Text = "Options";
		config = ScoreTracker.config;
		ils = ScoreTracker.individualLevels;

		Controls.Add (tabs);
		Controls.Add (save);
		Controls.Add (saveClose);

		individual_levels.Text = "Edit PBs";
		tabs.TabPages.Add (individual_levels);

		save.Text = "Save All Changes";
		save.Click += new EventHandler(Save);

		saveClose.Text = "Save and Close";
		saveClose.Click += new EventHandler(SaveClose);

		displayConfig.Text = "Display";
		tabs.TabPages.Add (displayConfig);
		
		colorConfig.Text = "Colors";
		tabs.TabPages.Add (colorConfig);

		Size = new Size (w, h);
		MaximumSize = Size;
		MinimumSize = Size;

		Resize += delegate { DoLayout(); };

		ConfigureILsPage ();
		ConfigDisplayConfig ();
		ConfigColors();

		DoLayout ();
	}

	public void SaveClose(object sender, EventArgs e)
	{
		Save();
		this.Close();
	}

	public void Save(object sender, EventArgs e)
	{
		Save();
	}
	public void Save()
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

		ScoreTracker.text_color                   = text_color.GetColor();
		ScoreTracker.background_color_highlighted = background_color_highlighted.GetColor();
		ScoreTracker.background_color             = background_color.GetColor();
		ScoreTracker.text_color_highlighted       = text_color_highlighted.GetColor();
		ScoreTracker.text_color_ahead             = text_color_ahead.GetColor();
		ScoreTracker.text_color_behind            = text_color_behind.GetColor();
		ScoreTracker.text_color_best              = text_color_best.GetColor();
		ScoreTracker.text_color_total            = text_color_total.GetColor();             

		config["text_color"]                   = text_color.GetOption();
		config["background_color_highlighted"] = background_color_highlighted.GetOption();
		config["background_color"]             = background_color.GetOption();
		config["text_color_highlighted"]       = text_color_highlighted.GetOption();
		config["text_color_ahead"]             = text_color_ahead.GetOption();
		config["text_color_behind"]            = text_color_behind.GetOption();
		config["text_color_best"]              = text_color_best.GetOption();
		config["text_color_total"]             = text_color_total.GetOption();             

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
		DoColorsLayout();

		save.Width = GetWidth()/2;
		save.Height = 20;
		save.Top = tabs.Bottom;

		saveClose.Width = save.Width;
		saveClose.Height = 20;
		saveClose.Top = save.Top;
		saveClose.Left = save.Width;
	}


	private void DoILsLayout()
	{
		foreach (NumericField s in scores)
		{
			s.Width = individual_levels.Width;
			//s.DoLayout ();
		}


	}

	private void DoColorsLayout()
	{
		for (int i = 0; i < colors.Count; i++)
		{
			if (i > 0)
			{
				colors[i].Top = colors[i-1].Top + colors[i-1].Height;
			}
			colors[i].Width = Width;
			colorConfig.Controls.Add(displayOptions[i]);
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

	private void ConfigColors()
	{
		
		text_color                   = new ColorField("Text:",                   ScoreTracker.text_color);
                background_color_highlighted = new ColorField("Background Highlighted:", ScoreTracker.background_color_highlighted);
                background_color             = new ColorField("Background:",             ScoreTracker.background_color);
                text_color_highlighted       = new ColorField("Text Highlighted",        ScoreTracker.text_color_highlighted);
                text_color_ahead             = new ColorField("Text Ahead:",             ScoreTracker.text_color_ahead);
                text_color_behind            = new ColorField("Text Behind:",            ScoreTracker.text_color_behind);
                text_color_best              = new ColorField("Text Best:",              ScoreTracker.text_color_best);
                text_color_total             = new ColorField("Totals Text:",            ScoreTracker.text_color_total);              
		
		colors.Add(text_color);
		colors.Add(background_color_highlighted);
		colors.Add(background_color);
		colors.Add(text_color_highlighted);
		colors.Add(text_color_ahead);
		colors.Add(text_color_behind);
		colors.Add(text_color_best);
		colors.Add(text_color_total);

		foreach (ColorField c in colors)
		{
			colorConfig.Controls.Add(c);
		}

		DoColorsLayout();
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

public class ColorField : OptionField
{
	public Label name = new Label();
	public ColorDialog color = new ColorDialog();
	private Button button = new Button();

	public ColorField(string name, Color color)
	{
		this.name.Text = name;
		this.color.Color = color;

		button.BackColor = color;
		button.Click += new EventHandler(OnClick);

		this.Controls.Add(this.name);
		this.Controls.Add(this.button);
		Resize += delegate { DoLayout(); };
	}

	public override string GetOption()
	{
		return ColorTranslator.ToHtml(color.Color);
	}

	public Color GetColor()
	{
		return color.Color;
	}
	
	public void OnClick(object sender, EventArgs e)
	{
		if (color.ShowDialog() == DialogResult.OK)
			button.BackColor = color.Color;
	}

	public void DoLayout()
	{
		Height = 20;

		name.Height = 20;
		button.Height = 20;

		button.Width = 60;
		name.Width = Width - 60;
		button.Left = Width - 60;
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
