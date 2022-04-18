using System;
using System.Collections;
using System.Collections.Generic;

public class ScoreSet : IEnumerable<ScoreEntry>
{
	private FileReader file;
	private string section = "";
	private List<ScoreEntry> scores = new List<ScoreEntry>();

	public ScoreSet(FileReader file, string section)
	{
		this.file = file;
		this.section = section;

		int i = 0;
		foreach (KeyValuePair<string, string> pair in file.GetSection(section))
		{
			if (pair.Key == "Scoreset Type" || pair.Key == "Total Score")
			{
				continue;
			}

			ScoreEntry entry = new ScoreEntry(pair.Key, Int32.Parse(pair.Value));
			entry.Position = i++;
			scores.Add(entry);
		}
		scores[0].IsCurrent = true;
	}

	public ScoreSet(FileReader file, string section, List<ScoreEntry> scores)
	{
		this.file = file;
		this.section = section;
		this.scores = scores;
		scores[0].IsCurrent = true;
	}

	public IEnumerator<ScoreEntry> GetEnumerator() {
		return scores.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return scores.GetEnumerator();
	}

	public ScoreEntry this[string key]
	{
		get
		{
			foreach (ScoreEntry score in scores)
			{
				if (score.Name == key)
					return score;
			}
			return null;
		}
	}

	public ScoreEntry this[int index]
	{
		get { return scores[index]; }
	}

	public int Count
	{
		get { return scores.Count; }
	}

	public string Name
	{
		get { Console.WriteLine(section); return section; }
	}

	public void Refresh()
	{
		foreach (ScoreEntry entry in scores)
		{
			entry.Score = -1;
			entry.Pace = 0;
			entry.Status = PaceStatus.Default;
			entry.IsCurrent = false;
			entry.Comparison = Int32.Parse(file[section, entry.Name]);
		}
		scores[0].IsCurrent = true;
	}

	public void SetCurrent(int index)
	{
		foreach (ScoreEntry score in scores)
		{
			score.IsCurrent = false;
		}
		if (index < Count && index >= 0) scores[index].IsCurrent = true;
	}

	public int GetScoreTotal()
	{
		int total = 0;
		for (int i = 0; i < scores.Count; i++)
		{
			if (!scores[i].IsSet)
				break;

			total += scores[i].Score;
		}
		return total;
	}

	public int GetCurrentComparisonTotal()
	{
		int total = 0;
		for (int i = 0; i < scores.Count; i++)
		{
			if (!scores[i].IsSet)
				break;

			total += scores[i].Comparison;
		}
		return total;
	}

	public int GetComparisonTotal()
	{
		int total = 0;
		for (int i = 0; i < scores.Count; i++)
		{
			total += scores[i].Comparison;
		}
		return total;
	}

	public PaceStatus GetCurrentPace()
	{
		PaceStatus stat = PaceStatus.Default;

		int temp = 0;
		for (int i = 0; i < scores.Count && scores[i].IsSet; i++)
		{
			temp = scores[i].Pace;
			//Console.WriteLine(i);
		}
		if (temp < 0)
		{
			stat = PaceStatus.Behind;
		}
		else
		{
			stat = PaceStatus.Ahead;
		}

		return stat;
	}

	public void SaveScores()
	{
		string pbh = "PBH " + GetScoreTotal();
		file.AddNewItem(pbh, "Scoreset Type", "PB History");
		foreach (ScoreEntry score in scores)
		{
			file[section, score.Name] = "" + score.Score;
			file.AddNewItem(pbh, score.Name, "" + score.Score);
		}
		file.Save();
	}
	public void SaveComparisons()
	{
		foreach (ScoreEntry score in scores)
		{
			file[section, score.Name] = "" + score.Comparison;
		}
		file.Save();
	}
}
