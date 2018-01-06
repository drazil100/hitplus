using System;
using System.Text;

/// ByteBuffer wraps a byte array, resizing it when new content is added to it.
/// It supports locating delimiters within the array and removing complete
/// messages from the beginning of the buffer.
class ByteBuffer
{
	private byte[] contents;

	/// <summary>Constructs an empty ByteBuffer.</summary>
	public ByteBuffer()
	{
	}

	/// <summary>Adds the entire contents of a byte array to the
	///     end of the ByteBuffer.</summary>
	/// <param name="contents">The new data to be added</param>
	public void Add(byte[] contents)
	{
		this.Add(contents, contents.Length);
	}

	/// <summary>Adds part of a byte array to the end of the 
	///     ByteBuffer.</summary>
	/// <param name="contents">The array containing the new data to be
	///     added</param>
	/// <param name="length">The number of bytes to add from the
	///     input array</param>
	public void Add(byte[] contents, int length)
	{
		if (this.contents == null) 
		{
			// If the buffer is currently empty, create a new array
			// and copy the data into it.
			this.contents = new byte[length];
			System.Array.Copy(contents, this.contents, length);
		}
		else
		{
			// If the buffer is not currently empty, resize it to
			// accommodate the new data before copying it in.
			int oldLength = this.contents.Length;
			Array.Resize(ref this.contents, oldLength + length);
			Array.Copy(contents, 0, this.contents, oldLength, length);
		}
	}

	/// <summary>Locate a series of bytes within the ByteBuffer.</summary>
	/// <param name="needle">The bytes to be found</param>
	/// <returns>The position of the sought bytes, or -1 if not 
	///     found</returns>
	public int Find(byte[] needle)
	{
		if (needle.Length == 0) return 0;

		int scanLength = this.contents.Length - needle.Length + 1;
		if (scanLength < 0)
		{
			return -1;
		}

		for (int i = 0; i < scanLength; i++)
		{
			if (this.contents[i] == needle[0])
			{
				bool found = true;
				for (int j = 1; j < needle.Length; j++)
				{
					if (this.contents[i + j] != needle[j])
					{
						found = false;
						break;
					}
				}

				if (found == true)
				{
					return i;
				}
			}
		}

		return -1;
	}

	/// <summary>Returns data from the beginning of the ByteBuffer and
	///     removes it.</summary>
	/// <param name="length">The number of bytes to extract</param>
	/// <returns>The bytes removed from the buffer</returns>
	public byte[] ExtractPrefix(int length)
	{
		byte[] prefix;

		if (length >= this.contents.Length)
		{
			prefix = this.contents;
			this.contents = null;
		}
		else
		{
			prefix = new byte[length];
			System.Array.Copy(this.contents, prefix, length);
			byte[] newContents = new byte[this.contents.Length - length];
			System.Array.Copy(this.contents, length, newContents, 0, this.contents.Length - length);
			this.contents = newContents;
		}

		return prefix;
	}

	/// <summary>Returns the contents of the buffer as ASCII text.
	///     Bytes that are not printable ASCII characters will be replaced
	///     with '?' characters.</summary>
	/// <returns>A representation of the content</returns>
	public override string ToString()
	{
		return Encoding.ASCII.GetString(this.contents, 0, this.contents.Length);
	}

	/// <summary>Provides read-only access to the current contents of the
	///     buffer.</summary>
	public byte[] Contents
	{
		get { return this.contents; }
	}

	/// <summary>The number of bytes currently in the buffer.</summary>
	public int Length
	{
		get { return this.contents.Length; }
	}
}

