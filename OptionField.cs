using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;


public class OptionField : Panel
{
	public virtual string GetOption()
	{
		return "";
	}
}

public class NameTextBox : TextBox
{
	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		base.OnKeyPress(e);
		// Check if the pressed character was a backspace or numeric.
		e.Handled = !(
				e.KeyChar == (char)8 ||
				Char.IsWhiteSpace(e.KeyChar) ||
				Char.IsLetterOrDigit(e.KeyChar)
			     );
	}
}

public class ColorField : OptionField
{
	public Label name = new Label();
	public ColorDialog color = new ColorDialog();
	private Button button = new Button();

	public ColorField(string name, Color color)
	{
		this.name.Text = name;
		this.color.Color = color;

		button.BackColor = color;
		button.Click += new EventHandler(OnClick);

		this.Controls.Add(this.name);
		this.Controls.Add(this.button);
		Layout += new LayoutEventHandler((object sender, LayoutEventArgs e) => DoLayout());
	}

	public override string GetOption()
	{
		return ColorTranslator.ToHtml(color.Color);
	}

	public string GetName()
	{
		return name.Text;
	}

	public Color GetColor()
	{
		return color.Color;
	}

	public void OnClick(object sender, EventArgs e)
	{
		if (color.ShowDialog() == DialogResult.OK)
			button.BackColor = color.Color;
	}

	public void DoLayout()
	{
		Height = 20;

		name.Dock = DockStyle.Fill;
		button.Dock = DockStyle.Right;

		button.Width = 60;
	}
}

public class NumericField : OptionField
{
	private Label name = new Label ();
	private NumericTextBox score = new NumericTextBox ();

	public new string Name
	{
		get { return name.Text; }
	}

	public string Number
	{
		get
		{
			if (score.Text != "")
				return score.Text;
			return "0";
		}
	}

	public NumericTextBox.OnChanged Changed
	{
		set { score.Changed = value; }
	}

	public NumericField(string name, string number)
	{
		this.name.Text = name;
		this.score.Text = number;

		Controls.Add(this.name);
		Controls.Add(this.score);

		Height = 20;

		Layout += new LayoutEventHandler((object sender, LayoutEventArgs e) => DoLayout());

		DoLayout();
	}

	public void DoLayout()
	{
		score.Height = 20;
		score.Width = 60;
		score.Dock = DockStyle.Right;
		name.Dock = DockStyle.Fill;
	}

	public override string ToString()
	{
		return Number;
	}
}

public class DropdownField : OptionField
{
	public Label name = new Label();
	public ComboBox options = new ComboBox();

	public DropdownField (string name, string current, params Object[] options)
	{
		this.name.Text = name;
		this.options.Items.AddRange (options);

		this.options.SelectedIndex = Int32.Parse(current);
		this.options.DropDownStyle = ComboBoxStyle.DropDownList;
		Controls.Add(this.name);
		Controls.Add(this.options);

		Layout += new LayoutEventHandler((object sender, LayoutEventArgs e) => DoLayout());

		DoLayout();
	}
	public void DoLayout()
	{
		Height = 20;

		options.Dock = DockStyle.Right;
		name.Dock = DockStyle.Fill;
	}
	public override string ToString()
	{
		return "" + options.SelectedIndex;
	}

	public override string GetOption()
	{
		return options.Text;
	}

}

public class CheckField : OptionField
{
	public Label name = new Label();
	public CheckBox option = new CheckBox();

	public CheckField(string name, string current)
	{
		this.name.Text = name;
		option.Checked = (current == "0") ? false : true;

		Controls.Add(this.name);
		Controls.Add(this.option);

		Layout += new LayoutEventHandler((object sender, LayoutEventArgs e) => DoLayout());

		DoLayout();
	}

	public void DoLayout()
	{
		Height = 20;

		option.Dock = DockStyle.Right;
		name.Dock = DockStyle.Fill;
	}

	public override string ToString()
	{
		return (option.Checked) ? "1" : "0";
	}
}

public class TextField : OptionField
{
	public Label name = new Label();
	public NameTextBox text = new NameTextBox();

	public new string Name
	{
		get { return name.Text; }
	}

	public TextField(string name, string text)
	{
		this.name.Text = name;
		this.text.Text = text;

		Controls.Add(this.name);
		Controls.Add(this.text);

		Height = 20;

		Layout += new LayoutEventHandler((object sender, LayoutEventArgs e) => DoLayout());

		DoLayout();
	}

	public void DoLayout()
	{
		text.Height = 20;
		text.Width = 120;
		text.Dock = DockStyle.Right;
		name.Dock = DockStyle.Fill;
	}

	public override string ToString()
	{
		return text.Text;
	}
}
