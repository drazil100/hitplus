using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class OptionsGeneralTab : OptionsTab
{

	private FileReader config;
	private List<OptionField> generalOptions = new List<OptionField>();

	public OptionsGeneralTab()
	{
		Text = "General";

		config = ScoreTracker.config;

		generalOptions.Add(new DropdownField("Route:", config["file_index"], ScoreTracker.files.ToArray()));
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
		generalOptions.Add(new TextField("Foo:", "Bar"));
		//generalOptions.Add(new DropdownField("Vertical Scaling Mode:", config["vertical_scale_mode"], "Space", "Split"));

	}

	public override void Save()
	{
		config ["file_index"]                = generalOptions[0].ToString();
		config ["layout"]                    = generalOptions[1].ToString();
		config ["highlight_current"]         = generalOptions[2].ToString();
		config ["start_highlighted"]         = generalOptions[3].ToString();
		config ["sums_horizontal_alignment"] = generalOptions[4].ToString();
		config ["font"]                      = generalOptions[5].GetOption();
		config ["font_size"]                 = generalOptions[6].ToString();
		//config ["vertical_scale_mode"]       = generalOptions[7].ToString();
		//Console.WriteLine (font.Text);
		config.Save ();
	}

	public override void DoLayout()
	{
		for (int i = 0; i < generalOptions.Count; i++)
		{
			if (i > 0)
			{
				generalOptions[i].Top = generalOptions[i-1].Top + generalOptions[i-1].Height;
			}
			generalOptions[i].Width = ClientRectangle.Width;
			Controls.Add(generalOptions[i]);
		}
	}
}
