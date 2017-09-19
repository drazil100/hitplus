using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class Entry
{
	public string name = "";
	public int score = 0;
	
	public Entry(string name, int score)
	{
		this.name = name;
		this.score = score;
	}
	
	public override string ToString()
	{
		return String.Format("{0}: {1}", name, score);
	}
}

public class Score : Panel
{
	private static Dictionary<string, Score> scores = new Dictionary<string, Score>();
	public static List<Score> scoresList = new List<Score>();
	public static List<Entry> topData = new List<Entry>();
	
	private Label nameLabel = new Label();
	private Label scoreLabel = new Label();
	
	
	private Color text_default = ScoreTracker.text_color;
	private Color bgColor = ScoreTracker.background_color;
	private Color bgColorHighlighted = ScoreTracker.background_color_highlighted;
	private Color current = ScoreTracker.text_color_highlighted;
	private Color ahead = ScoreTracker.text_color_ahead;
	private Color behind = ScoreTracker.text_color_behind;
	private Color bestColor = ScoreTracker.text_color_best;

	private int score = 0;
	public int pbScore = 0;
	public int best = 0;
	public string name = "";
	private int index = 0;
	public int arrayIndex = 0;
	
	private Score()
	{
		
	}
	
	public Score(string name, int score)
	{
		if (scores.ContainsKey(name))
			return;
		scores.Add(name, this);
		index = scoresList.Count;
		scoresList.Add(this);
		nameLabel.ForeColor = text_default;
		scoreLabel.ForeColor = text_default;
		nameLabel.BackColor = bgColor;
		scoreLabel.BackColor = bgColor;
		if (index == 0)
			Highlight();
		this.name = name;
		this.pbScore = score;
		scoreLabel.Text = String.Format("{0}", pbScore);
		nameLabel.Text = String.Format("{0}:", name);
		
		Controls.Add(nameLabel);
		Controls.Add(scoreLabel);
		
		Resize += delegate { DoLayout(); };
		
		DoLayout();
		
	}
	
	~Score()
	{
		scores.Remove(this.name);
		scoresList.Remove(this);
	}
	
	public override string ToString()
	{
		return String.Format("{0}:{1}", name, CurrentScore);
	}
	
	public int CurrentScore {
		get {return score;}
		set 
		{
			score = value;
			int runScore = 0;
			int oldScore = 0;
			for (int i = 0; (i < scoresList.Count && i <= index); i++)
			{
				oldScore += scoresList[i].pbScore;
				runScore += scoresList[i].CurrentScore;
			}
			if (runScore >= oldScore)
			{
				scoreLabel.Text = String.Format("{0}\n+{1}", score, (runScore - oldScore));
				scoreLabel.ForeColor = ahead;
			}
			else
			{
				scoreLabel.Text = String.Format("{0}\n-{1}", score, (oldScore - runScore));
				scoreLabel.ForeColor = behind;
			}
			if (score > best)
			{
				scoreLabel.ForeColor = bestColor;
			}
			
			if (score == 0)
			{
				scoreLabel.ForeColor = text_default;
				scoreLabel.Text = ""+pbScore;//+"\n"+best;
				
			}
		}
	}
	
	public static void UpdateBestScores()
	{
		bool updated = false;
		for(int i = 0; i < scoresList.Count; i++)
		{
			if (scoresList[i].CurrentScore > scoresList[i].best)
			{
				scoresList[i].best = scoresList[i].CurrentScore;
				updated = true;
				foreach(Entry entry in topData)
				{
					if (entry.name == scoresList[i].name)
					{
						entry.score = scoresList[i].CurrentScore;
					}
				}
			}
		}
		
		if (updated)
		{
			//string temp = topData[0].ToString();
		
			for(int i = 0; i < topData.Count; i++)
			{
				ScoreTracker.individualLevels[topData[i].name] = "" + topData[i].score;
			}
			
			try
			{
				ScoreTracker.individualLevels.Save();
			}
			catch (Exception e2)
			{
				Console.WriteLine("Error: " + e2.Message);
			}
		}
	}
	
	public static void SaveRun()
	{
		
		//string temp = "";
		int sob = 0;
		int tot = GetTotal();
		bool doSave = false;
		
		if (GetTotal() > GetOldScore())
			doSave = true;
		
		FileReader file = ScoreTracker.pbEasy;
				
		if (ScoreTracker.config["hard_route"] == "1")
			file = ScoreTracker.pbHard;
		
		//temp = scoresList[0].ToString();
		
		for(int i = 0; i < scoresList.Count; i++)
		{
			//if (i > 0)
				//temp += "\n" + scoresList[i].ToString();
			
			sob += scoresList[i].best;
			if (doSave)
			{
				scoresList[i].pbScore = scoresList[i].CurrentScore;
				file[scoresList[i].name] = "" + scoresList[i].CurrentScore;
			}
			scoresList[i].CurrentScore = 0;
		}
		
		
		ScoreTracker.sobScore.Text = "SoB: " + sob;
		//string config = "";
		
		if (!doSave)
			return;
		file.Save();
		
		ScoreTracker.topScore.Text = "Top: " + tot;
	}
	
	public static Score GetScore(string name)
	{
		if (scores.ContainsKey(name))
			return scores[name];
		return null;
	}
	
	public static void SetBest(string name, int score, int arrayIndex = -1)
	{
		if (scores.ContainsKey(name))
		{
			scores[name].best = score;
			//scores[name].scoreLabel.Text = scores[name].scoreLabel.Text + "\n" + score;
		
			if (arrayIndex > -1)
				scores[name].arrayIndex = arrayIndex;
		}
		topData.Add(new Entry(name, score));
	}
	
	public static int GetTotal()
	{
		int t = 0;
		foreach (Score s in scoresList)
		{
			t += s.CurrentScore;
		}
		return t;
	}
	
	public static int GetOldScore()
	{
		int t = 0;
		foreach (Score s in scoresList)
		{
			t += s.pbScore;
		}
		return t;
	}
	
	public void Highlight()
	{
		nameLabel.ForeColor = current;
		scoreLabel.ForeColor = current;
		nameLabel.BackColor = bgColorHighlighted;
		scoreLabel.BackColor = bgColorHighlighted;
	}
	
	public void Unhighlight()
	{
		nameLabel.ForeColor = text_default;
		nameLabel.BackColor = bgColor;
		scoreLabel.BackColor = bgColor;
	}
	
	private void DoLayout()
	{
		nameLabel.Height = Height;
		scoreLabel.Height = Height;
		nameLabel.Width = 65;
		scoreLabel.Left = 65;
		scoreLabel.Width = Width - 65;
	}
	
}