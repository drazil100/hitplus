using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class OptionsColorsTab : OptionsTab
{
	private ColorFileReader colorTheme;
	private List<ColorField> colors = new List<ColorField>();
	
	public OptionsColorsTab()
	{
		Text = "Colors";
		colorTheme = ScoreTracker.colors;

		foreach (KeyValuePair<string, Color> pair in colorTheme.GetSection("General"))
		{
			ColorField c = new ColorField(OptionNameToLabelName(pair.Key), pair.Value);
			colors.Add(c);
			Controls.Add(c);
		}

		DoLayout();
	}
	public override void Save()
	{
		foreach (ColorField color in colors)
		{
			colorTheme[LabelNameToOptionName(color.GetName())] = color.GetColor();
		}

		colorTheme.Save();
	}

	public override void DoLayout()
	{
		for (int i = 0; i < colors.Count; i++)
		{
			if (i > 0)
			{
				colors[i].Top = colors[i-1].Top + colors[i-1].Height;
			}
			colors[i].Width = ClientRectangle.Width;
		}
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
