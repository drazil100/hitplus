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
	private Label paceLabel = new Label();
	private Label signLabel = new Label();
	
	
	private Color text_default = ScoreTracker.text_color;
	private Color bgColor = ScoreTracker.background_color;
	private Color bgColorHighlighted = ScoreTracker.background_color_highlighted;
	private Color current = ScoreTracker.text_color_highlighted;
	private Color ahead = ScoreTracker.text_color_ahead;
	private Color behind = ScoreTracker.text_color_behind;
	private Color bestColor = ScoreTracker.text_color_best;

	private int score = -1;
	public int pbScore = 0;
	public int best = 0;
	public string name = "";
	public string displayName = "";
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
		displayName = name;
		if (ScoreTracker.config["layout"] == "0")
			displayName = GetName(name);
		index = scoresList.Count;
		scoresList.Add(this);
		nameLabel.ForeColor = text_default;
		scoreLabel.ForeColor = text_default;
		nameLabel.BackColor = bgColor;
		scoreLabel.BackColor = bgColor;
		paceLabel.BackColor = bgColor;
		signLabel.BackColor = bgColor;
		if (index == 0 && ScoreTracker.config["start_highlighted"] == "1")
			Highlight();
		this.name = name;
		this.pbScore = score;
		scoreLabel.Text = String.Format("{0}", pbScore);
		nameLabel.Text = String.Format("{0}:", displayName);
		
		Controls.Add(nameLabel);
		Controls.Add(scoreLabel);
		if (ScoreTracker.config["layout"] == "0")
			Controls.Add(signLabel);
		Controls.Add(paceLabel);
		
		Resize += delegate { DoLayout(); };
		
		DoLayout();
		
	}
	
	~Score()
	{
		scores.Remove(this.name);
		scoresList.Remove(this);
	}

	public static void ClearScores ()
	{
		scores.Clear ();
		scoresList.Clear ();
		topData.Clear ();
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
			if (score > best)
			{
				scoreLabel.ForeColor = bestColor;
				paceLabel.ForeColor = bestColor;
				signLabel.ForeColor = bestColor;
			}
			if (score == -1)
			{
				scoreLabel.ForeColor = bgColor;
				scoreLabel.Text = ""+pbScore;//+"\n"+best;
				paceLabel.Text = "";
				signLabel.Text = "";
			}
			if (ScoreTracker.config["casual_mode"] == "0")
				ColorLabels();
		}
	}
	
	public void ColorLabels()
	{
		int runScore = 0;
		int oldScore = 0;
		for (int i = 0; (i < scoresList.Count && i <= index); i++)
		{
			oldScore += scoresList[i].pbScore;
			runScore += scoresList[i].CurrentScore;
		}
		if (runScore >= oldScore)
		{
			scoreLabel.Text = String.Format("{0}", score);
			paceLabel.Text = String.Format("+{0}", (runScore - oldScore));
			scoreLabel.ForeColor = ahead;
			paceLabel.ForeColor = ahead;
			if (ScoreTracker.config ["layout"] == "0")
			{
				paceLabel.Text = String.Format("{0}", (runScore - oldScore));
				signLabel.Text = "+";
				signLabel.ForeColor = ahead;
			}
		}
		else
		{
			scoreLabel.Text = String.Format("{0}", score);
			paceLabel.Text = String.Format("-{0}", (oldScore - runScore));
			scoreLabel.ForeColor = behind;
			paceLabel.ForeColor = behind;
			if (ScoreTracker.config ["layout"] == "0")
			{
				paceLabel.Text = String.Format("{0}", (oldScore - runScore));
				signLabel.Text = "-";
				signLabel.ForeColor = behind;
			}
		}
		if (score > best)
		{
			scoreLabel.ForeColor = bestColor;
			paceLabel.ForeColor = bestColor;
			signLabel.ForeColor = bestColor;
		}
		
		if (score == -1)
		{
			scoreLabel.ForeColor = text_default;
			scoreLabel.Text = ""+pbScore;//+"\n"+best;
			paceLabel.Text = "";
			paceLabel.Text = text_default;
			signLabel.Text = "";
			signLabel.Text = text_default;
			if (index == 0 && ScoreTracker.config["start_highlighted"] == "0")
				Unhighlight();
			
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
				bool contains = false;
				foreach(Entry entry in topData)
				{
					if (entry.name == scoresList[i].name)
					{
						entry.score = scoresList[i].CurrentScore;
						contains = true;
					}
				}
				if (!contains)
				{
					topData.Add(new Entry(scoresList[i].name, scoresList[i].CurrentScore));
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
			scoresList[i].CurrentScore = -1;
		}
		ScoreTracker.sobScore.Text = "" + sob;
		
		//string config = "";
		
		if (!doSave)
			return;
		file.Save();
		
		if (ScoreTracker.config["include_route_pbs_in_individuals_file"] == "1")
		{
			int total = 0;
			foreach (KeyValuePair<string, string> pair in ScoreTracker.pbEasy)
			{
				total += Int32.Parse(pair.Value);
			}
			if (total > 0)
			{
				ScoreTracker.individualLevels["Easy Route"] = "" + total;
			}
			total = 0;
			foreach (KeyValuePair<string, string> pair in ScoreTracker.pbHard)
			{
				total += Int32.Parse(pair.Value);
			}
			if (total > 0)
			{
				ScoreTracker.individualLevels["Hard Route"] = "" + total;
			}
			ScoreTracker.individualLevels.Save();
		}
		
		ScoreTracker.topScore.Text = "" + tot;
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
		if (ScoreTracker.config["highlight_current"] != "1")
			return;
		nameLabel.ForeColor = current;
		scoreLabel.ForeColor = current;
		paceLabel.ForeColor = current;
		nameLabel.BackColor = bgColorHighlighted;
		scoreLabel.BackColor = bgColorHighlighted;
		paceLabel.BackColor = bgColorHighlighted;
		signLabel.BackColor = bgColorHighlighted;
	}
	
	public void Unhighlight()
	{
		nameLabel.ForeColor = text_default;
		nameLabel.BackColor = bgColor;
		scoreLabel.BackColor = bgColor;
		paceLabel.BackColor = bgColor;
		signLabel.BackColor = bgColor;
		if (ScoreTracker.config["casual_mode"] == "1")
		{
			scoreLabel.ForeColor = text_default;
			paceLabel.ForeColor = bgcolor;
			signLabel.ForeColor = bgColor;
		}
	}
	
	public static void ToggleCasualMode()
	{
		for(int i = 0; i < scoresList.Count; i++)
		{
			if (ScoreTracker.config["casual_mode"] == "0")
			{
				scoresList[i].ColorLabels();
			}
			else
			{
				if (scoresList[i].CurrentScore > -1)
				{
					scoresList[i].scoreLabel.ForeColor = text_default;
				}
				else
				{
					scoresList[i].scoreLabel.ForeColor = bgColor;
				}
				scoresList[i].paceLabel.ForeColor = bgColor;
				scoresList[i].signLabel.ForeColor = bgColor;
			}
		}
	}
	
	public string GetName(string n)
	{
		switch (n)
		{
			case "Corneria":
			n = "CO";
			break;
			case "Meteo":
			n = "ME";
			break;
			case "Katina":
			n = "KA";
			break;
			case "Sector X":
			n = "SX";
			break;
			case "Sector Y":
			n = "SY";
			break;
			case "Aquas":
			n = "AQ";
			break;
			case "Zoness":
			n = "ZO";
			break;
			case "Macbeth":
			n = "MA";
			break;
			case "Area 6":
			n = "a6";
			break;
			case "Venom":
			n = "VE";
			break;
		}
		
		return n;
	}
	
	private void DoLayout()
	{
		if (ScoreTracker.config["layout"] == "0")
		{
			nameLabel.Height = Height/2;
			scoreLabel.Height = Height/2;
			nameLabel.Width = 65;
			scoreLabel.Left = 65;
			scoreLabel.Width = Width - 65;
			paceLabel.Top = scoreLabel.Height;
			paceLabel.Height = Height/2;
			paceLabel.Width = scoreLabel.Width;
			paceLabel.Left = nameLabel.Left + nameLabel.Width;
			//scoreLabel.TextAlign = ContentAlignment.TopRight;
			signLabel.TextAlign = ContentAlignment.TopRight;
			signLabel.Width = nameLabel.Width;
			signLabel.Height = paceLabel.Height;
			signLabel.Top = nameLabel.Height;
		}
		else
		{
			nameLabel.Height = Height;
			scoreLabel.Height = Height;
			paceLabel.Height = Height;
			scoreLabel.TextAlign = ContentAlignment.TopRight;
			paceLabel.TextAlign = ContentAlignment.TopRight;
			nameLabel.Width = 150;
			paceLabel.Left = nameLabel.Width;
			paceLabel.Width = (Width - nameLabel.Width) / 2;
			scoreLabel.Left = paceLabel.Left + paceLabel.Width;
			scoreLabel.Width = paceLabel.Width;
		}
	}
	
}