using System;
using System.Collections.Generic;
using System.Text;

abstract public class HttpHeader : Dictionary<string, string>
{
	protected const string version = "HTTP/1.1";
	public static readonly byte[] CrLfCrLf = new byte[] { 13, 10, 13, 10 };
	public static readonly byte[] LfLf = new byte[] { 10, 10 };

	/// <exception cref="HttpException">Throws HttpException if the
	///     header violates the HTTP/1.1 specification.
	///     </exception>
	protected static bool ParseHeaderLines(string[] lines, HttpHeader header)
	{
		for (int i = 1; i < lines.Length; i++)
		{
			int index = lines[i].IndexOf(':');
			if (index == -1)
			{
				throw new HttpException(400, "Bad Request", "no separator found");
			}

			string key = lines[i].Substring(0, index);
			string value = lines[i].Substring(index + 1).Trim();

			if (header.ContainsKey(key))
			{
				header[key] += "," + value;
			}
			else
			{
				header[key] = value;
			}
		}

		return true;
	}

	protected static string[] SplitLines(byte[] data)
	{
		string message = Encoding.ASCII.GetString(data, 0, data.Length);

		// Convert CRLF to LF to support non-spec-compliant peers
		message = message.Trim().Replace("\r\n", "\n");
		return message.Split('\n');
	}

	protected HttpHeader() 
		: base(StringComparer.OrdinalIgnoreCase)
	{
	}

	protected abstract string FirstLine 
	{
		get;
	}

	public override string ToString()
	{
		string header = FirstLine + "\r\n";

		foreach (KeyValuePair<string, string> entry in this)
		{
			header += String.Format("{0}: {1}\r\n", entry.Key, entry.Value);
		}
		
		return header + "\r\n";
	}

}

public class HttpResponse : HttpHeader
{
	public byte[] body = null;

	/// <exception cref="HttpException">Throws HttpException if the
	///     response header violates the HTTP/1.1 specification.
	///     </exception>
	public static HttpResponse Parse(byte[] data)
	{
		string[] lines = HttpHeader.SplitLines(data);
		string[] firstLine = lines[0].Split(new char[] { ' ' }, 3);

		if(firstLine[0] != HttpHeader.version)
		{
			throw new HttpException(500, "Internal Server Error", "Incorrect HTTP version");
		}

		HttpResponse response = new HttpResponse(Convert.ToInt32(firstLine[1]), firstLine[2]);
		HttpHeader.ParseHeaderLines(lines, response);

		return response;
	}

	public HttpResponse(int statusCode, string statusMessage) 
		: base()
	{
		this.StatusCode = statusCode;
		this.StatusMessage = statusMessage;
	}

	protected override string FirstLine
	{
		get { return String.Format("{0} {1} {2}", HttpHeader.version, this.StatusCode, this.StatusMessage); }
	}

	public int StatusCode { get; set; }
	public string StatusMessage { get; set; }
}

public class HttpRequest : HttpHeader
{

	public Dictionary<string, string> parameters = new Dictionary<string, string>();

	/// <exception cref="HttpException">Throws HttpException if the
	///     request header violates the HTTP/1.1 specification.
	///     </exception>
	public static HttpRequest Parse(byte[] data)
	{
		string[] lines = HttpHeader.SplitLines(data);
		string[] firstLine = lines[0].Split(' ');
		if (firstLine.Length != 3)
		{
			throw new HttpException(400, "Bad Request", "malformed first line in request header");
		}

		if(firstLine[2] != HttpHeader.version)
		{
			throw new HttpException(400, "Bad Request", "Incorrect HTTP version");
		}

		HttpRequest request = new HttpRequest(firstLine[0], firstLine[1]);
		HttpHeader.ParseHeaderLines(lines, request);

		return request;
	}

	public HttpRequest(string verb, string url) 
		: base()
	{
		this.Verb = verb;
		
		string errMsg = "", resource = url;
		WWW.UnformatResource(resource, out url, parameters, out errMsg);
		this.Url = url;
	}

	protected override string FirstLine
	{
		get { return String.Format("{0} {1} {2}", this.Verb, WWW.FormatResource(this.Url, parameters), HttpHeader.version); }
	}

	public string Verb { get; set; }
	public string Url { get; set; }
}
