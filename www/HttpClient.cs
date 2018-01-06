using System.Net.Sockets;
using System.Text;
using System.Net;
using System;
using System.IO;
using System.Collections.Generic;

public class HttpClient
{
	public static bool DoGetRequest(
			string                     host,
			int                        port,
			string                     path,
			Dictionary<string, string> parameters,
			out string                 contents,
			out string                 errMsg
			)
	{
		contents = errMsg = "";

		path = WWW.FormatResource(path, parameters);

		TcpClient     client = null;
		NetworkStream stream = null;

		try
		{
			client = new TcpClient(host, port);
			stream = client.GetStream();
			HttpRequest request = new HttpRequest("GET", path);
			request["User-Agent"] = "SimpleClient/1.1";
			request["Host"] = host + ":" + port;
			string message = request.ToString(); 
			Console.Write(message);

			byte[] data    = Encoding.ASCII.GetBytes(message);

			stream.Write(data, 0, data.Length);

			ByteBuffer buffer;
			byte[] headerContents = WWW.ReadHttpHeader(stream, out buffer);

			HttpResponse response = HttpResponse.Parse(headerContents);
			//contents = response.ToString();

			int contentLength = -1;
			if (response.ContainsKey("Content-Length"))
			{
				contentLength = Convert.ToInt32(response["Content-Length"]);
			}

			data = new byte[4096];
			while (contentLength == -1 || contentLength > buffer.Length)
			{
				// read here, be sure to catch exceptions and break out of the loop
				int count = stream.Read(data, 0, data.Length);
				if (count <= 0)
				{
					break;
				}
				buffer.Add(data, count);

			}

			if(response["Content-Type"].StartsWith("text/"))
			{
				contents += buffer.ToString();
			}
			else
			{
				string destinationFile;
				if (path.EndsWith("/"))
				{
					string extension = response["Content-Type"].Split('/')[1];
					destinationFile = "index." + extension;
				}
				else
				{
					string[] pathSegments = path.Split('/');
					destinationFile = pathSegments[pathSegments.Length - 1];
				}

				File.WriteAllBytes(destinationFile, buffer.Contents);
				contents += "Output written to " + destinationFile;
			}
		}
		catch(Exception e)
		{
			HttpException ex = e as HttpException;
			if (ex != null)
				errMsg = String.Format("{0} {1}: {2}\n{3}", ex.StatusCode, ex.StatusMessage, e.Message, e.StackTrace);
			else
				errMsg = e.Message + "\n" + e.StackTrace;
		}

		if(stream != null)
			stream.Close();

		if(client != null)
			client.Close();

		return(errMsg == "");
	}

	public static bool DoGetRequest(
			string                     host, 
			int                        port, 
			string                     path, 
			out string                 contents, 
			out string                 errMsg
			)
	{
		return DoGetRequest(host, port, path, new Dictionary<string, string>(), out contents, out errMsg);
	}

	public static string ClientMain(string target)
	{
		int    port     = 8888;
		string host     = "localhost";
		string path     = "/test.txt";
		string contents = "";
		string errMsg   = "";
		Dictionary<string, string> parameters = new Dictionary<string, string>();

		string[] url = target.Split(new char[] { '/' }, 2);
		string[] domain = url[0].Split(':');
		host = domain[0];
		port = (domain.Length == 2) ? Convert.ToInt32(domain[1]) : 80;
		if (url.Length == 1)
			path = "/";
		else
			path = "/" + url[1];

		//Console.WriteLine(host);
		//Console.WriteLine(port);
		//Console.WriteLine(path);
		
		string resource = path;

		if (!WWW.UnformatResource(resource, out path, parameters, out errMsg))
		{
			//Console.WriteLine(errMsg);
			return "";
		}
		

		if(!DoGetRequest(host, port, path, parameters, out contents, out errMsg))
		{
			//Console.WriteLine(errMsg);
		}
		else
		{
			//Console.WriteLine(contents);
			return contents;
		}
		return "";
	}
}
