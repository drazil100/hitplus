using System;
using System.Collections;
using System.Collections.Generic;

public class ScoreSet : IEnumerable<ScoreEntry>
{
	private FileReader file;
	private List<ScoreEntry> scores = new List<ScoreEntry>();

	public ScoreSet(FileReader file)
	{
		this.file = file;
		foreach (KeyValuePair<string, string> pair in file)
		{
			scores.Add(new ScoreEntry(pair.Key, Int32.Parse(pair.Value)));
		}
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

	public void SaveScores()
	{
		foreach (ScoreEntry score in scores)
		{
			file[score.Name] = "" + score.Score;
		}
		file.Save();
	}
	public void SaveComparisons()
	{
		foreach (ScoreEntry score in scores)
		{
			file[score.Name] = "" + score.Comparison;
		}
		file.Save();
	}
}
