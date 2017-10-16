using System.Text;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public enum SortingStyle
{
	Unsort,
	Sort,
	Validate
}

public class FileReader : IEnumerable<KeyValuePair<string,string>>
{
	private string fileName = "";
	private List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>();
	private int newItemCounter = 0;
	private SortingStyle sorting = SortingStyle.Sort;
	public IEnumerator<KeyValuePair<string,string>> GetEnumerator() {
		return content.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return content.GetEnumerator();
	}

	private FileReader() : base() {}
	public FileReader(string file, SortingStyle sorting = SortingStyle.Sort) : base()
	{
		this.sorting = sorting;
		fileName = file;
		try
		{
			string contents = File.ReadAllText(file, Encoding.UTF8);

			string[] lines = contents.Split(new string[] { "\n" }, StringSplitOptions.None);

			foreach (string line in lines)
			{
				string[] parts = line.Split(':');
				parts[0] = parts[0].Trim();
				parts[1] = parts[1].Trim();
				
				switch (parts[0])
				{
					case "Cornaria":
					case "CO":
					parts[0] = "Corneria";
					break;
					case "ME":
					parts[0] = "Meteo";
					break;
					case "KA":
					parts[0] = "Katina";
					break;
					case "SX":
					parts[0] = "Sector X";
					break;
					case "SY":
					parts[0] = "Sector Y";
					break;
					case "AQ":
					parts[0] = "Aquas";
					break;
					case "ZO":
					parts[0] = "Zoness";
					break;
					case "MA":
					parts[0] = "Macbeth";
					break;
					case "a6":
					parts[0] = "Area 6";
					break;
					case "VE":
					parts[0] = "Venom";
					break;
				}

				this[parts[0]] = parts[1];
			}

		}
		catch (Exception)
		{

		}

	}
	
	public void RemoveKey(string key)
	{
		for (int i = 0; i < content.Count; i++)
		{
			if (content[i].Key == key)
			{
				content.RemoveAt(i);
				if (newItemCounter >= content.Count)
					newItemCounter = content.Count-1;
			}
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
				if (i != newItemCounter && sorting != SortingStyle.Unsort)
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
		if (content.Count-1 != newItemCounter && sorting != SortingStyle.Unsort)
		{
			KeyValuePair<string, string> temp = content[newItemCounter];
			content[newItemCounter] = content[content.Count-1];
			content[content.Count-1] = temp;
		}
		newItemCounter++;
	}

	public void Save()
	{
		if (sorting == SortingStyle.Validate)
		{
			while (newItemCounter < content.Count)
			{
				content.RemoveAt(newItemCounter);
			}
		}
		var sw = new StreamWriter(fileName);
		foreach(KeyValuePair<string, string> pair in content)
		{
			sw.Write(String.Format("{0}: {1}\r\n", pair.Key, pair.Value));
		}
		sw.Close();
	}

}
