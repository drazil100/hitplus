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
	public static string version = "4/12/2022";

	public static string license = @"Copyright (c) 2017-2022 Austin Allman

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
		Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
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

		/*OpenFileDialog test = new OpenFileDialog();
		test.Filter = ".ini|*.ini";
		test.Title = "Test";
		test.ShowDialog();*/

		try
		{
			//Test();
			ConvertFiles();
		}
		catch (Exception e)
		{
			Console.WriteLine("Conversion Error: " + e.Message + "\n" + e.StackTrace);
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
					}
					catch (Exception)
					{
						config.RemoveKey("Files", key);
						continue;
					}
					files.Add(config["Files", key]);
					config.RemoveKey("Files", key);
				}
				int fileIndex = 0;
				foreach (string file in files)
				{
					config.AddNewItem("Files", "File_" + FormatNumber(fileIndex++), file);
				}
			}
			else
			{
				config["Files", "File_0000"] = "pb_easy.ini";
				config["Files", "File_0001"] = "pb_hard.ini";
				files.Add("pb_easy.ini");
				files.Add("pb_hard.ini");
			}

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

			int configVersion = DateToNumber(config["version"]);


			FileReader pbEasy = new FileReader("pb_easy.ini", SortingStyle.Validate);
			if (configVersion <= 20210130)
			{
				pbEasy.AddNewItem("Aliases", "Corneria", "CO");
				pbEasy.AddNewItem("Aliases", "Meteo", "ME");
				pbEasy.AddNewItem("Aliases", "Katina", "KA");
				pbEasy.AddNewItem("Aliases", "Sector X", "SX");
				pbEasy.AddNewItem("Aliases", "Macbeth", "Ma");
				pbEasy.AddNewItem("Aliases", "Area 6", "A6");
				pbEasy.AddNewItem("Aliases", "Venom 2", "VE");
			}
			if (!File.Exists("pb_easy.ini"))
			{
				pbEasy.AddNewItem("Best Run", "Corneria", "0");
				pbEasy.AddNewItem("Best Run", "Meteo",    "0");
				pbEasy.AddNewItem("Best Run", "Katina",   "0");
				pbEasy.AddNewItem("Best Run", "Sector X", "0");
				pbEasy.AddNewItem("Best Run", "Macbeth",  "0");
				pbEasy.AddNewItem("Best Run", "Area 6",   "0");
				pbEasy.AddNewItem("Best Run", "Venom 2",    "0");
				pbEasy.Save();
			}
			if (!pbEasy.ContainsKey("game")) 
			{
				pbEasy["name"] = "Easy Route";
				pbEasy["game"] = "Star Fox 64";
				//pbEasy["IL Syncing"] = "on";
			}

			TrackerData.ValidateFile(pbEasy);

			FileReader pbHard = new FileReader("pb_hard.ini", SortingStyle.Validate);
			if (configVersion <= 20210130)
			{
				pbHard.AddNewItem("Aliases", "Corneria", "CO");
				pbHard.AddNewItem("Aliases", "Sector Y", "SY");
				pbHard.AddNewItem("Aliases", "Aquas", "AQ");
				pbHard.AddNewItem("Aliases", "Zoness", "ZO");
				pbHard.AddNewItem("Aliases", "Macbeth", "Ma");
				pbHard.AddNewItem("Aliases", "Area 6", "A6");
				pbHard.AddNewItem("Aliases", "Venom 2", "VE");
			}
			if (!File.Exists("pb_hard.ini"))
			{
				pbHard.AddNewItem("Best Run", "Corneria", "0");
				pbHard.AddNewItem("Best Run", "Sector Y", "0");
				pbHard.AddNewItem("Best Run", "Aquas",    "0");
				pbHard.AddNewItem("Best Run", "Zoness",   "0");
				pbHard.AddNewItem("Best Run", "Macbeth",  "0");
				pbHard.AddNewItem("Best Run", "Area 6",   "0");
				pbHard.AddNewItem("Best Run", "Venom 2",    "0");
				pbHard.Save();
			}
			if (!pbHard.ContainsKey("game")) 
			{
				pbHard["name"] = "Hard Route";
				pbHard["game"] = "Star Fox 64";
				//pbHard["IL Syncing"] = "on";
			}

			TrackerData.ValidateFile(pbHard);
			

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
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
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

	public static void ConvertFiles()
	{
		if (File.Exists("config.txt"))
		{
			FileReader convertConfig = new FileReader(':', "config.txt", SortingStyle.Sort);
			convertConfig.KeySeparator = '=';
			convertConfig.FileName = "config.ini";
			convertConfig.Save();

			FileReader convertEasy = new FileReader(':', "pb_easy.txt");

			FileReader convertHard = new FileReader(':', "pb_hard.txt");

			FileReader convertIL = new FileReader(':', "pb_individuals.txt");

			FileReader easy = new FileReader("pb_easy.ini");
			FileReader hard = new FileReader("pb_hard.ini");

			easy.AddNewItem("name", "Easy Route");
			foreach (SectionKeyValue<string> pair in convertEasy)
			{
				string k = (pair.Key != "Venom") ? pair.Key : "Venom 2";
				easy.AddNewItem("Best Run", k, convertEasy[pair.Key]);
				string tmp = "0";
				if (convertIL.ContainsKey(pair.Key)) tmp = convertIL["General", pair.Key];
				easy.AddNewItem("Top Scores", k, tmp);
			}
			if (easy.GetSection("Best Run").Keys.Count > 0) easy.Save();

			easy.AddNewItem("name", "Hard Route");
			foreach (SectionKeyValue<string> pair in convertHard)
			{
				string k = (pair.Key != "Venom") ? pair.Key : "Venom 2";
				hard.AddNewItem("Best Run", k, convertHard[pair.Key]);
				string tmp = "0";
				if (convertIL.ContainsKey(pair.Key)) tmp = convertIL["General", pair.Key];
				hard.AddNewItem("Top Scores", k, tmp);
			}
			if (hard.GetSection("Best Run").Keys.Count > 0) hard.Save();

			if (convertIL.ContainsKey("Venom"))
			{
				convertIL["Venom 2"] = convertIL["Venom"];
				convertIL.RemoveKey("Venom");
			}
			convertIL.Export();


			File.Delete("config.txt");
			File.Delete("pb_easy.txt");
			File.Delete("pb_hard.txt");
			
			MessageBox.Show("Your files have been converted to a new format."+
			                " Going forward \"pb_individuals.txt\" will no longer be used to track your individual level PBs.\r\n\r\n"+
					"Instead level pbs are now saved in the same file as your run pb and are unique to that run with no crossover between routes.\r\n\r\n"+
					"Your \"pb_individuals.txt\" will be left intact and if you wish to continue storing your ILs in this file you can do so by adding \"generate_legacy_il_file = 1\" to \"config.ini\" while the program is closed.\r\n\r\n"+
					"Do note that enabling this option WILL NOT change where this program reads these scores from."+
					" These scores will still be output to and read from the file that stores your best run.", 
					"Your tracker files have been converted!");
		}
	}

	public static void Test()
	{
		FileReader unsort = new FileReader("unsort.ini", SortingStyle.Unsort);
		unsort.AddNewItem("add 1", "value");
		unsort.AddNewItem("Section", "add 2", "value");
		unsort["set 1"] = "value";
		unsort["Section", "set 2"] = "value";
		unsort.Save();

		FileReader sort = new FileReader("sort.ini", SortingStyle.Sort);
		sort.AddNewItem("add 1", "value");
		sort.AddNewItem("Section", "add 2", "value");
		sort["set 1"] = "value";
		sort["Section", "set 2"] = "value";
		sort.Save();

		FileReader validate = new FileReader("validate.ini", SortingStyle.Validate);
		validate.AddNewItem("add 1", "value");
		validate.AddNewItem("Section", "add 2", "value");
		validate["set 1"] = "value";
		validate["Section", "set 2"] = "value";
		validate.Save();

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
