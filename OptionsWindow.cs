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
	private List<ScoreField> scores = new List<ScoreField>();
	private Button save = new Button();

	private TabPage displayConfig = new TabPage ();
	private Label routeL = new Label ();
	private ComboBox route = new ComboBox ();
	private Label layoutL = new Label ();
	private ComboBox layout = new ComboBox ();
	private Label highlightL = new Label ();
	private CheckBox highlight = new CheckBox();
	private Label startHighlightedL = new Label ();
	private CheckBox startHighlighted = new CheckBox();
	private Label sumsAlignmentL = new Label ();
	private ComboBox sumsAlignment = new ComboBox ();
	private Label fontL = new Label();
	private ComboBox font = new ComboBox();
	private Label fontSizeL = new Label();
	private NumericTextBox fontSize = new NumericTextBox();
	private Label vScaleModeL = new Label();
	private ComboBox vScaleMode = new ComboBox();

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
		displayConfig.Controls.Add (routeL);
		displayConfig.Controls.Add (route);
		displayConfig.Controls.Add (layoutL);
		displayConfig.Controls.Add (layout);
		displayConfig.Controls.Add (highlightL);
		displayConfig.Controls.Add (highlight);
		displayConfig.Controls.Add (startHighlightedL);
		displayConfig.Controls.Add (startHighlighted);
		displayConfig.Controls.Add (sumsAlignmentL);
		displayConfig.Controls.Add (sumsAlignment);
		displayConfig.Controls.Add (fontL);
		displayConfig.Controls.Add (font);
		displayConfig.Controls.Add (fontSizeL);
		displayConfig.Controls.Add (fontSize);
		displayConfig.Controls.Add (vScaleModeL);
		displayConfig.Controls.Add (vScaleMode);

		Size = new Size (w, h);
		MaximumSize = Size;
		MinimumSize = Size;

		Resize += delegate { DoLayout(); };

		ConfigueILsPage ();
		ConfigDisplayConfig ();

		DoLayout ();
	}

	public void Save(object sender, EventArgs e)
	{
		foreach (ScoreField s in scores)
		{
			ils [s.Level] = s.Score;
		}
		ils.Save ();
		ScoreTracker.config ["hard_route"] = "" + route.SelectedIndex;
		ScoreTracker.config ["layout"] = "" + layout.SelectedIndex;
		ScoreTracker.config ["sums_horizontal_alignment"] = "" + sumsAlignment.SelectedIndex;
		ScoreTracker.config ["highlight_current"] = (highlight.Checked) ? "1" : "0";
		ScoreTracker.config ["start_highlighted"] = (startHighlighted.Checked) ? "1" : "0";
		ScoreTracker.config ["font"] = font.Text;
		ScoreTracker.config ["font_size"] = fontSize.Text;
		ScoreTracker.config ["vertical_scale_mode"] = "" + vScaleMode.SelectedIndex;
		//Console.WriteLine (font.Text);

		ScoreTracker.config.Save ();
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
	}

	private void DoILsLayout()
	{
		foreach (ScoreField s in scores)
		{
			s.Width = individual_levels.Width;
			//s.DoLayout ();
		}


	}

	private void ConfigDisplayConfig()
	{
		routeL.Text = "Route:";

		route.Items.AddRange (new Object[] { "Easy", "Hard" });
		route.SelectedIndex = Int32.Parse(ScoreTracker.config["hard_route"]);
		route.DropDownStyle = ComboBoxStyle.DropDownList;

		layoutL.Text = "Layout:";

		layout.Items.AddRange (new Object[] { "Horizontal", "Vertical" });
		layout.SelectedIndex = Int32.Parse(ScoreTracker.config["layout"]);
		layout.DropDownStyle = ComboBoxStyle.DropDownList;

		highlightL.Text = "Highlight Current:";
		//highlight.Appearance = Appearance.Button;
		highlight.Checked = (ScoreTracker.config ["highlight_current"] == "0") ? false : true;

		startHighlightedL.Text = "Start Highlighted:";
		//startHighlighted.Appearance = Appearance.Button;
		startHighlighted.Checked = (ScoreTracker.config ["start_highlighted"] == "0") ? false : true;

		sumsAlignmentL.Text = "Horizontal Splits Alignment:";

		sumsAlignment.Items.AddRange (new Object[] { "Left", "Right" });
		sumsAlignment.SelectedIndex = Int32.Parse(ScoreTracker.config["sums_horizontal_alignment"]);
		sumsAlignment.DropDownStyle = ComboBoxStyle.DropDownList;

		List<string> fonts = new List<string>();
		int count = 0;
		int ind = 0;

		foreach (FontFamily f in System.Drawing.FontFamily.Families)
		{
			fonts.Add(f.Name);

			if (f.Name == ScoreTracker.config ["font"])
				ind = count;
			count++;
		}

		fontL.Text = "Font:";

		font.Items.AddRange (fonts.ToArray());
		font.SelectedIndex = ind;
		font.DropDownStyle = ComboBoxStyle.DropDownList;

		fontSizeL.Text = "Font Size:";
		fontSize.Text = ScoreTracker.config ["font_size"];

		vScaleModeL.Text = "Vertical Scaling Mode";

		vScaleMode.Items.AddRange (new Object[] { "Space", "Split" });
		vScaleMode.SelectedIndex = Int32.Parse(ScoreTracker.config["vertical_scale_mode"]);
		vScaleMode.DropDownStyle = ComboBoxStyle.DropDownList;
	}

	private void ConfigueILsPage ()
	{
		individual_levels.Controls.Clear ();
		foreach (KeyValuePair<string, string> il in ils)
		{
			if (il.Key != "Easy Route" && il.Key != "Hard Route")
			{
				ScoreField newScore = new ScoreField (il);
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

public class ScoreField : Panel
{
	private Label name = new Label ();
	private NumericTextBox score = new NumericTextBox ();

	public string Level
	{
		get { return name.Text; }
	}

	public string Score
	{
		get { return score.Text; }
	}

	public ScoreField(KeyValuePair<string, string> il)
	{
		name.Text = il.Key;
		score.Text = il.Value;

		Controls.Add(name);
		Controls.Add(score);

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
}

