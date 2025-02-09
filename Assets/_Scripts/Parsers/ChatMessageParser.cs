public class ChatMessageParser : BaseMessageParser
{
	private ChatBox chat;

	public ChatMessageParser(ChatBox chat)
	{
		this.chat = chat;
	}
	public override bool Parse(string line)
	{
		if (line.Contains("PRIVMSG"))
		{
			var formatedMessage = GetFormatedMessage(line);

			chat.AddMessage(formatedMessage);
			return false;
		}
		return true;
	}

	private string GetFormatedMessage(string line)
	{
		(var message, var author) = GetMessageData(line);
		return $"<b><color=#fe0>{author}</color></b>\n{message}\n";
	}
}

