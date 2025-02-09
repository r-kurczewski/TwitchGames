using System;
using System.IO;
using System.Net.Sockets;

[Serializable]
public class ChatConnection
{
	public string channelName;
	public StreamReader reader;
	public StreamWriter writer;
	public TcpClient client;

	public ChatConnection(string channelName,StreamReader reader, StreamWriter writer, TcpClient client)
	{
		this.channelName = channelName;
		this.reader = reader;
		this.writer = writer;
		this.client = client;
	}
}
