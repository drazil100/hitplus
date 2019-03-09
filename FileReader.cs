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
	private List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>();
	private List<KeyValuePair<string, string>> defaultValues = new List<KeyValuePair<string, string>>();
	private Dictionary<string, T> cachedValues = new Dictionary<string, T>();
	private SortingStyle sorting = SortingStyle.Sort;
	private int stackFrame = 3;
	public IEnumerator<KeyValuePair<string, T>> GetEnumerator() {
		lock (content)
		{
			List<KeyValuePair<string, T>> returnContent = new List<KeyValuePair<string, T>>();
			for (int i = 0; i < content.Count; i++)
			{
				string key = content[i].Key;
				if(!cachedValues.ContainsKey(key))
				{
					cachedValues[key] = StringToValue(content[i].Value);
				}
				returnContent.Add(new KeyValuePair<string, T>(key, cachedValues[key]));
			}
			return returnContent.GetEnumerator();
		}
	}

	IEnumerator IEnumerable.GetEnumerator() {
		lock (content)
		{
			List<KeyValuePair<string, T>> returnContent = new List<KeyValuePair<string, T>>();
			for (int i = 0; i < content.Count; i++)
			{
				string key = content[i].Key;
				if(!cachedValues.ContainsKey(key))
				{
					cachedValues[key] = StringToValue(content[i].Value);
				}
				returnContent.Add(new KeyValuePair<string, T>(key, cachedValues[key]));
			}
			return returnContent.GetEnumerator();
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
				string[] parts = line.Split(new char[] { ':' }, 2);
				parts[0] = parts[0].Trim();
				parts[1] = parts[1].Trim();

				WriteDebug(parts[0], parts[1], 5);

				bool contains = false;
				for (int i = 0; i < this.content.Count; i++)
				{
					if (this.content[i].Key == parts[0])
					{
						this.content[i] = new KeyValuePair<string, string>(parts[0], parts[1]);
						contains = true;
						break;
					}
				}
				if (!contains)
				{
					this.content.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
				}
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
					cachedValues.Remove(key);
				}
			}
			for (int i = 0; i < defaultValues.Count; i++)
			{
				if (defaultValues[i].Key == key)
				{
					defaultValues.RemoveAt(i);
				}
			}
		}
	}

	public T this[string key]
	{
		get {
			lock (content)
			{
				if (cachedValues.ContainsKey(key))
				{
					return cachedValues[key];
				}
				foreach (KeyValuePair<string, string> pair in content)
				{
					if (pair.Key == key)
					{
						T val = StringToValue(pair.Value);
						cachedValues[key] = val;
						return val;
					}
				}
			}
			return this.StringToValue("");
		}

		set {
			lock (content)
			{
				string val = ValueToString(value);
				WriteDebug(key, val, 3);

				cachedValues[key] = value;
				
				for (int i = 0; i < content.Count; i++)
				{
					if (content[i].Key == key)
					{
						content[i] = new KeyValuePair<string, string>(key, val);
						return;
					}
				}
				content.Add(new KeyValuePair<string, string>(key, val));
			}
		}

	}
	public void WriteDebug(string key, string value, int frame)
	{

		string caller = GetStackTrace(frame) + " " + fileName;
		if (caller != lastCaller)
		{
			Console.WriteLine();
			Console.WriteLine(caller);
			lastCaller = caller;
		}
		Console.WriteLine(String.Format("  {0} = {1}", key, value));
	}

	public string GetStackTrace(int frame)
	{
		StackTrace st = new StackTrace();
		StackFrame sf = st.GetFrame(frame);
		string n = "." + sf.GetMethod().Name;
		n = (n == "..ctor") ? "" : n;
		string r = String.Format("{0}{1}():", sf.GetMethod().DeclaringType, n);
		return r;
	}
	
	public void AddNewItem(string key, T value)
	{
		stackFrame = 4;
		AddNewItem(key, ValueToString(value));
		stackFrame = 3;
	}

	public void AddNewItem(string key, string value)
	{
		lock (content)
		{
			bool contains = false;
			int j = 0;
			for (int i = 0; i < defaultValues.Count; i++)
			{
				if (defaultValues[i].Key == key)
				{
					contains = true;
					defaultValues[i] = new KeyValuePair<string, string>(key, value);
					j = i;
					break;
				}
			}
			if (!contains)
			{
				defaultValues.Add(new KeyValuePair<string, string>(key, value));
				j = defaultValues.Count - 1;
			}
			for (int i = 0; i < content.Count; i++)
			{
				if (content[i].Key == key)
				{
					if (i != j && sorting != SortingStyle.Unsort)
					{
						KeyValuePair<string, string> temp = content[j];
						content[j] = content[i];
						content[i] = temp;
					}
					return;
				}
			}
			WriteDebug(key, value, stackFrame);
			content.Add(new KeyValuePair<string, string>(key, value));
			if (content.Count-1 != j && sorting != SortingStyle.Unsort)
			{
				KeyValuePair<string, string> temp = content[j];
				content[j] = content[content.Count-1];
				content[content.Count-1] = temp;
			}
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
				while (defaultValues.Count < content.Count)
				{
					content.RemoveAt(defaultValues.Count);
				}
			}
			var sw = new StreamWriter(fileName);
			foreach(KeyValuePair<string, string> pair in content)
			{	
				string val = pair.Value;
				if (cachedValues.ContainsKey(pair.Key))
				{
					val = ValueToString(cachedValues[pair.Key]);
				}
				sw.Write(String.Format("{0}: {1}\r\n", pair.Key, val));
			}
			sw.Close();
		}
	}

	public abstract T StringToValue(string val);
	public abstract string ValueToString(T val);
}

public class FileReader<T> : BaseFileReader<T> where T:class
{
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

	public override string StringToValue(string val)
	{
		return val;
	}

	public override string ValueToString(string val)
	{
		return val;
	}
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
