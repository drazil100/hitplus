using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;


class WWW
{
	public static bool IsAllowedInUrl(char ch)
	{
		if(ch >= 'A' && ch <= 'Z') return true;
		if(ch >= 'a' && ch <= 'z') return true;
		if(ch >= '0' && ch <= '9') return true;
		return ch == '.' || ch == '-' || ch == '_' || ch == '~';
	}

	public static string Encode(string s)
	{
		string encoded = "";

		foreach (char c in s)
		{
			if (IsAllowedInUrl(c))
			{
				encoded += c;
			}
			else
			{
				int value = Convert.ToInt32(c);
				encoded += "%" + String.Format("{0:X}", value);
			}
		}
		return encoded;
	}

	public static string Decode(string s)
	{
		string decoded = "";

		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '%')
			{
				string hex = s.Substring(i + 1, 2);
				i += 2;
				int value = Convert.ToInt32(hex, 16);
				decoded += Char.ConvertFromUtf32(value);	
			}
			else
			{
				decoded += s[i];
			}
		}
		return decoded;
	}

	public static string FormatResource(string fileName, Dictionary<string, string> parameters)
	{
		string resource = fileName;

		foreach (KeyValuePair<string, string> parameter in parameters)
		{
			resource += (resource == fileName) ? "?" : "&";
			resource += String.Format("{0}={1}", Encode(parameter.Key), Encode(parameter.Value));
		}
		return resource;
	}

	public static bool UnformatResource(
			string                     resource,
			out string                 fileName,
			Dictionary<string, string> parameters,
			out string                 errMsg
	)
	{
		errMsg = "";

		if (resource.IndexOf('?') == -1)
		{
			fileName = resource;
			return true;
		}

		string[] split = resource.Split(new char[] { '?' }, 2);
		fileName = split[0];

		string[] sections = split[1].Split('&');
		foreach (string section in sections)
		{
			if (section.IndexOf('=') == -1)
			{
				errMsg = "Failed to parse parameters";
				return false;
			}
			else
			{
				string[] split2 = section.Split(new char[] { '=' }, 2);
				parameters[Decode(split2[0])] = Decode(split2[1]);
			}
		}
		return true;
	}

	public static byte[] ReadHttpHeader(NetworkStream stream, out ByteBuffer buffer)
	{
		buffer = new ByteBuffer();
		int delimiter = -1;
		int delimiterSize = 4;

		byte[] data = new byte[4096];
		do
		{
			int count = stream.Read(data, 0, data.Length);
			if (count <= 0)
			{
				throw new HttpException(500, "Internal Server Error", "error reading request");
			}
			buffer.Add(data, count);
			delimiter = buffer.Find(HttpHeader.CrLfCrLf);
			if (delimiter == -1) 
			{
				delimiter = buffer.Find(HttpHeader.LfLf);
				if (delimiter != -1)
					delimiterSize = 2;
			}
		} while(delimiter == -1);

		return buffer.ExtractPrefix(delimiter + delimiterSize);
	}
}

class HttpException : Exception
{
	public HttpException(int statusCode, string statusMessage, string errMsg)
		: base(errMsg)
	{
		this.StatusCode = statusCode;
		this.StatusMessage = statusMessage;
	}

	public HttpException(int statusCode, string statusMessage, string errMsg, Exception inner)
		: base(errMsg, inner)
	{
		this.StatusCode = statusCode;
		this.StatusMessage = statusMessage;
	}

	public int StatusCode { get; set; }
	public string StatusMessage { get; set; }
}
