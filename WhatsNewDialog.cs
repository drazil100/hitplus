using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

public class WhatsNewDialog : Form
{
	public bool close = false;
	private string version;
	private string whatsNew;
	private TextBox text = new TextBox();
	private Button download = new Button();
	private Button openRepo = new Button();
	private Button remind = new Button();
	private Button skip = new Button();

	public WhatsNewDialog(string version, string whatsNew)
	{
		this.version = version;
		this.whatsNew = whatsNew;

		download.Click += delegate { 
			MessageBox.Show("The download link to the new version will be opened in your browser and the score tracker will close itself. \nTo install the new score tracker simply replace the old score tracker with the new one. \n\nYour old configuration and score files will be compatible with the new tracker.", "How to install", MessageBoxButtons.OK); 
			Process.Start("https://drazil100.bitbucket.io/SF64ScoreTracker.exe"); 
			close = true;
			this.Close();
		};
		openRepo.Click += delegate { Process.Start("https://bitbucket.org/drazil100/sf64scoretracker/"); };
		remind.Click += delegate { this.Close(); };
		skip.Click += delegate { ScoreTracker.config["skip_version"] = version; this.Close(); };

		Size s = new Size(500, 350);
		MaximumSize = s;
		MinimumSize = s;

		ConfigureControls();

	}

	public void ConfigureControls()
	{
		Text = String.Format("Update Available ({0})", version);
		text.ReadOnly = true;
		text.Multiline = true;
		text.WordWrap = true;
		text.BackColor = Color.White;
		text.ScrollBars = ScrollBars.Vertical;
		text.Dock = DockStyle.Fill;
		text.SuspendLayout();
		text.Text = whatsNew;
		text.ResumeLayout();
		download.Text = "Download Now";
		openRepo.Text = "Homepage";
		remind.Text = "Remind Me Later";
		skip.Text = "Skip This Version";

		Panel buttons = new Panel();
		buttons.Height = 25;
		download.Width = Width/4;
		openRepo.Width = download.Width;
		remind.Width = download.Width;
		skip.Width = download.Width;

		buttons.Dock = DockStyle.Bottom;
		download.Dock = DockStyle.Right;
		openRepo.Dock = DockStyle.Right;
		remind.Dock = DockStyle.Right;
		skip.Dock = DockStyle.Right;

		Controls.Add(buttons);
		Controls.Add(text);
		buttons.Controls.Add(openRepo);
		buttons.Controls.Add(download);
		buttons.Controls.Add(remind);
		buttons.Controls.Add(skip);

		remind.Focus();
	}

	private int GetCaptionSize()
	{
		return (2 * SystemInformation.FrameBorderSize.Height + SystemInformation.CaptionHeight);
	}
	
}
