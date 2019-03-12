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

public struct SectionKeyValue<T>
{
	public string Section;
	public string Key;
	public T Value;

	public SectionKeyValue(string section, string key, T value)
	{
		Section = section;
		Key = key;
		Value = value;
	}
}

public abstract class BaseFileReader<T> : IEnumerable<SectionKeyValue<T>>
{
	protected sealed class StackMonitor
	{
		private int depth;

		private class StackMonitorBlock : IDisposable
		{
			private StackMonitor owner;

			public StackMonitorBlock(StackMonitor owner) : base()
			{
				this.owner = owner;
				owner.depth++;
			}

			public void Dispose()
			{
				owner.depth--;
				GC.SuppressFinalize(this);
			}
		}

		public StackMonitor(int baseDepth) : base()
		{
			depth = baseDepth;
		}

		public IDisposable Block
		{
			get {
				return new StackMonitorBlock(this);
			}
		}

		public int Depth
		{
			get {
				return depth;
			}
		}
	}
	protected StackMonitor stackMonitor = new StackMonitor(3);

	private char[] keySeparator = { '=' };
	private static string[] lineSeparator = { "\n" };
	private OrderedDictionary<string, OrderedDictionary<string, string>> content = new OrderedDictionary<string, OrderedDictionary<string, string>>();
	private OrderedDictionary<string, OrderedDictionary<string, string>> defaultValues = new OrderedDictionary<string, OrderedDictionary<string, string>>();
	private Dictionary<Tuple<string, string>, T> cachedValues = new Dictionary<Tuple<string, string>, T>();
	private SortingStyle sorting = SortingStyle.Sort;

	private IEnumerator<SectionKeyValue<T>> GetEnumeratorInternal()
	{
		lock (content)
		{
			var first = (sorting != SortingStyle.Unsort) ? defaultValues : content;
			var second = (sorting != SortingStyle.Unsort) ? content : defaultValues;
			foreach (var firstSectionPair in first) {
				var section = firstSectionPair.Key;
				var firstSection = firstSectionPair.Value;
				var secondSection = second.GetWithDefault(section, null);
				foreach (var pair in firstSection) {
					yield return new SectionKeyValue<T>(section, pair.Key, this[section, pair.Key]);
				}
				if (secondSection == null || sorting == SortingStyle.Validate) {
					continue;
				}
				foreach (var pair in secondSection) {
					if (!firstSection.ContainsKey(pair.Key)) {
						// if it's in the first section then it's already been output
						continue;
					}
				}
			}
			if (sorting == SortingStyle.Validate) {
				yield break;
			}
			foreach (var secondSectionPair in second) {
				var section = secondSectionPair.Key;
				if (first.ContainsKey(section)) {
					// if it's in the first container then it's already been output
					continue;
				}
				foreach (var pair in secondSectionPair.Value) {
					yield return new SectionKeyValue<T>(section, pair.Key, this[section, pair.Key]);
				}
			}
			yield break;
		}
	}

	public IEnumerator<SectionKeyValue<T>> GetEnumerator() {
		return this.GetEnumeratorInternal();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return this.GetEnumeratorInternal();
	}

	public string Name
	{
		get { return Path.GetFileNameWithoutExtension(FileName); }
	}

	public string FileName { get; set; }

	public string DefaultSection { get; set; }

	public char KeySeparator
	{
		get { return keySeparator[0]; }
		set { keySeparator[0] = value; }
	}

	public BaseFileReader(SortingStyle sorting = SortingStyle.Sort) : base()
	{
		this.sorting = sorting;
		FileName = "";
		DefaultSection = "General";
	}

	public BaseFileReader(string file, SortingStyle sorting = SortingStyle.Sort) : this(sorting)
	{
		FileName = file;

		string sectionName = DefaultSection;
		var section = new OrderedDictionary<string, string>();
		content.Add(sectionName, section);

		try
		{
			string fileContent = File.ReadAllText(file, Encoding.UTF8);

			string[] lines = fileContent.Split(lineSeparator, StringSplitOptions.None);

			foreach (string line in lines)
			{
				string[] parts = line.Split(keySeparator, 2);
				parts[0] = parts[0].Trim();
				if (parts.Length == 1) {
					if (parts[0][0] == '[') {
						int end = parts[0].IndexOf(']');
						sectionName = parts[0].Substring(1, end - 1);
						if (content.ContainsKey(sectionName)) {
							section = content[sectionName];
						} else {
							section = content[sectionName] = new OrderedDictionary<string, string>();
						}
					}
					continue;
				}
				parts[1] = parts[1].Trim();

				WriteDebug(MakeModifyMessage(sectionName, parts[0], parts[1]), true);

				section[parts[0]] = parts[1];
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
			return this[DefaultSection, key];
		}

		set {
			this[DefaultSection, key] = value;
		}
	}

	public T this[string section, string key]
	{
		get {
			var tuple = new Tuple<string, string>(section, key);
			lock (content)
			{
				if (!cachedValues.ContainsKey(tuple)) {
					string value;
					try {
						value = content[section][key];
					} catch {
						value = defaultValues[section][key];
					}
					cachedValues[tuple] = StringToValue(value);
				}
				return cachedValues[tuple];
			}
		}

		set {
			var tuple = new Tuple<string, string>(section, key);
			lock (content)
			{
				string val = ValueToString(value);
				WriteDebug(MakeModifyMessage(section, key, val));

				if (!content.ContainsKey(section)) {
					content[section] = new OrderedDictionary<string, string>();
				}
				content[section][key] = val;
				cachedValues[tuple] = value;
			}
		}

	}

	public void AddNewItem(string key, T value)
	{
		using (stackMonitor.Block) {
			AddNewItem(DefaultSection, key, value);
		}
	}

	public void AddNewItem(string section, string key, T value)
	{
		using (stackMonitor.Block) {
			AddNewItem(section, key, ValueToString(value));
		}
	}

	public void AddNewItem(string key, string value)
	{
		using (stackMonitor.Block) {
			AddNewItem(DefaultSection, key, value);
		}
	}

	public void AddNewItem(string section, string key, string value)
	{
		lock (content)
		{
			if (!defaultValues.ContainsKey(section)) {
				defaultValues[section] = new OrderedDictionary<string, string>();
			}
			defaultValues[section][key] = value;
			WriteDebug("ADD: " + MakeModifyMessage(section, key, value));
		}
	}

	public void RemoveKey(string key)
	{
		RemoveKey(DefaultSection, key);
	}

	public void RemoveKey(string section, string key)
	{
		lock (content)
		{
			var tuple = new Tuple<string, string>(section, key);
			if (cachedValues.ContainsKey(tuple)) {
				cachedValues.Remove(tuple);
			}
			if (content.ContainsKey(section) && content[section].ContainsKey(key)) {
				content[section].Remove(key);
			}
			if (defaultValues.ContainsKey(section) && defaultValues[section].ContainsKey(key)) {
				defaultValues[section].Remove(key);
			}
		}
	}

	public bool ContainsKey(string key)
	{
		return ContainsKey(DefaultSection, key);
	}

	public bool ContainsKey(string section, string key)
	{
		return (content.ContainsKey(section) && content[section].ContainsKey(key)) ||
			(defaultValues.ContainsKey(section) && defaultValues[section].ContainsKey(key));
	}

	private string MakeModifyMessage(string section, string key, string value)
	{
		return String.Format("[{0}] {1} = {2}", section, key, value);
	}

	private void WriteDebug(string message, bool inConstructor = false)
	{
		string caller = (inConstructor ? "FileReader()" : GetStackTrace(stackMonitor.Depth)) + ": " + FileName;
		//Console.WriteLine(String.Format("caller: {0}, lastCaller: {1}, FileName: {2}, lastFileName {3}", caller, lastCaller, FileName, lastFileName));
		if (caller != DebugVariables.lastCaller || FileName != DebugVariables.lastFileName)
		{
			Console.WriteLine();
			Console.WriteLine(caller);
			DebugVariables.lastCaller = caller;
			DebugVariables.lastFileName = FileName;
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
			Console.WriteLine(" Writing to " + FileName);
			Console.WriteLine();
			DebugVariables.lastCaller = "";
			DebugVariables.lastFileName = "";

			using (var sw = new StreamWriter(FileName)) {
				string lastSection = null;
				foreach (var entry in this)
				{
					if (lastSection != entry.Section) {
						sw.Write(String.Format("[{0}]\n", entry.Section));
						lastSection = entry.Section;
					}
					sw.Write(String.Format("{0}: {1}\r\n", entry.Key, ValueToString(entry.Value)));
				}
			}
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
