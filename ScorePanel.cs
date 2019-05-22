using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class ScorePanel : Panel
{
	private Label nameLabel = new Label();
	private Label scoreLabel = new Label();
	private Label paceLabel = new Label();
	private Label signLabel = new Label();

	private ColorFileReader colors = ScoreTracker.colors;

	private bool highlighted = false;

	public ScoreEntry entry;
	public string displayName = "";
	public int arrayIndex = 0;
	
	private ScorePanel()
	{
		
	}
	
	public ScorePanel(ScoreEntry entry)
	{
		this.entry = entry;
		UpdatePanel();
	}

	public void UpdatePanel()
	{
		displayName = entry.Name;
		
		if (ScoreTracker.config["layout"] == "0")
			displayName = GetAlias(entry.Name);

		Unhighlight();

		if (ScoreTracker.config["highlight_current"] == "1" && entry.IsCurrent)
			Highlight();
		
		scoreLabel.Text = String.Format("{0}", entry.Score);
		nameLabel.Text = String.Format("{0}:", displayName);
		paceLabel.Text = "" + Math.Abs(entry.Pace);
		
		Controls.Add(nameLabel);
		Controls.Add(scoreLabel);

		if (ScoreTracker.config["layout"] == "0")
		{
			if (entry.Pace < 0) 
			{
				signLabel.Text = "-";
			}
			else
			{
				signLabel.Text = "+";
			}
			Controls.Add(signLabel);
			Controls.Add(paceLabel);
		}
		else
		{
			if (entry.Pace < 0) 
			{
				paceLabel.Text = "-" + paceLabel.Text;
			}
			else
			{
				paceLabel.Text = "+" + paceLabel.Text;
			}
			Controls.Add(paceLabel);
		}
		
		
		Resize += delegate { DoLayout(); };
		
		DoLayout();

		RecolorPanel();
	}
	
	public void RecolorPanel()
	{
		bool casual = (ScoreTracker.config["casual_mode"] == "0") ? false : true;
		switch (entry.Status)
		{
			case PaceStatus.Ahead:   ColorAhead(casual);   break;
			case PaceStatus.Behind:  ColorBehind(casual);  break;
			case PaceStatus.Gold:    ColorGold(casual);    break;
			default:                 ColorDefault(casual); break;
		}
	}

	public void ColorDefault(bool casual)
	{
		if (ScoreTracker.config["casual_mode"] == "0")
			scoreLabel.Text = ""+entry.Comparison;
		else
			scoreLabel.Text = "";

		if (!highlighted)
		{
			nameLabel.ForeColor = colors["text_color"];
			if (casual)
				scoreLabel.ForeColor = colors["background_color"];
			else
				scoreLabel.ForeColor = colors["text_color"];
			signLabel.ForeColor = colors["background_color"];
			paceLabel.ForeColor = colors["background_color"];

			BackColor = colors["background_color"];
		}
		else
		{
			nameLabel.ForeColor = colors["text_color_highlighted"];
			if (casual)
				scoreLabel.ForeColor = colors["background_color_highlighted"];
			else
				scoreLabel.ForeColor = colors["text_color_highlighted"];
			signLabel.ForeColor = colors["background_color_highlighted"];
			paceLabel.ForeColor = colors["background_color_highlighted"];

			BackColor = colors["background_color_highlighted"];
		}
	}

	public void ColorAhead(bool casual)
	{
		BackColor = colors["background_color"];

		if (!casual)
		{
			nameLabel.ForeColor = colors["text_color"];
			scoreLabel.ForeColor = colors["text_color_ahead"];
			signLabel.ForeColor = colors["text_color_ahead"];
			paceLabel.ForeColor = colors["text_color_ahead"];
		}
		else
		{
			nameLabel.ForeColor = colors["text_color"];
			scoreLabel.ForeColor = colors["text_color"];
			signLabel.ForeColor = colors["background_color"];
			paceLabel.ForeColor = colors["background_color"];
		}
	}

	public void ColorBehind(bool casual)
	{
		BackColor = colors["background_color"];

		if (!casual)
		{
			nameLabel.ForeColor = colors["text_color"];
			scoreLabel.ForeColor = colors["text_color_behind"];
			signLabel.ForeColor = colors["text_color_behind"];
			paceLabel.ForeColor = colors["text_color_behind"];
		}
		else
		{
			nameLabel.ForeColor = colors["text_color"];
			scoreLabel.ForeColor = colors["text_color"];
			signLabel.ForeColor = colors["background_color"];
			paceLabel.ForeColor = colors["background_color"];
		}
	}

	public void ColorGold(bool casual)
	{
		BackColor = colors["background_color"];

		if (!casual)
		{
			nameLabel.ForeColor = colors["text_color"];
			scoreLabel.ForeColor = colors["text_color_best"];
			signLabel.ForeColor = colors["text_color_best"];
			paceLabel.ForeColor = colors["text_color_best"];
		}
		else
		{
			nameLabel.ForeColor = colors["text_color"];
			scoreLabel.ForeColor = colors["text_color_best"];
			signLabel.ForeColor = colors["background_color"];
			paceLabel.ForeColor = colors["background_color"];
		}
	}
	

	public void Highlight()
	{
		if (ScoreTracker.config["highlight_current"] != "1" || (ScoreTracker.config["start_highlighted"] != "1" && entry.Position == 0))
			return;
		highlighted = true;
		RecolorPanel();
	}
	
	public void Unhighlight()
	{
		highlighted = false;
		RecolorPanel();
	}
	
	public string GetAlias(string n)
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
			case "Sector Z":
			n = "SZ";
			break;
			case "Area 6":
			n = "A6";
			break;
			case "Bolse":
			n = "BO";
			break;
			case "Venom 1":
			case "Venom 2":
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
