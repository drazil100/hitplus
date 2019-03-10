using System;
using System.Collections.Generic;

public enum PaceStatus
{
	Default,
	Ahead,
	Behind,
	Gold
}

public class ScoreEntry
{
	public string name = "";
	public int score = 0;
	
	public ScoreEntry(string name, int score)
	{
		this.name = name;
		this.score = score;
	}
	
	public override string ToString()
	{
		return String.Format("{0}: {1}", name, score);
	}
}

public class TrackerCore
{
	public int index = 0;

	public int GetScore()
	{
		int tmp = 0;
			foreach (ScorePanel sc in ScorePanel.scoresList)
			{
				tmp += sc.CurrentScore;
			}
			return tmp;
	}

	public bool IsRunning()
	{
		return (index > 0);
	}

	public bool IsFinished()
	{
		return (index == ScorePanel.scoresList.Count);
	}
	
	public void Submit(int score)
	{	
		if (IsFinished()) return;
		ScorePanel.scoresList[index].CurrentScore = score;
		ScorePanel.scoresList[index].Unhighlight();

		index++;

		if (index != ScorePanel.scoresList.Count)
			ScorePanel.scoresList[index].Highlight();
	}

	public void Undo()
	{
		if (index == 0) return;
		if (!IsFinished())
		{
			ScorePanel.scoresList[index].CurrentScore = -1;
			ScorePanel.scoresList[index].Unhighlight();
		}
		index--;
		ScorePanel.scoresList[index].CurrentScore = -1;
		if (index > 0 || ScoreTracker.config["start_highlighted"] == "1")
			ScorePanel.scoresList[index].Highlight();
	}

	public void SaveAndReset()
	{
		ScorePanel.UpdateBestScores();
		ScorePanel.SaveRun();
		index = 0;
		if (ScoreTracker.config["start_highlighted"] == "1")
			ScorePanel.scoresList[index].Highlight();
	}

	public void Reset()
	{
		index = 0;
		if (ScoreTracker.config["start_highlighted"] == "1")
			ScorePanel.scoresList[index].Highlight();
	}
}
