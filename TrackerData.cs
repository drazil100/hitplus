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
		scores = new ScoreSet(file, "Best Run");
		Validate(file, "Best Run");
		Validate(file, "Best Scores");
		file.Save();
		best = new ScoreSet(file, "Best Scores");

	}

	private void Validate(FileReader file, string section)
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
		return GetScoreSet();
	}

	public ScoreSet PreviousComparison()
	{
		comparisonIndex--;
		if (comparisonIndex < 0)
			comparisonIndex = comparisons.Count + 1;
		return GetScoreSet();
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
					if (Int32.Parse(ScoreTracker.individualLevels[entry.Name]) < entry.Score)
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
