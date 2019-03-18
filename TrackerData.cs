using System;
using System.IO;
using System.Collections.Generic;

public class TrackerData
{
	private FileReader file;
	private ScoreSet scores;
	private ScoreSet best;
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
			if (section == "Best Run" || section == "Top Scores" || section == "General" || section == "Sum of Best")
				continue;

			comparisons.Add(new ScoreSet(file, section));
		}
		file.Save();
	}

	public static void ValidateFile(FileReader file)
	{
		file.AddNewItem("name", "Run");
		file.AddNewItem("comparison_index", "0");
		
		Validate(file, "Best Run");

		Validate(file, "Top Scores");

		int i = 2;
		foreach (string section in file.Sections)
		{
			if (section == "Best Run" || section == "Top Scores" || section == "General" || section == "Sum of Best")
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
		foreach(string key in file.GetSection("Best Run").Keys)
		{
			file.AddNewItem(section, key, "0");
		}
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
