using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class OptionsAboutTab : OptionsTab
{
	public OptionsAboutTab()
	{
		Text = "About";
		TextBox about = new TextBox();
		about.ReadOnly = true;
		about.Multiline = true;
		about.WordWrap = true;
		about.ScrollBars = ScrollBars.Vertical;
		about.Dock = DockStyle.Fill;
		about.SuspendLayout();
		about.Text = "Star Fox 64 Score Tracker\nVersion: " + ScoreTracker.version + "\n\nLicense:\n" + ScoreTracker.license;
		about.ResumeLayout();
		Controls.Add(about);
	}
}
