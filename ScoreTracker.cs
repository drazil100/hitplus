using System.Text;
using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Globalization;


public class ScoreTracker : Form
{
	public static string version = "3/7/2019";

	[DllImport("kernel32.dll")]
	static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	const int SW_HIDE = 0;
	const int SW_SHOW = 5;
	

	public static FileReader config;
	public static FileReader pbEasy;
	public static FileReader pbHard;
	public static FileReader individualLevels;
	public static ColorFileReader colors;

	public ScoreTracker()
	{

	}
	
	public static int DateToNumber(string dt) {
		string[] parts = dt.Split('/');
		return Int32.Parse(parts[2]) * 10000 + Int32.Parse(parts[0]) * 100 + Int32.Parse(parts[1]);
	}






	public static void Main(string[] args)
	{
		try
		{
			var handle = GetConsoleWindow();
			ShowWindow(handle, SW_HIDE);
		}
		catch (Exception)
		{

		}

		for (int i = 0; i < args.Length; i++)
		{
			/*if (args[i] == "-s")
			{
				//  If -s is found in the arguments check if the port is specified
				for (int j = 0; j < args.Length; j++)
				{
					if (args[j] == "-p" && j < args.Length - 1)
					{
						//  If -p is found set port to the next argument
						port = Convert.ToInt32(args[j+1]);
					}
				}
				//  Start server and return from Main() before client is started
				StartServer(port);
				return;
			}*/
		}

		try
		{

			config = new FileReader("config.txt", SortingStyle.Sort);
			config.AddNewItem("version",                               "");
			config.AddNewItem("hard_route",                            "0");
			config.AddNewItem("casual_mode",                           "0");
			config.AddNewItem("layout",                                "1");
			config.AddNewItem("include_route_pbs_in_individuals_file", "0");
			config.AddNewItem("sums_horizontal_alignment",             "0");

			List<string> fonts = new List<string>();
			
			foreach (FontFamily f in System.Drawing.FontFamily.Families)
			{
				fonts.Add(f.Name);
			}

			if (fonts.Contains("Segoe UI"))
			{
				config.AddNewItem("font", "Segoe UI");
			}
			else if (fonts.Contains("DejaVu Sans"))
			{
				config.AddNewItem("font", "DejaVu Sans");
			}
			else if (fonts.Contains("Arial"))
			{
				config.AddNewItem("font", "Arial");
			}
			else
			{
				config.AddNewItem("font", SystemFonts.MessageBoxFont.Name);
			}

			config.AddNewItem("font_size",                             "18");
			config.AddNewItem("highlight_current",                     "0");
			config.AddNewItem("start_highlighted",                     "1");
			config.AddNewItem("horizontal_width",                      "1296");
			config.AddNewItem("horizontal_height",                     "99");
			config.AddNewItem("vertical_width",                        "316");
			config.AddNewItem("vertical_height",                       "309");
			config["version"] = version;

			if (config.ContainsKey("debug") && config ["debug"] == "1")
			{
				try
				{
					var handle = GetConsoleWindow ();
					ShowWindow (handle, SW_SHOW);
				}
				catch (Exception)
				{

				}
			}

			if (config ["layout"] == "horizontal")
			{
				config ["layout"] = "0";
			}
			if (config ["layout"] == "vertical")
			{
				config ["layout"] = "1";
			}
			if (config ["layout"] != "0" && config ["layout"] != "1")
			{
				config ["layout"] = "0";
			}

			if (config ["sums_horizontal_alignment"] == "right")
			{
				config ["sums_horizontal_alignment"] = "0";
			}
			if (config ["sums_horizontal_alignment"] == "left")
			{
				config ["sums_horizontal_alignment"] = "1";
			}
			if (config ["sums_horizontal_alignment"] != "0" && config ["sums_horizontal_alignment"] != "1")
			{
				config ["sums_horizontal_alignment"] = "0";
			}

			config.Save();

			colors = new ColorFileReader("color_theme.txt", SortingStyle.Validate);
			colors.AddNewItem("text_color",                            "#FFFFFF");
			colors.AddNewItem("text_color_total",                      "#FFFFFF");
			colors.AddNewItem("text_color_highlighted",                "#FFFFFF");
			colors.AddNewItem("background_color",                      "#0F0F0F");
			colors.AddNewItem("background_color_highlighted",          "#3373F4");
			colors.AddNewItem("text_color_ahead",                      "#00CC36");
			colors.AddNewItem("text_color_behind",                     "#CC1200");
			colors.AddNewItem("text_color_best",                       "#D8AF1F");

			if (config.ContainsKey("text_color"))
			{
				string c = config["text_color"];
				config.RemoveKey("text_color");
				colors["text_color"] = ColorTranslator.FromHtml(c);
			}
			if (config.ContainsKey("text_color_total"))
			{
				string c = config["text_color_total"];
				config.RemoveKey("text_color_total");
				colors["text_color_total"] = ColorTranslator.FromHtml(c);
			}
			if (config.ContainsKey("text_color_highlighted"))
			{
				string c = config["text_color_highlighted"];
				config.RemoveKey("text_color_highlighted");
				colors["text_color_highlighted"] = ColorTranslator.FromHtml(c);
			}
			if (config.ContainsKey("background_color"))
			{
				string c = config["background_color"];
				config.RemoveKey("background_color");
				colors["background_color"] = ColorTranslator.FromHtml(c);
			}
			if (config.ContainsKey("background_color_highlighted"))
			{
				string c = config["background_color_highlighted"];
				config.RemoveKey("background_color_highlighted");
				colors["background_color_highlighted"] = ColorTranslator.FromHtml(c);
			}
			if (config.ContainsKey("text_color_ahead"))
			{
				string c = config["text_color_ahead"];
				config.RemoveKey("text_color_ahead");
				colors["text_color_ahead"] = ColorTranslator.FromHtml(c);
			}
			if (config.ContainsKey("text_color_behind"))
			{
				string c = config["text_color_behind"];
				config.RemoveKey("text_color_behind");
				colors["text_color_behind"] = ColorTranslator.FromHtml(c);
			}
			if (config.ContainsKey("text_color_best"))
			{
				string c = config["text_color_best"];
				config.RemoveKey("text_color_best");
				colors["text_color_best"] = ColorTranslator.FromHtml(c);
			}

			colors.Save();
			config.Save();


			pbEasy = new FileReader("pb_easy.txt", SortingStyle.Validate);
			pbEasy.AddNewItem("Corneria", "0");
			pbEasy.AddNewItem("Meteo",    "0");
			pbEasy.AddNewItem("Katina",   "0");
			pbEasy.AddNewItem("Sector X", "0");
			pbEasy.AddNewItem("Macbeth",  "0");
			pbEasy.AddNewItem("Area 6",   "0");
			pbEasy.AddNewItem("Venom 2",    "0");
			if (pbEasy.ContainsKey("Venom"))
			{
				pbEasy["Venom 2"] = pbEasy["Venom"];
				pbEasy.RemoveKey("Venom");
			}
			pbEasy.Save();

			pbHard = new FileReader("pb_hard.txt", SortingStyle.Validate);
			pbHard.AddNewItem("Corneria", "0");
			pbHard.AddNewItem("Sector Y", "0");
			pbHard.AddNewItem("Aquas",    "0");
			pbHard.AddNewItem("Zoness",   "0");
			pbHard.AddNewItem("Macbeth",  "0");
			pbHard.AddNewItem("Area 6",   "0");
			pbHard.AddNewItem("Venom 2",    "0");
			if (pbHard.ContainsKey("Venom"))
			{
				pbHard["Venom 2"] = pbHard["Venom"];
				pbHard.RemoveKey("Venom");
			}
			pbHard.Save();

			individualLevels = new FileReader("pb_individuals.txt", SortingStyle.Unsort);
			individualLevels.RemoveKey("Easy Route");
			individualLevels.RemoveKey("Hard Route");

			if (config["include_route_pbs_in_individuals_file"] == "1")
			{
				int total = 0;
				foreach (KeyValuePair<string, string> pair in pbEasy)
				{
					total += Int32.Parse(pair.Value);
				}
				if (total > 0)
				{
					individualLevels.AddNewItem("Easy Route", "" + total);
					individualLevels["Easy Route"] = "" + total;
				}
				total = 0;
				foreach (KeyValuePair<string, string> pair in pbHard)
				{
					total += Int32.Parse(pair.Value);
				}
				if (total > 0)
				{
					individualLevels.AddNewItem("Hard Route", "" + total);
					individualLevels["Hard Route"] = "" + total;
				}
			}
			
			
			if (individualLevels.ContainsKey("Venom"))
			{
				individualLevels["Venom 2"] = individualLevels["Venom"];
				individualLevels.RemoveKey("Venom");
			}

			individualLevels.Save();
		}
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);
		}

		try
		{
			Application.Run(new InputWindow());
		}
		catch (Exception e) 
		{
			Console.WriteLine(e.Message);
		}


		if (config["debug"] == "1")
			Console.Read();
	}
}

public class NumericTextBox : TextBox
{
	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		base.OnKeyPress(e);
		// Check if the pressed character was a backspace or numeric.
		if (e.KeyChar != (char)8  && !char.IsNumber(e.KeyChar) && e.KeyChar != '\n')
		{
			e.Handled = true;
		}
	}
}
