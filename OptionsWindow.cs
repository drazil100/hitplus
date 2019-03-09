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
	private ColorFileReader colorTheme;

	private TabControl tabs = new TabControl();

	private Button save = new Button();
	private Button saveClose = new Button();

	private TabPage scoreTab = new TabPage();
	private List<NumericField> scores = new List<NumericField>();

	private TabPage generalTab = new TabPage ();
	private List<OptionField> generalOptions = new List<OptionField>();

	private TabPage colorTab = new TabPage();
	private List<ColorField> colors = new List<ColorField>();

	private int w = 300;
	private int h = 400;

	public OptionsWindow()
	{
		Text = "Options";

		config = ScoreTracker.config;
		ils = ScoreTracker.individualLevels;
		colorTheme = ScoreTracker.colors;

		Controls.Add (tabs);
		Controls.Add (save);
		Controls.Add (saveClose);

		scoreTab.Text = "Edit PBs";
		tabs.TabPages.Add (scoreTab);

		generalTab.Text = "General";
		tabs.TabPages.Add (generalTab);
		
		colorTab.Text = "Colors";
		tabs.TabPages.Add (colorTab);

		save.Text = "Save All Changes";
		save.Click += new EventHandler(Save);

		saveClose.Text = "Save and Close";
		saveClose.Click += new EventHandler(SaveClose);

		Size = new Size (w, h);
		MaximumSize = Size;
		MinimumSize = Size;

		Resize += delegate { DoLayout(); };

		ConfigureScoreTab ();
		ConfigureGeneralTab ();
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

		config ["hard_route"]                = generalOptions[0].ToString();
		config ["layout"]                    = generalOptions[1].ToString();
		config ["highlight_current"]         = generalOptions[2].ToString();
		config ["start_highlighted"]         = generalOptions[3].ToString();
		config ["sums_horizontal_alignment"] = generalOptions[4].ToString();
		config ["font"]                      = generalOptions[5].GetOption();
		config ["font_size"]                 = generalOptions[6].ToString();
		//config ["vertical_scale_mode"]       = generalOptions[7].ToString();
		//Console.WriteLine (font.Text);
		config.Save ();

		foreach (ColorField color in colors)
		{
			colorTheme[LabelNameToOptionName(color.GetName())] = color.GetColor();
		}

		colorTheme.Save();
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

		DoScoreLayout ();
		DoColorLayout();

		save.Width = GetWidth()/2;
		save.Height = 20;
		save.Top = tabs.Bottom;

		saveClose.Width = save.Width;
		saveClose.Height = 20;
		saveClose.Top = save.Top;
		saveClose.Left = save.Width;
	}


	private void DoScoreLayout()
	{
		foreach (NumericField s in scores)
		{
			s.Width = scoreTab.Width;
			//s.DoLayout ();
		}


	}

	private void DoColorLayout()
	{
		for (int i = 0; i < colors.Count; i++)
		{
			if (i > 0)
			{
				colors[i].Top = colors[i-1].Top + colors[i-1].Height;
			}
			colors[i].Width = Width;
		}
	}

	private void ConfigureGeneralTab()
	{
		generalOptions.Add(new DropdownField("Route:", config["hard_route"], "Easy", "Hard"));
		generalOptions.Add(new DropdownField("Layout:", config["layout"], "Horizontal", "Vertical"));
		generalOptions.Add(new CheckField("Highlight Current:", config["highlight_current"]));
		generalOptions.Add(new CheckField("Start Highlighted:", config["start_highlighted"]));
		generalOptions.Add(new DropdownField("Horizontal Splits Alignment:", config["sums_horizontal_alignment"], "Left", "Right"));

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

		generalOptions.Add(new DropdownField("Font:", "" + ind, fonts.ToArray()));
		generalOptions.Add(new NumericField("Font Size:", config["font_size"]));
		//generalOptions.Add(new DropdownField("Vertical Scaling Mode:", config["vertical_scale_mode"], "Space", "Split"));

		for (int i = 0; i < generalOptions.Count; i++)
		{
			if (i > 0)
			{
				generalOptions[i].Top = generalOptions[i-1].Top + generalOptions[i-1].Height;
			}
			generalOptions[i].Width = Width;
			generalTab.Controls.Add(generalOptions[i]);
		}
	}

	private void ConfigureScoreTab ()
	{
		scoreTab.Controls.Clear ();
		foreach (KeyValuePair<string, string> il in ils)
		{
			if (il.Key != "Easy Route" && il.Key != "Hard Route")
			{
				NumericField newScore = new NumericField (il.Key, il.Value);
				scoreTab.Controls.Add (newScore);
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
		foreach (KeyValuePair<string, Color> pair in colorTheme)
		{
			ColorField c = new ColorField(OptionNameToLabelName(pair.Key), pair.Value);
			colors.Add(c);
			colorTab.Controls.Add(c);
		}

		DoColorLayout();
	}

	public string OptionNameToLabelName(string name)
	{
		switch (name)
		{
			case "text_color":                   name = "Text:";                   break;
			case "text_color_total":             name = "Totals Text:";            break;
			case "text_color_highlighted":       name = "Text Highlighted";        break;
			case "background_color":             name = "Background:";             break;
			case "background_color_highlighted": name = "Background Highlighted:"; break;
			case "text_color_ahead":             name = "Text Ahead:";             break;
			case "text_color_behind":            name = "Text Behind:";            break;
			case "text_color_best":              name = "Text Best:";              break;
		}

		return name;
	}

	public string LabelNameToOptionName(string name)
	{
		switch (name)
		{
			case "Text:":                   name = "text_color";                   break;
			case "Totals Text:":            name = "text_color_total";             break;
			case "Text Highlighted":        name = "text_color_highlighted";       break;
			case "Background:":             name = "background_color";             break;
			case "Background Highlighted:": name = "background_color_highlighted"; break;
			case "Text Ahead:":             name = "text_color_ahead";             break;
			case "Text Behind:":            name = "text_color_behind";            break;
			case "Text Best:":              name = "text_color_best";              break;
		}

		return name;
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

	public string GetName()
	{
		return name.Text;
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
		get 
		{ 
			if (score.Text != "") 
				return score.Text; 
			return "0";
		}
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
