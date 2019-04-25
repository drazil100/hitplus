using System;
using System.IO;
using System.Collections.Generic;

public class TrackerData
{
	private FileReader file;
	private ScoreSet scores;
	private ScoreSet best;
	private List<FileReader> syncFiles = new List<FileReader>();
	private List<ScoreSet> comparisons = new List<ScoreSet>();

	private int comparisonIndex = 0;

	public TrackerData(FileReader file)
	{
		this.file = file;
		ValidateFile(file);
		comparisonIndex = Int32.Parse(file["comparison_index"]);
		scores = new ScoreSet(file, "Best Run");
		best = new ScoreSet(file, "Top Scores");
		foreach (string section in file.Sections)
		{
			if (!IsScoreset(file, section))
				continue;

			comparisons.Add(new ScoreSet(file, section));
		}

		if (file.ContainsKey("File Sync", "File_000"))
		{
			foreach (string key in file.GetSection("File Sync").Keys)
			{
				FileReader syncFile = new FileReader(file["File Sync", key], SortingStyle.Unsort);
				bool doAdd = false;
				foreach (ScoreEntry score in best)
				{
					if (syncFile.ContainsKey("Top Scores", score.Name))
					{
						doAdd = true;
						if (score.Comparison > Int32.Parse(syncFile["Top Scores", score.Name]))
							syncFile["Top Scores", score.Name] = "" + score.Comparison;
						if (score.Comparison < Int32.Parse(syncFile["Top Scores", score.Name]))
							score.Comparison = Int32.Parse(syncFile["Top Scores", score.Name]);
					}
				}
				if (doAdd)
				{
					syncFile.Save();
					syncFiles.Add(syncFile);
					best.SaveComparisons();
				}
			}
		}

		file.Save();
	}

	public static string FormatNumber(int i)
	{
		if (i < 10) return "00" + i;
		if (i < 100) return "0" + i;
		return "" + i;
	}

	public static void ValidateFile(FileReader file)
	{
		file.AddNewItem("name", "Run");
		file.AddNewItem("comparison_index", "0");
		if (file.ContainsKey("File Sync", "File_000"))
		{
			int fileIndex = 0;
			foreach (string key in file.GetSection("File Sync").Keys)
			{
				if (!System.IO.File.Exists(file["File Sync", key]))
				{
					continue;
				}
				file.AddNewItem("File Sync", "File_" + FormatNumber(fileIndex++), "");
			}
		}
		
		file.AddNewItem("Best Run", "Scoreset Type", "PB");
		Validate(file, "Best Run");

		file.AddNewItem("Top Scores", "Scoreset Type", "ILs");
		Validate(file, "Top Scores");

		foreach (string section in file.Sections)
		{
			if (section == "General" || 
			    section == "File Sync" || 
			    section == "Sum of Best" || 
			    section == "Best Run" || 
			    section == "Top Scores")
				continue;
			file.AddNewItem(section, "Scoreset Type", "Comparison");
		}

		int i = 2;
		foreach (string section in file.Sections)
		{
			if (section == "Sum of Best" || !file.ContainsKey(section, "Scoreset Type") || file[section, "Scoreset Type"] != "Comparison")
				continue;

			Validate(file, section);
			i++;
		}

		if (Int32.Parse(file["comparison_index"]) >= i)
		{
			file["comparison_index"] = "0";
		}

		file.Save();
	}

	private static void Validate(FileReader file, string section)
	{
		file.AddNewItem(section, "Scoreset Type", "Comparison");
		file.AddNewItem(section, "Show In Comparisons", "yes");
		int total = 0;
		foreach(string key in file.GetSection("Best Run").Keys)
		{
			if (key == "Total Score" || key == "Scoreset Type" || key == "Show In Comparisons") 
				continue;

			file.AddNewItem(section, key, "0");
			total += Int32.Parse(file[section, key]);
		}
		file.AddNewItem(section, "Total Score", "" + total);
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

	public int this[int index]
	{
		set
		{
			SetScore(scores, index, value);
			SetScore(best, index, value);
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

	public int Count
	{
		get { return scores.Count; }
	}
	
	public void Refresh()
	{
		scores.Refresh();
		best.Refresh();
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
		foreach (ScoreSet scoreSet in comparisons)
		{
			scoreSet.SetCurrent(index);
		}
	}

	public ScoreSet NextComparison()
	{
		comparisonIndex++;
		if (comparisonIndex >= comparisons.Count + 2)
			comparisonIndex = 0;
		file["comparison_index"] = "" + comparisonIndex;
		file.Save();
		return GetScoreSet();
	}

	public ScoreSet PreviousComparison()
	{
		comparisonIndex--;
		if (comparisonIndex < 0)
			comparisonIndex = comparisons.Count + 1;
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
		if (index != comparisonIndex && index < comparisons.Count + 2 && index >= 0)
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
		ScoreSet toReturn;
		switch (index)
		{
			case 0: toReturn = scores; break;
			case 1: toReturn = best; break;
			default: toReturn = comparisons[comparisonIndex - 2]; break;
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
			doSave = true;

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
	}
}
