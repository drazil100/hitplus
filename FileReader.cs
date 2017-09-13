using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class FileReader : Dictionary<string, string>
{
	private string fileName = "";
	
	private FileReader() : base() {}
	public FileReader(string file) : base()
	{
		fileName = file;
		try
		{
			string contents = File.ReadAllText(file, Encoding.UTF8);
			
			string[] lines = contents.Split(new string[] { "\r\n" }, StringSplitOptions.None);
			
			foreach (string line in lines)
			{
				string[] parts = line.Split(':');
				
				this[parts[0]] = parts[1].Trim();
			}
			
		}
		catch (Exception)
		{
			
		}
			
	}
	
	public void Save()
	{
		var sw = new StreamWriter(fileName);
		foreach(KeyValuePair<string, string> pair in this)
		{
			sw.Write(String.Format("{0}: {1}\r\n", pair.Key, pair.Value));
		}
		sw.Close();
	}
	
}