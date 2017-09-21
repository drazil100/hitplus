using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class ScoreInput : Form
{
	private TextBox inputBox = new TextBox();
	private Button submit = new Button();
	private Button undo = new Button();
	private Button save = new Button();
	
	public int index = 0;
	
	public ScoreInput()
	{
		Size = new Size(300, 89);
		
		submit.Text = "Keep";
		undo.Text = "Back";
		undo.Enabled = false;
		save.Text = "Save & Reset";
		save.Enabled = false;
	
	
		submit.Click += new EventHandler(OnSubmit);
		undo.Click += new EventHandler(OnUndo);
		save.Click += new EventHandler(OnReset);
		
		SwapControls(submit);
		
		//Resize += delegate { DoLayout(); };
		
	}
	
	private int GetWidth()
	{
		return (
				Width - (2 * SystemInformation.FrameBorderSize.Width)
		       );
	}

	private int GetHeight()
	{
		return (
				Height - (2 * SystemInformation.FrameBorderSize.Height +
					SystemInformation.CaptionHeight)
		       );
	}
	
	private void DoLayout()
	{
		inputBox.Height = 25;
		inputBox.Width = GetWidth();
		submit.Top = 25;
		submit.Width = GetWidth()/2;
		submit.Height = 25;
		undo.Top = submit.Top;
		undo.Width = GetWidth() - submit.Width;
		undo.Left = submit.Width;
		undo.Height = submit.Height;
		save.Top = submit.Top;
		save.Width = submit.Width;
		save.Height = submit.Height;
	}
	
	public void SwapControls(Button b)
	{
		Controls.Clear();
		
		if (b != save)
		{
			Controls.Add(inputBox);
			inputBox.Enabled = true;
		}
		else
		{
			inputBox.Enabled = true;
		}
		AcceptButton = b;
		Controls.Add(b);
		Controls.Add(undo);
		
		DoLayout();
	}
	
	
	
	public void OnSubmit(object sender, EventArgs e)
	{
		try
		{
			int s = Int32.Parse(inputBox.Text);
			if (s < 0 || s > 999)
				return;
			Score.scoresList[index].CurrentScore = s;
			inputBox.Text = "";
			Score.scoresList[index].Unhighlight();
			index++;
			if (index != Score.scoresList.Count)
				Score.scoresList[index].Highlight();
			
			if (index > 0)
			{
				undo.Enabled = true;
			}
			
			inputBox.Focus();
			
			if (index == Score.scoresList.Count)
			{
				submit.Enabled = false;
				save.Enabled = true;
				SwapControls(save);
				return;
			}
			else
			{
				submit.Enabled = true;
			}
		}
		catch (Exception)
		{
			
		}
	}
	
	
	public void OnUndo(object sender, EventArgs e)
	{
		try
		{
			if (index < Score.scoresList.Count)
			{
				Score.scoresList[index].CurrentScore = -1;
				Score.scoresList[index].Unhighlight();
			}
			else
			{
				SwapControls(submit);
				save.Enabled = false;
			}
			index--;
			Score.scoresList[index].CurrentScore = -1;
			Score.scoresList[index].Highlight();
			inputBox.Text = "";
			
			
			if (index < Score.scoresList.Count)
			{
				submit.Enabled = true;
			}
			
			inputBox.Focus();
			
			if (index == 0)
			{
				undo.Enabled = false;
				return;
			}
			else
			{
				undo.Enabled = true;
			}
		}
		catch (Exception)
		{
			
		}
	}
	
	public void OnReset(object sender, EventArgs e)
	{
		Score.UpdateBestScores();
		Score.SaveRun();
		SwapControls(submit);
		submit.Enabled = true;
		index = 0;
		if (ScoreTracker.config["start_highlighted"] == "1")
			Score.scoresList[index].Highlight();
		inputBox.Focus();
	}
}