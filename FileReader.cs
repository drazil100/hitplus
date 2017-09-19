using System.Text;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class FileReader : IEnumerable<KeyValuePair<string,string>>
{
	private string fileName = "";
	private List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>();
	private int newItemCounter = 0;

	public IEnumerator<KeyValuePair<string,string>> GetEnumerator() {
		return content.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return content.GetEnumerator();
	}

	private FileReader() : base() {}
	public FileReader(string file) : base()
	{
		fileName = file;
		try
		{
			string contents = File.ReadAllText(file, Encoding.UTF8);

			string[] lines = contents.Split(new string[] { "\n" }, StringSplitOptions.None);

			foreach (string line in lines)
			{
				string[] parts = line.Split(':');

				this[parts[0].Trim()] = parts[1].Trim();
			}

		}
		catch (Exception)
		{

		}

	}

	public string this[string key]
	{
		get {
			foreach (KeyValuePair<string, string> pair in content)
			{
				if (pair.Key == key)
					return pair.Value;
			}
			return "";
		}

		set {
			for (int i = 0; i < content.Count; i++)
			{
				if (content[i].Key == key)
				{
					content[i] = new KeyValuePair<string, string>(key, value);
					return;
				}
			}
			content.Add(new KeyValuePair<string, string>(key, value));
		}
	}

	public void AddNewItem(string key, string value)
	{
		for (int i = 0; i < content.Count; i++)
		{
			if (content[i].Key == key)
			{
				if (i != newItemCounter)
				{
					KeyValuePair<string, string> temp = content[newItemCounter];
					content[newItemCounter] = content[i];
					content[i] = temp;
				}
				newItemCounter++;
				return;
			}
		}
		content.Add(new KeyValuePair<string, string>(key, value));
		if (content.Count-1 != newItemCounter)
		{
			KeyValuePair<string, string> temp = content[newItemCounter];
			content[newItemCounter] = content[content.Count-1];
			content[content.Count-1] = temp;
		}
		newItemCounter++;
	}

	public void Save()
	{
		var sw = new StreamWriter(fileName);
		foreach(KeyValuePair<string, string> pair in content)
		{
			sw.Write(String.Format("{0}: {1}\r\n", pair.Key, pair.Value));
		}
		sw.Close();
	}

}
