using System;
using System.Collections;
using System.Collections.Generic;

public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
	private List<KeyValuePair<TKey, TValue>> content = new List<KeyValuePair<TKey, TValue>>();

	public OrderedDictionary() : base() {}

	public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> copyFrom) : base()
	{
		foreach (var pair in copyFrom) {
			this[pair.Key] = pair.Value;
		}
	}

	public int Count
	{
		get {
			return content.Count;
		}
	}

	public bool IsReadOnly
	{
		get {
			return false;
		}
	}

	public ICollection<TKey> Keys
	{
		get {
			return content.ConvertAll(pair => pair.Key);
		}
	}

	public ICollection<TValue> Values
	{
		get {
			return content.ConvertAll(pair => pair.Value);
		}
	}

	public TValue this[TKey key]
	{
		get {
			lock (content)
			{
				int index = IndexOf(key);
				if (index >= 0) {
					return content[index].Value;
				} else {
					throw new KeyNotFoundException();
				}
			}
		}

		set {
			lock (content)
			{
				int index = IndexOf(key);
				if (index >= 0) {
					content[index] = new KeyValuePair<TKey, TValue>(key, value);
				} else {
					content.Add(new KeyValuePair<TKey, TValue>(key, value));
				}
			}
		}
	}

	public int IndexOf(KeyValuePair<TKey, TValue> pair)
	{
		return content.IndexOf(pair);
	}

	public int IndexOf(TKey key)
	{
		return content.FindIndex(pair => pair.Key.Equals(key));
	}

	public TValue GetWithDefault(TKey key, TValue ifMissing)
	{
		TValue result = ifMissing;
		TryGetValue(key, out result);
		return result;
	}

	// required by ICollection
	public void Add(KeyValuePair<TKey, TValue> pair)
	{
		this.Add(pair.Key, pair.Value);
	}

	// required by IDictionary
	public void Add(TKey key, TValue value)
	{
		if (IndexOf(key) >= 0) {
			throw new ArgumentException();
		}
		this[key] = value;
	}

	// required by ICollection
	public void Clear()
	{
		content.Clear();
	}

	// required by ICollection
	public bool Contains(KeyValuePair<TKey, TValue> pair)
	{
		try {
			return this[pair.Key].Equals(pair.Value);
		} catch {
			return false;
		}
	}

	// required by IDictionary
	public bool ContainsKey(TKey key)
	{
		return content.FindIndex(pair => pair.Key.Equals(key)) >= 0;
	}

	// required by ICollection
	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		content.CopyTo(array, arrayIndex);
	}

	// required by ICollection
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
		return content.GetEnumerator();
	}

	// required by ICollection
	IEnumerator IEnumerable.GetEnumerator() {
		return content.GetEnumerator();
	}

	// required by IDictionary
	public bool TryGetValue(TKey key, out TValue value)
	{
		int index = IndexOf(key);
		if (index >= 0) {
			value = content[index].Value;
			return true;
		}
		value = default(TValue);
		return false;
	}

	// required by ICollection
	public bool Remove(KeyValuePair<TKey, TValue> pair)
	{
		int index = IndexOf(pair);
		if (index < 0) {
			return false;
		}
		content.RemoveAt(index);
		return true;
	}

	// required by IDictionary
	public bool Remove(TKey key)
	{
		int index = IndexOf(key);
		if (index < 0) {
			return false;
		}
		content.RemoveAt(index);
		return true;
	}
}
