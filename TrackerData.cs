using System;
using System.IO;
using System.Collections.Generic;

public class TrackerData
{
	private FileReader file;
	private ScoreSet scores;
	private ScoreSet best;
	private ScoreSet worst;
	private List<FileReader> syncFiles = new List<FileReader>();
	private List<ScoreSet> comparisons = new List<ScoreSet>();
	private List<string> pbHistory = new List<string>();

	private int comparisonIndex = 0;

	public TrackerData(FileReader file)
	{
		this.file = file;
		ValidateFile(file);
		scores = new ScoreSet(file, "Best Run");
		best = new ScoreSet(file, "Top Scores");

		foreach (string section in file.Sections)
		{
			if (!IsComparison(file, section))
				continue;

			comparisons.Add(new ScoreSet(file, section));
		}


		GenerateWorst();
		file.Save();
		comparisonIndex = Int32.Parse(file["comparison_index"]);
	}



	public static void ValidateFile(FileReader file)
	{
		file.AddNewItem("name", "Run");
		file.AddNewItem("game", "");
		//file.AddNewItem("IL Syncing", "off");
		file.AddNewItem("comparison_index", "0");
		file.AddNewItem("pb_history_count", "0");
		/*
		foreach (string key in file.GetSection("Aliases").Keys)
		{
			file.AddNewItem("Aliases", key, "0");
		}
		*/
		
		//file.AddNewItem("Best Run", "Scoreset Type", "Record");
		Validate(file, "Best Run", "Record");

		//file.AddNewItem("Top Scores", "Scoreset Type", "Top Scores");
		Validate(file, "Top Scores", "Top Scores");

		foreach (string section in file.Sections)
		{
			if (section == "General" || file.ContainsKey(section, "Scoreset Type"))
				continue;
			file.AddNewItem(section, "Scoreset Type", "Comparison");
		}

		int i = 2;
		foreach (string section in file.Sections)
		{
			if (!file.ContainsKey(section, "Scoreset Type") || file[section, "Scoreset Type"] != "Comparison")
				continue;

			Validate(file, section, "Comparison");
			i++;
		}
		List<string> history = SortHistory(file);
		foreach (string section in history)
		{
			Validate(file, section, "PB History");
		}
		if (history.Count > 0 && ScoreTracker.config["sum_of_worst_depth"] != "0")
			i++;

		if (Int32.Parse(file["comparison_index"]) >= i)
		{
			file["comparison_index"] = "0";
		}
		file["pb_history_count"] = "" + history.Count;
		

		file.Save();
	}

	private static void Validate(FileReader file, string section, string type)
	{
		file.AddNewItem(section, "Scoreset Type", type);
		//file.AddNewItem(section, "Show In Comparisons", "yes");
		foreach(string key in file.GetSection("Best Run").Keys)
		{
			if (key == "Total Score" || key == "Scoreset Type" || key == "Show In Comparisons") 
				continue;

			file.AddNewItem(section, key, "0");
		}
	}

	public static bool IsScoreset(FileReader file, string section)
	{
		if (file.ContainsKey(section, "Scoreset Type")) return true;
		return false;
	}

	public static bool IsComparison(FileReader file, string section)
	{
		if (section == "Sum of Best") section = "Top Scores";

		if (file.ContainsKey(section, "Scoreset Type") && file[section, "Scoreset Type"] == "Comparison")
			return true;

		return false;
	}

	public static bool IsHistory(FileReader file, string section)
	{
		if (file.ContainsKey(section, "Scoreset Type") && file[section, "Scoreset Type"] == "PB History")
		{
			if (section.StartsWith("PBH "))
			{
				string[] parts = section.Split(' ');
				int i = 0;
				if (parts.Length != 2 || !int.TryParse(parts[1], out i))
					return false;

				return true;
			}
		}

		return false;
	}
	public static int ParseHistory(string section)
	{
			if (section.StartsWith("PBH "))
			{
				string[] parts = section.Split(' ');
				int i = 0;
				if (parts.Length != 2 || !int.TryParse(parts[1], out i))
					throw new System.Exception();
				return Int32.Parse(parts[1]);
			}

			throw new System.Exception();
	}

	public static List<string> SortHistory(FileReader file)
	{
		List<string> history = new List<string>();
		foreach (string section in file.Sections)
		{
			if (!file.ContainsKey(section, "Scoreset Type") || file[section, "Scoreset Type"] != "PB History")
				continue;
			if (IsHistory(file, section))
			{
				int pbr = ParseHistory(section);
				bool inserted = false;
				for (int j = 0; j < history.Count; j++)
				{
					if (pbr < ParseHistory(history[j]))
						continue;
					history.Insert(j, section);
					inserted = true;
					break;
				}
				if (!inserted)
					history.Add(section);
			}

		}
		return history;
	}

	public int this[int index]
	{
		set
		{
			SetScore(scores, index, value);
			SetScore(best, index, value);
			if (worst != null)
				SetScore(worst, index, value);
			foreach (ScoreSet scoreSet in comparisons)
			{
				SetScore(scoreSet, index, value);
			}
		}
	}

	public FileReader File
	{
		get { return file; }
	}

	public string Name
	{
		get { return file["name"]; }
	}

	public string SetName
	{
		get { return GetScoreSet().Name; }
	}

	public string Game
	{
		get { return file["game"]; }
	}

	public int Count
	{
		get { return scores.Count; }
	}
	
	public void Refresh()
	{
		scores.Refresh();
		best.Refresh();
		if (worst != null)
			worst.Refresh();
		foreach (ScoreSet scoreSet in comparisons)
		{
			scoreSet.Refresh();
		}

	}

	private void SetScore(ScoreSet scoreSet, int index, int value)
	{
		scoreSet[index].Score = value;

		if (value < 0) return;
		//Console.WriteLine("test");

		scoreSet[index].Pace = scoreSet.GetScoreTotal() - scoreSet.GetCurrentComparisonTotal();

		if (scoreSet[index].Pace < 0)
		{
			scoreSet[index].Status = PaceStatus.Behind;
		}
		else
		{
			scoreSet[index].Status = PaceStatus.Ahead;
		}

		if (value > best[index].Comparison)
		{
			scoreSet[index].Status = PaceStatus.Gold;
		}
	}

	public void SetCurrent(int index)
	{
		scores.SetCurrent(index);
		best.SetCurrent(index);
		if (worst != null)
			worst.SetCurrent(index);
		foreach (ScoreSet scoreSet in comparisons)
		{
			scoreSet.SetCurrent(index);
		}
	}

	public ScoreSet NextComparison()
	{
		int max = 2;
		if (worst != null)
			max = 3;
		comparisonIndex++;
		if (comparisonIndex >= comparisons.Count + max)
			comparisonIndex = 0;
		file["comparison_index"] = "" + comparisonIndex;
		file.Save();
		return GetScoreSet();
	}

	public ScoreSet PreviousComparison()
	{
		int max = 2;
		if (worst != null)
			max = 3;
		comparisonIndex--;
		if (comparisonIndex < 0)
			comparisonIndex = comparisons.Count + max;
		file["comparison_index"] = "" + comparisonIndex;
		file.Save();
		return GetScoreSet();
	}

	public int GetComparisonIndex()
	{
		return comparisonIndex;
	}

	public void SetComparisonIndex(int index)
	{
		int max = 2;
		if (worst != null)
			max = 3;
		if (index != comparisonIndex && index < comparisons.Count + max && index >= 0)
		{
			comparisonIndex = index;
			file["comparison_index"] = "" + comparisonIndex;
			file.Save();
		}
	}

	public List<string> GetNames()
	{
		List<string> toReturn = new List<string>();
		toReturn.Add("Best Run");
		toReturn.Add("Top Scores");
		if (worst != null)
			toReturn.Add("Sum of Worst");
		foreach (ScoreSet set in comparisons)
		{
			toReturn.Add(set.Name);
		}
		return toReturn;
	}

	public List<string> GetComparisonNames()
	{
		List<string> toReturn = new List<string>();
		foreach (ScoreSet set in comparisons)
		{
			toReturn.Add(set.Name);
		}
		return toReturn;
	}

	public ScoreSet GetScoreSet(int index)
	{
		int max = 2;
		if (worst != null)
			max = 3;
		ScoreSet toReturn;
		switch (index)
		{
			case 0: toReturn = scores; break;
			case 1: toReturn = best; break;
			case 2: if (worst != null) toReturn = worst; else toReturn = comparisons[comparisonIndex - max]; break;
			default: toReturn = comparisons[comparisonIndex - max]; break;
		}
		return toReturn;
	}

	public ScoreSet GetScoreSet()
	{
		return GetScoreSet(comparisonIndex);
	}

	public PaceStatus GetCurrentPace()
	{
		return GetCurrentPace(comparisonIndex);
	}

	public PaceStatus GetCurrentPace(int index)
	{
		return GetScoreSet(index).GetCurrentPace();
	}

	public void GenerateWorst()
	{
		worst = null;
		pbHistory.Clear();
		pbHistory = SortHistory(file);
		List<ScoreEntry> histSet = new List<ScoreEntry>();
		int pos = 0;
		int pbhCount = pbHistory.Count;
		if (pbhCount > Int32.Parse(ScoreTracker.config["sum_of_worst_depth"]))
			pbhCount = Int32.Parse(ScoreTracker.config["sum_of_worst_depth"]);
		Console.WriteLine(pbhCount);
		if (pbhCount > 0)
		{
			foreach(string key in file.GetSection("Best Run").Keys)
			{
				if (key == "Total Score" || key == "Scoreset Type") 
					continue;
				int lowest = Int32.Parse(file[pbHistory[0], key]);
				for (int i = 1; i < pbhCount; i++)
				{
					int check = Int32.Parse(file[pbHistory[i], key]);
					if (check < lowest)
						lowest = check;
				}
				ScoreEntry entry = new ScoreEntry(key, lowest);
				entry.Position = pos++;
				histSet.Add(entry);
			}
			worst = new ScoreSet(file, "Sum of Worst", histSet);
		}
	}

	public void UpdateBestScores()
	{
		bool updated = false;
		for(int i = 0; i < scores.Count; i++)
		{
			ScoreEntry entry = best[i];
			if (entry.Score > entry.Comparison)
			{
				entry.Comparison = entry.Score;
				
				foreach (FileReader file in syncFiles)
				{
					if (file.ContainsKey("Top Scores", entry.Name))
						file["Top Scores", entry.Name] = "" + entry.Comparison;
				}


				if (ScoreTracker.config.ContainsKey("generate_legacy_il_file") && ScoreTracker.config["generate_legacy_il_file"] == "1")
				{
					if (!ScoreTracker.individualLevels.ContainsKey(entry.Name) || Int32.Parse(ScoreTracker.individualLevels[entry.Name]) < entry.Score)
						ScoreTracker.individualLevels[entry.Name] = "" + entry.Score;
				}
				updated = true;
			}
		}

		if (updated)
		{
			best.SaveComparisons();
			foreach (FileReader file in syncFiles)
			{
				file.Save();
			}
			if (ScoreTracker.config.ContainsKey("generate_legacy_il_file") && ScoreTracker.config["generate_legacy_il_file"] == "1")
				ScoreTracker.individualLevels.Export();
		}
	}

	public void SaveRun()
	{
		int tot = scores.GetScoreTotal();
		bool doSave = false;

		if (tot > scores.GetComparisonTotal())
		{
			doSave = true;
		}

		for(int i = 0; i < scores.Count; i++)
		{
			if (doSave)
			{
				scores[i].Comparison = scores[i].Score;
			}
		}

		if (!doSave)
			return;
		scores.SaveScores();
		ValidateFile(file);
		GenerateWorst();
	}
}
