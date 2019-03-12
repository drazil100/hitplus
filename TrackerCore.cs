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

	public TrackerData Data
	{
		get 
		{
			return scores;
		}
		set 
		{
			scores = value;
			Reset();
		}
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

	public PaceStatus GetCurrentPace()
	{
		return scores.GetCurrentPace();
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

		scores.SetCurrent(index);
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
		scores.SetCurrent(index);
	}

	public void SaveAndReset()
	{
		scores.UpdateBestScores();
		scores.SaveRun();
		Reset();
	}

	public void Reset()
	{
		ClearScores();
		index = 0;
		scores.SetCurrent(0);
	}
	public void ClearScores ()
	{
		for (int i = 0; i < scores.Count; i++)
		{
			scores[i] = -1;
		}
	}
}
