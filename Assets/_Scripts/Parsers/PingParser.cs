
using System.IO;

public class PingParser : BaseMessageParser
{
	private StreamWriter writer;

	public PingParser(StreamWriter writer)
	{
		this.writer = writer;
	}

	public override bool Parse(string message)
	{
		if (message.StartsWith("PING"))
		{
			writer.WriteLine("PONG :tmi.twitch.tv\r\n");
			writer.Flush();
			return true;
		}
		return false;
	}
}
