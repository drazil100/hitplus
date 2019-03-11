using System;
using System.Collections.Generic;

public enum PaceStatus
{
	Default,
	Ahead,
	Behind,
	Gold
}

public class TrackerCore
{
	private TrackerData scores;
	public int index = 0;

	public TrackerCore(TrackerData data)
	{
		scores = data;
	}


	public int GetTotalScore()
	{
		return scores.GetScoreSet().GetScoreTotal();
	}
	public int GetOldTotal()
	{
		return scores.GetScoreSet(0).GetComparisonTotal();
	}

	public int GetSOB()
	{
		return scores.GetScoreSet(1).GetComparisonTotal();
	}

	public bool IsRunning()
	{
		return (index > 0);
	}

	public bool IsFinished()
	{
		return (index == scores.Count);
	}
	
	public void Submit(int score)
	{	
		if (IsFinished()) return;
		scores[index] = score;

		index++;
	}

	public void Undo()
	{
		if (index == 0) return;
		if (!IsFinished())
		{
			scores[index] = -1;
		}
		index--;
		scores[index] = -1;
	}

	public void SaveAndReset()
	{
		scores.UpdateBestScores();
		scores.SaveRun();
		ClearScores();
		index = 0;
	}

	public void Reset()
	{
		index = 0;
	}
	public void ClearScores ()
	{
		for (int i = 0; i < scores.Count; i++)
		{
			scores[i] = -1;
		}
	}
}
