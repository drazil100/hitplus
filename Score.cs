using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public enum PaceStatus
{
	Default,
	Ahead,
	Behind,
	Gold
}
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
	
	private static Color text_color = ScoreTracker.text_color;
	private static Color text_color_highlighted = ScoreTracker.text_color_highlighted;
	private static Color text_color_ahead = ScoreTracker.text_color_ahead;
	private static Color text_color_behind = ScoreTracker.text_color_behind;
	private static Color text_color_best = ScoreTracker.text_color_best;
	private static Color background_color = ScoreTracker.background_color;
	private static Color background_color_highlighted = ScoreTracker.background_color_highlighted;

	private PaceStatus pace = PaceStatus.Default;

	private bool highlighted = false;

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

		RecolorPanel();
		
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

	public void RecolorPanel()
	{
		bool casual = (ScoreTracker.config["casual_mode"] == "0") ? false : true;
		switch (pace)
		{
			case PaceStatus.Ahead:   ColorAhead(casual); break;
			case PaceStatus.Behind:  ColorBehind(casual); break;
			case PaceStatus.Gold: ColorGold(casual); break;
			default: ColorDefault(casual); break;
		}
	}

	public void ColorDefault(bool casual)
	{
		if (ScoreTracker.config["casual_mode"] == "0")
			scoreLabel.Text = ""+pbScore;//+"\n"+best;
		else
			scoreLabel.Text = "";

		if (!highlighted)
		{
			nameLabel.ForeColor = text_color;
			if (casual)
				scoreLabel.ForeColor = background_color;
			else
				scoreLabel.ForeColor = text_color;
			signLabel.ForeColor = background_color;
			paceLabel.ForeColor = background_color;

			BackColor = background_color;
		}
		else
		{
			nameLabel.ForeColor = text_color_highlighted;
			if (casual)
				scoreLabel.ForeColor = background_color_highlighted;
			else
				scoreLabel.ForeColor = text_color_highlighted;
			signLabel.ForeColor = background_color_highlighted;
			paceLabel.ForeColor = background_color_highlighted;

			BackColor = background_color_highlighted;
		}
	}

	public void ColorAhead(bool casual)
	{
		BackColor = background_color;

		if (!casual)
		{
			nameLabel.ForeColor = text_color_ahead;
			scoreLabel.ForeColor = text_color_ahead;
			signLabel.ForeColor = text_color_ahead;
			paceLabel.ForeColor = text_color_ahead;
		}
		else
		{
			nameLabel.ForeColor = text_color;
			scoreLabel.ForeColor = text_color;
			signLabel.ForeColor = background_color;
			paceLabel.ForeColor = background_color;
		}
	}

	public void ColorBehind(bool casual)
	{
		BackColor = background_color;

		if (!casual)
		{
			nameLabel.ForeColor = text_color_behind;
			scoreLabel.ForeColor = text_color_behind;
			signLabel.ForeColor = text_color_behind;
			paceLabel.ForeColor = text_color_behind;
		}
		else
		{
			nameLabel.ForeColor = text_color;
			scoreLabel.ForeColor = text_color;
			signLabel.ForeColor = background_color;
			paceLabel.ForeColor = background_color;
		}
	}

	public void ColorGold(bool casual)
	{
		BackColor = background_color;

		if (!casual)
		{
			nameLabel.ForeColor = text_color_best;
			scoreLabel.ForeColor = text_color_best;
			signLabel.ForeColor = text_color_best;
			paceLabel.ForeColor = text_color_best;
		}
		else
		{
			nameLabel.ForeColor = text_color_best;
			scoreLabel.ForeColor = text_color_best;
			signLabel.ForeColor = background_color;
			paceLabel.ForeColor = background_color;
		}
	}
	
	public int CurrentScore {
		get {return score;}
		set 
		{
			score = value;
			scoreLabel.Text = ""+score;
			int runScore = 0;
			int oldScore = 0;
			for (int i = 0; (i < scoresList.Count && i <= index); i++)
			{
				oldScore += scoresList[i].pbScore;
				runScore += scoresList[i].CurrentScore;
			}
			if (runScore >= oldScore)
			{
				pace = PaceStatus.Ahead;
				paceLabel.Text = String.Format("+{0}", (runScore - oldScore));
				if (ScoreTracker.config ["layout"] == "0")
				{
					paceLabel.Text = String.Format("{0}", (runScore - oldScore));
					signLabel.Text = "+";
				}
			}
			else
			{
				pace = PaceStatus.Behind;
				paceLabel.Text = String.Format("-{0}", (oldScore - runScore));
				if (ScoreTracker.config ["layout"] == "0")
				{
					paceLabel.Text = String.Format("{0}", (oldScore - runScore));
					signLabel.Text = "-";
				}
			}
			if (score > best)
			{
				pace = PaceStatus.Gold;
			}

			if (score == -1)
			{
				pace = PaceStatus.Default;
				if (ScoreTracker.config["casual_mode"] == "0")
					scoreLabel.Text = ""+pbScore;//+"\n"+best;
				else
					scoreLabel.Text = "";
				paceLabel.Text = "";
				signLabel.Text = "";
				if (index == 0 && ScoreTracker.config["start_highlighted"] == "0")
					Unhighlight();

			}

			RecolorPanel();
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
		highlighted = true;
		RecolorPanel();
	}
	
	public void Unhighlight()
	{
		highlighted = false;
		RecolorPanel();
	}
	
	public static void ToggleCasualMode()
	{
		for(int i = 0; i < scoresList.Count; i++)
		{
			scoresList[i].RecolorPanel();
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
