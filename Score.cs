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
		return String.Format("{0}:{1}", name, score);
	}
}

public class Score : Panel
{
	private static Dictionary<string, Score> scores = new Dictionary<string, Score>();
	public static List<Score> scoresList = new List<Score>();
	public static List<Entry> topData = new List<Entry>();
	
	private Label nameLabel = new Label();
	private Label scoreLabel = new Label();
	
	
	private Color text_default = ColorTranslator.FromHtml("#99dfff");
	private Color bgColor = ColorTranslator.FromHtml("#052e51");
	private Color current = ColorTranslator.FromHtml("#FF8800");
	private Color ahead = ColorTranslator.FromHtml("#00CC36");
	private Color behind = ColorTranslator.FromHtml("#CC1200");
	private Color bestColor = ColorTranslator.FromHtml("#D8AF1F");

	private int score = 0;
	public int pbScore = 0;
	public int best = 0;
	public string name = "";
	private int index = 0;
	public int arrayIndex = 0;
	
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
			string temp = topData[0].ToString();
		
			for(int i = 1; i < topData.Count; i++)
			{
				temp += "\n" + topData[i].ToString();
			}
			
			try
			{
				string file = @"pb_individuals.txt";
				var sw = new StreamWriter(file);
				sw.Write(temp);
				sw.Close();
			}
			catch (Exception e2)
			{
				Console.WriteLine("Error: " + e2.Message);
			}
		}
	}
	
	public static void SaveRun()
	{
		
		string temp = "";
		int sob = 0;
		int tot = GetTotal();
		bool doSave = false;
		
		if (GetTotal() > GetOldScore())
			doSave = true;
		
		try
		{
			temp = scoresList[0].ToString();
			
			for(int i = 0; i < scoresList.Count; i++)
			{
				if (i > 0)
					temp += "\n" + scoresList[i].ToString();
				
				sob += scoresList[i].best;
				if (doSave)
					scoresList[i].pbScore = scoresList[i].CurrentScore;
				scoresList[i].CurrentScore = 0;
			}
		}
		catch (Exception e3)
		{
			Console.WriteLine("Error: " + e3.Message);
			return;
		}
		
		ScoreWindow.sobScore.Text = "SoB: " + sob;
		string config = "";
		
		if (!doSave)
			return;
		
		ScoreWindow.topScore.Text = "Top: " + tot;
		
		try
		{
			config = File.ReadAllText(@"config.txt", Encoding.UTF8);
			
			try
			{
				string file = "";
				
				if (config == "Hard Route: 0")
					file = @"pb_easy.txt";
				else
					file = @"pb_hard.txt";
				
				var sw = new StreamWriter(file);
				sw.Write(temp);
				sw.Close();
			}
			catch (Exception e2)
			{
				Console.WriteLine("Error: " + e2.Message);
			}
			
		}
		catch (Exception)
		{
			try
			{
				var sw = new StreamWriter(@"emergency_save.txt");
				sw.Write(temp);
				sw.Close();
			}
			catch (Exception e2)
			{
				Console.WriteLine("Error: " + e2.Message);
			}
		}
	}
	
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
		if (index == 0)
			Highlight();
		scoreLabel.ForeColor = text_default;
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
	}
	
	public void Unhighlight()
	{
		nameLabel.ForeColor = text_default;
	}
	
	private void DoLayout()
	{
		nameLabel.Height = Height;
		scoreLabel.Height = Height;
		nameLabel.Width = 60;
		scoreLabel.Left = 60;
		scoreLabel.Width = Width - 60;
	}
	
}