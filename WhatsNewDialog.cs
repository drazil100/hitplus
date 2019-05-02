using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

public class WhatsNewDialog : Form
{
	private TextBox text = new TextBox();
	private Button download = new Button();
	private Button openRepo = new Button();
	private Button remind = new Button();
	private Button skip = new Button();

	public WhatsNewDialog(string version, string whatsNew)
	{
		Text = String.Format("Update Available ({0})", version);
		text.ReadOnly = true;
		text.Multiline = true;
		text.WordWrap = true;
		text.ScrollBars = ScrollBars.Vertical;
		text.Dock = DockStyle.Fill;
		text.SuspendLayout();
		text.Text = whatsNew;
		text.ResumeLayout();
		download.Text = "Download Now";
		openRepo.Text = "Tracker Homepage";
		remind.Text = "Remind Me Later";
		skip.Text = "Skip Version " + version;
		download.Dock = DockStyle.Bottom;
		openRepo.Dock = DockStyle.Bottom;
		remind.Dock = DockStyle.Bottom;
		skip.Dock = DockStyle.Bottom;

		Controls.Add(text);
		Controls.Add(download);
		Controls.Add(openRepo);
		Controls.Add(remind);
		Controls.Add(skip);

		Size s = new Size(500, 350);
		MaximumSize = s;
		MinimumSize = s;

	}
	private int GetCaptionSize()
	{
		return (2 * SystemInformation.FrameBorderSize.Height + SystemInformation.CaptionHeight);
	}
}
