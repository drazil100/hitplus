using System;
using System.Collections.Generic;

public class TrackerCore
{
	public int index = 0;

	public int GetScore()
	{
		int tmp = 0;
			foreach (Score sc in Score.scoresList)
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
		return (index == Score.scoresList.Count);
	}
	
	public void Submit(int score)
	{	
		if (IsFinished()) return;
		Score.scoresList[index].CurrentScore = score;
		Score.scoresList[index].Unhighlight();

		index++;

		if (index != Score.scoresList.Count)
			Score.scoresList[index].Highlight();
	}

	public void Undo()
	{
		if (index == 0) return;
		if (!IsFinished())
		{
			Score.scoresList[index].CurrentScore = -1;
			Score.scoresList[index].Unhighlight();
		}
		index--;
		Score.scoresList[index].CurrentScore = -1;
		if (index > 0 || ScoreTracker.config["start_highlighted"] == "1")
			Score.scoresList[index].Highlight();
	}

	public void SaveAndReset()
	{
		Score.UpdateBestScores();
		Score.SaveRun();
		index = 0;
		if (ScoreTracker.config["start_highlighted"] == "1")
			Score.scoresList[index].Highlight();
	}

	public void Reset()
	{
		index = 0;
		if (ScoreTracker.config["start_highlighted"] == "1")
			Score.scoresList[index].Highlight();
	}
}
