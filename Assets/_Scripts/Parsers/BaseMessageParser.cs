public abstract class BaseMessageParser
{
	/// <summary>
	/// Parse message and return if next parsers should be run.
	/// </summary>
	/// <param name="line"></param>
	/// <returns></returns>
	public abstract bool Parse(string line);

	protected (string message, string author) GetMessageData(string line)
	{
		var authorSplitPoint = line.IndexOf('!');
		var chatMessageSplitPoint = line.IndexOf(':', 1);

		var author = line[1..authorSplitPoint];
		var chatMessage = line[(chatMessageSplitPoint + 1)..];
		return (chatMessage, author);
	}
}
