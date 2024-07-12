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
using System.Diagnostics;


public class ScoreTracker : Form
{
	public static string version = "7/4/2024";

	public static string license = @"Copyright (c) 2017-2024 Austin Allman

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";

	[DllImport("kernel32.dll")]
	static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	const int SW_HIDE = 0;
	const int SW_SHOW = 5;

	private static TrackerCore tracker;
	private static TrackerData data;

	public static TrackerCore Tracker
	{
		get { return tracker; }
	}

	public static TrackerData Data
	{
		get { return tracker.Data; }
		set { tracker.Data = value; }
	}

	private static int fileIndex = 0;
	public static int FileIndex
	{
		get { return fileIndex; }
		set 
		{
			fileIndex = value;
			if (fileIndex >= files.Count)
			{
				fileIndex = 0;
			}
			if (fileIndex < 0)
			{
				fileIndex = files.Count - 1;
			}
		}

	}

	public static FileReader config;
	public static List<string> files;
	public static FileReader individualLevels;
	public static ColorFileReader colors;

	public ScoreTracker()
	{

	}

	public static int DateToNumber(string dt) {
		string[] parts = dt.Split('/');
		try
		{
			if (parts.Length != 3) throw new System.Exception();
			int m = Int32.Parse(parts[0]);
			int d = Int32.Parse(parts[1]);
			int y = Int32.Parse(parts[2]);

			if (m < 1 || m > 12) throw new System.Exception();
			if (d < 1 || d > 31) throw new System.Exception();
			if (y < 1) throw new System.Exception();

		}
		catch (Exception)
		{
			return -1;
		}
		return Int32.Parse(parts[2]) * 10000 + Int32.Parse(parts[0]) * 100 + Int32.Parse(parts[1]);
	}








	public static void Main(string[] args)
	{
		Console.WriteLine("Running in: " + Directory.GetCurrentDirectory());
		try 
		{
			Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
		} 
		catch (Exception) {}

		try
		{
			var handle = GetConsoleWindow();
			ShowWindow(handle, SW_HIDE);
		}
		catch (Exception)
		{

		}

		//try
		{

			config = new FileReader("config.ini", SortingStyle.Sort);
			config.AddNewItem("version",                               "");
			config.AddNewItem("file_index",                            "0");
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
			config.AddNewItem("sum_of_worst_depth",                "3");

			if (config.ContainsKey("debug") && config["debug"] == "1")
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

			colors = new ColorFileReader("color_theme.ini", SortingStyle.Validate);
			colors.AddNewItem("text_color",                            "#FFFFFF");
			colors.AddNewItem("text_color_total",                      "#FFFFFF");
			colors.AddNewItem("text_color_highlighted",                "#FFFFFF");
			colors.AddNewItem("background_color",                      "#0F0F0F");
			colors.AddNewItem("background_color_highlighted",          "#3373F4");
			colors.AddNewItem("text_color_ahead",                      "#00CC36");
			colors.AddNewItem("text_color_behind",                     "#CC1200");
			colors.AddNewItem("text_color_best",                       "#D8AF1F");

			colors.Save();


			string examplePath = Path.Combine("records", "Example Game", "Category Name.ini");

			int configVersion = DateToNumber(config["version"]);

			files = new List<string>();
			if (config.ContainsSection("Files"))
			{
				foreach (string key in config.GetSection("Files").Keys)
				{
					if (!System.IO.File.Exists(config["Files", key]))
					{
						config.RemoveKey("Files", key);
						continue;
					}
					try
					{
						FileReader tmp = new FileReader(config["Files", key]);

						if (!tmp.ContainsSection("Best Run"))
						{
							config.RemoveKey("Files", key);
							continue;
						}
						else
						{
							if (configVersion < 20220418)
							{
								int total = 0;
								foreach(string key2 in tmp.GetSection("Best Run").Keys)
								{
									if (key2 == "Total Score" || key2 == "Scoreset Type" || key2 == "Show In Comparisons") 
										continue;
									total += Int32.Parse(tmp["Best Run", key2]);

								}
								if (total == 0) continue;
								tmp.AddNewItem("PBH " + total, "Scoreset Type", "PB History");
								foreach(string key2 in tmp.GetSection("Best Run").Keys)
								{
									if (key2 == "Total Score" || key2 == "Scoreset Type" || key2 == "Show In Comparisons") 
										continue;

									tmp.AddNewItem("PBH " + total, key2, tmp["Best Run", key2]);
								}
								tmp.Save();
							}
							string game = "";
							if (tmp.ContainsKey("game") && tmp["game"] != "")
							{
								game = tmp["game"];
							}
							Directory.CreateDirectory(Path.Combine("records", game));
							Console.WriteLine("blah");
							string newLocation = Path.Combine("records", game, Path.GetFileName(config["Files", key]));
							File.Move(config["Files", key], newLocation);
							continue;
						}
					}
					catch (Exception)
					{
						config.RemoveKey("Files", key);
						continue;
					}
					config.RemoveKey("Files", key);
				}
			}
			else
			{
				/* files.Add(ePath); */
				/* files.Add(hPath); */
			}
			CrawlRecords("records");

			config.AddNewItem("System", "horizontal_width",                      "1296");
			config.AddNewItem("System", "horizontal_height",                     "99");
			config.AddNewItem("System", "vertical_width",                        "316");
			config.AddNewItem("System", "vertical_height",                       "309");

			if (config.ContainsKey("horizontal_width"))
			{
				string t = config["horizontal_width"];
				config.RemoveKey("horizontal_width");
				config["System", "horizontal_width"] = t;
			}
			if (config.ContainsKey("horizontal_height"))
			{
				string t = config["horizontal_height"];
				config.RemoveKey("horizontal_height");
				config["System", "horizontal_height"] = t;
			}
			if (config.ContainsKey("vertical_width"))
			{
				string t = config["vertical_width"];
				config.RemoveKey("vertical_width");
				config["System", "vertical_width"] = t;
			}
			if (config.ContainsKey("vertical_height"))
			{
				string t = config["vertical_height"];
				config.RemoveKey("vertical_height");
				config["System", "vertical_height"] = t;
			}
			if (config.ContainsKey("input_x"))
			{
				string t = config["input_x"];
				config.RemoveKey("input_x");
				config["System", "input_x"] = t;
			}
			if (config.ContainsKey("input_y"))
			{
				string t = config["input_y"];
				config.RemoveKey("input_y");
				config["System", "input_y"] = t;
			}
			if (config.ContainsKey("tracker_x"))
			{
				string t = config["tracker_x"];
				config.RemoveKey("tracker_x");
				config["System", "tracker_x"] = t;
			}
			if (config.ContainsKey("tracker_y"))
			{
				string t = config["tracker_y"];
				config.RemoveKey("tracker_y");
				config["System", "tracker_y"] = t;
			}

			config.Save();

			Directory.CreateDirectory(Path.Combine("records", "Example Game"));

			FileReader pbEasy = new FileReader(examplePath, SortingStyle.Validate);
			if (!File.Exists(examplePath))
			{
				pbEasy.AddNewItem("Best Run", "First",     "0");
				pbEasy.AddNewItem("Best Run", "Second",    "0");
				pbEasy.AddNewItem("Best Run", "Third",     "0");
				pbEasy.AddNewItem("Best Run", "Fourth",    "0");
				pbEasy.AddNewItem("Best Run", "Fifth",     "0");
				pbEasy.AddNewItem("Best Run", "Sixth",     "0");
				pbEasy.AddNewItem("Best Run", "Seventh",   "0");
				pbEasy.AddNewItem("Aliases", "First",     "1st");
				pbEasy.AddNewItem("Aliases", "Second",    "2nd");
				pbEasy.AddNewItem("Aliases", "Third",     "3rd");
				pbEasy.AddNewItem("Aliases", "Fourth",    "4th");
				pbEasy.AddNewItem("Aliases", "Fifth",     "5th");
				pbEasy.AddNewItem("Aliases", "Sixth",     "6th");
				pbEasy.AddNewItem("Aliases", "Seventh",   "7th");
				pbEasy.Save();
				files.Add(examplePath);
			}
			if (!pbEasy.ContainsKey("game")) 
			{
				pbEasy["name"] = "Category Name";
				pbEasy["game"] = "Example Game";
				//pbEasy["IL Syncing"] = "on";
			}

			TrackerData.ValidateFile(pbEasy);

			individualLevels = new FileReader(':', "pb_individuals.txt", SortingStyle.Unsort);

			fileIndex = Int32.Parse(config["file_index"]);
			if (fileIndex >= files.Count || fileIndex < 0)
			{
				fileIndex = 0;
				config["file_index"] = "0";
				config.Save();
			}

			data = new TrackerData(new FileReader(files[FileIndex], SortingStyle.Validate));
			tracker = new TrackerCore(data);
		}
    /*
		catch (Exception e)
		{
			Console.WriteLine("Startup Error: " + e.Message);
		}
    */


		config["version"] = version;
		config.Save();
		try
		{
			Application.Run(new InputWindow());
		}
		catch (Exception e)
		{
			LogError(e);
		}


		if (config.ContainsKey("debug") && config["debug"] == "1")
			Console.Read();
	}

	public static string FormatNumber(int i)
	{
		if (i < 10) return "000" + i;
		if (i < 100) return "00" + i;
		if (i < 1000) return "0" + i;
		return "" + i;
	}

	private static bool CrawlRecords(string path)
	{
		try
		{
			string[] directories = Directory.GetDirectories(path);
			string[] fileList = Directory.GetFiles(path);

			foreach(string f in fileList)
			{
				FileReader file = new FileReader(f, SortingStyle.Validate);
				if (TrackerData.ValidateFile(file, false))
				{
					files.Add(f);
				}
			}
			foreach(string d in directories)
			{
				if (!CrawlRecords(d))
				{
					return false;
				}
			}
		}
		catch (Exception e)
		{
			LogError(e);
			return false;
		}

		return true;
	}

	public static void LogError(Exception ex)
	{
		var st = new StackTrace(ex, true);
		var frame = st.GetFrame(0);
		var line = frame.GetFileLineNumber();
		var method = frame.GetMethod();
		var fileName = frame.GetFileName();

		Console.WriteLine($"Exception: {ex.Message}");
		Console.WriteLine($"Method: {method}");
		Console.WriteLine($"File: {fileName}");
		Console.WriteLine($"Line: {line}");
	}
}

public class NumericTextBox : TextBox
{
	public delegate void OnChanged();
	public OnChanged Changed
	{
		get;
		set;
	}

	public NumericTextBox() : base()
	{
		TextChanged += delegate { if (Changed != null) Changed(); };
	}

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
