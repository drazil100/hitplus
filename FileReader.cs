using System.Text;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

public enum SortingStyle
{
	Unsort,
	Sort,
	Validate
}

public abstract class BaseFileReader<T> : IEnumerable<KeyValuePair<string, T>>
{
	private static string lastCaller = "";
	private string fileName = "";
	private List<KeyValuePair<string, T>> content = new List<KeyValuePair<string, T>>();
	private int newItemCounter = 0;
	private SortingStyle sorting = SortingStyle.Sort;
	public IEnumerator<KeyValuePair<string, T>> GetEnumerator() {
		lock (content)
		{
			return content.GetEnumerator();
		}
	}

	IEnumerator IEnumerable.GetEnumerator() {
		lock (content)
		{
			return content.GetEnumerator();
		}
	}

	private BaseFileReader() : base() {}
	public BaseFileReader(string file, SortingStyle sorting = SortingStyle.Sort) : base()
	{
		this.sorting = sorting;
		fileName = file;
		try
		{
			string content = File.ReadAllText(file, Encoding.UTF8);

			string[] lines = content.Split(new string[] { "\n" }, StringSplitOptions.None);

			foreach (string line in lines)
			{
				string[] parts = line.Split(':');
				parts[0] = parts[0].Trim();
				parts[1] = parts[1].Trim();

				this[parts[0]] = this.StringToValue(parts[1]);
			}

		}
		catch (Exception)
		{

		}

	}

	public void RemoveKey(string key)
	{
		lock (content)
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
	}

	public T this[string key]
	{
		get {
			lock (content)
			{
				foreach (KeyValuePair<string, T> pair in content)
				{
					if (pair.Key == key)
						return pair.Value;
				}
			}
			return this.StringToValue("");
		}

		set {
			lock (content)
			{
				string caller = GetStackTrace() + " " + fileName;
				if (caller != lastCaller)
				{
					Console.WriteLine();
					Console.WriteLine(caller);
					lastCaller = caller;
				}

				Console.WriteLine(String.Format("  {0} = {1}", key, value));
				for (int i = 0; i < content.Count; i++)
				{
					if (content[i].Key == key)
					{
						content[i] = new KeyValuePair<string, T>(key, value);
						return;
					}
				}
				content.Add(new KeyValuePair<string, T>(key, value));
			}
		}

	}

	public string GetStackTrace()
	{
		StackTrace st = new StackTrace();
		StackFrame sf = st.GetFrame(2);
		string n = "." + sf.GetMethod().Name;
		n = (n == "..ctor") ? "" : n;
		string r = String.Format("{0}{1}():", sf.GetMethod().DeclaringType, n);
		return r;
	}

	public void AddNewItem(string key, T value)
	{
		lock (content)
		{
			for (int i = 0; i < content.Count; i++)
			{
				if (content[i].Key == key)
				{
					if (i != newItemCounter && sorting != SortingStyle.Unsort)
					{
						KeyValuePair<string, T> temp = content[newItemCounter];
						content[newItemCounter] = content[i];
						content[i] = temp;
					}
					newItemCounter++;
					return;
				}
			}
			content.Add(new KeyValuePair<string, T>(key, value));
			if (content.Count-1 != newItemCounter && sorting != SortingStyle.Unsort)
			{
				KeyValuePair<string, T> temp = content[newItemCounter];
				content[newItemCounter] = content[content.Count-1];
				content[content.Count-1] = temp;
			}
			newItemCounter++;
		}
	}

	public void Save()
	{
		lock (content)
		{
			Console.WriteLine(" Writing to " + fileName);
			Console.WriteLine();
			lastCaller = "";
			if (sorting == SortingStyle.Validate)
			{
				while (newItemCounter < content.Count)
				{
					content.RemoveAt(newItemCounter);
				}
			}
			var sw = new StreamWriter(fileName);
			foreach(KeyValuePair<string, T> pair in content)
			{
				sw.Write(String.Format("{0}: {1}\r\n", pair.Key, pair.Value));
			}
			sw.Close();
		}
	}

	public abstract T StringToValue(string val);
	public abstract string ValueToString(T val);
}

public class FileReader<T> : BaseFileReader<T> where T:class {
	public FileReader(string file, SortingStyle sorting = SortingStyle.Sort) : base(file, sorting) {}

	public override T StringToValue(string val)
	{
		return Activator.CreateInstance(typeof(T), new object[]{ val }) as T;
	}

	public override string ValueToString(T val)
	{
		return val.ToString();
	}
}

public class FileReader : FileReader<string>
{
	public FileReader(string file, SortingStyle sorting = SortingStyle.Sort) : base(file, sorting) {}
}

public class ColorFileReader : BaseFileReader<Color>
{
	public ColorFileReader(string file, SortingStyle sorting = SortingStyle.Sort) : base(file, sorting) {}

	public override Color StringToValue(string val)
	{
		return ColorTranslator.FromHtml(val);
	}

	public override string ValueToString(Color val)
	{
		return ColorTranslator.ToHtml(val);
	}
}
