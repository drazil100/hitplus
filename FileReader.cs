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

public class DebugVariables
{
	public static string lastCaller = "";
	public static string lastFileName = "";
}

public abstract class BaseFileReader<T> : IEnumerable<KeyValuePair<string, T>>
{
	private string fileName = "";
	private List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>();
	private List<KeyValuePair<string, string>> defaultValues = new List<KeyValuePair<string, string>>();
	private Dictionary<string, T> cachedValues = new Dictionary<string, T>();
	private SortingStyle sorting = SortingStyle.Sort;
	private int stackDepth = 3;

	private IEnumerator<KeyValuePair<string, T>> GetEnumeratorInternal() {
		lock (content)
		{
			var returnContent = new List<KeyValuePair<string, T>>();
			foreach (var pair in content) {
				returnContent.Add(new KeyValuePair<string, T>(pair.Key, this[pair.Key]));
			}
			return returnContent.GetEnumerator();
		}
	}

	public IEnumerator<KeyValuePair<string, T>> GetEnumerator() {
  	return this.GetEnumeratorInternal();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return this.GetEnumeratorInternal();
	}

	public string FileName
	{
		get { return fileName; }
		set { fileName = value; }
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
				if (parts.Length < 2) continue;
				parts[0] = parts[0].Trim();
				parts[1] = parts[1].Trim();

				WriteDebug(MakeModifyMessage(parts[0], parts[1]));

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
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);
		}

	}

	public T this[string key]
	{
		get {
			lock (content)
			{
				if (!cachedValues.ContainsKey(key)) {
					int index = content.FindIndex(pair => pair.Key == key);
					if (index >= 0) {
						this.cachedValues[key] = StringToValue(content[index].Value);
					} else {
						index = defaultValues.FindIndex(pair => pair.Key == key);
						if (index >= 0) {
							this.cachedValues[key] = StringToValue(defaultValues[index].Value);
						}
					}
				}
				return this.cachedValues[key];
			}
		}

		set {
			lock (content)
			{
				string val = ValueToString(value);
				WriteDebug(MakeModifyMessage(key, val), 3);

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
	
	public void AddNewItem(string key, T value)
	{
		stackDepth = 4;
		AddNewItem(key, ValueToString(value));
		stackDepth = 3;
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
			WriteDebug("ADD: " + MakeModifyMessage(key, value), stackDepth);
			content.Add(new KeyValuePair<string, string>(key, value));
			if (content.Count-1 != j && sorting != SortingStyle.Unsort)
			{
				KeyValuePair<string, string> temp = content[j];
				content[j] = content[content.Count-1];
				content[content.Count-1] = temp;
			}
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
					WriteDebug(String.Format("DEL: {0}", key), 3);
					break;
				}
			}
			for (int i = 0; i < defaultValues.Count; i++)
			{
				if (defaultValues[i].Key == key)
				{
					defaultValues.RemoveAt(i);
					break;
				}
			}
		}
	}

	public bool ContainsKey(string key)
	{
		bool contains = false;
		foreach (KeyValuePair<string, string> pair in content)
		{
			if (pair.Key == key)
			{
				contains = true;
				break;
			}
		}
		return contains;
	}

	private string MakeModifyMessage(string key, string value)
	{
		return String.Format("{0} = {1}", key, value);
	}

	private void WriteDebug(string message, int frame = -1)
	{

		string caller = "FileReader(): " + fileName;
		if (frame > -1) caller = GetStackTrace(frame) + " " + fileName;
		//Console.WriteLine(String.Format("caller: {0}, lastCaller: {1}, fileName: {2}, lastFileName {3}", caller, lastCaller, fileName, lastFileName));
		if (caller != DebugVariables.lastCaller || fileName != DebugVariables.lastFileName)
		{
			Console.WriteLine();
			Console.WriteLine(caller);
			DebugVariables.lastCaller = caller;
			DebugVariables.lastFileName = fileName;
		}
		Console.WriteLine("  " + message);
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

	public void Save()
	{
		lock (content)
		{
			Console.WriteLine(" Writing to " + fileName);
			Console.WriteLine();
			DebugVariables.lastCaller = "";
			DebugVariables.lastFileName = "";
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

public class FileReader : BaseFileReader<string>
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
