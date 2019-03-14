using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class ComparisonSelector : Panel
{
	private Button back = new Button();
	private Button next = new Button();
	public ComboBox options = new ComboBox();
	public ComparisonSelector()
	{
		Height = 20;
		back.Text = "<-";
		next.Text = "->";
		back.Width = 40;
		next.Width = 40;
		back.Dock = DockStyle.Left;
		next.Dock = DockStyle.Right;
		options.Dock = DockStyle.Fill;
		this.options.DropDownStyle = ComboBoxStyle.DropDownList;
		Controls.Add(back);
		Controls.Add(next);
		Controls.Add(options);

	}
}
