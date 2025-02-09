using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class ChatBox : MonoBehaviour
{
	[SerializeField]
	private int messageBufferCount;

	[SerializeField]
	private ChatMessageView messagePrefab;

	private BaseMessageParser[] parsers;

	private StreamReader reader;
	private StreamWriter writer;
	private TcpClient client;

	private Queue<ChatMessageView> messages = new();

	public bool CanRead => (client?.Connected ?? false) && client.Available > 0;

	public void Init(ChatConnection connection, params BaseMessageParser[] parsers)
	{
		reader = connection.reader;
		writer = connection.writer;
		client = connection.client;
		this.parsers = parsers;
	}

	public void Update()
	{
		while (CanRead) HandleMessage();
	}

	private void HandleMessage()
	{
		string message = reader.ReadLine();
		foreach (var parser in parsers)
		{
			var stop = parser.Parse(message);
			if (stop) break;
		}
	}

	public void AddMessage(string message)
	{
		ChatMessageView view;
		if (messages.Count < messageBufferCount)
		{
			view = Instantiate(messagePrefab, transform);
			view.name = $"Message {view.transform.parent.childCount.ToString()}";
		}
		else
		{
			view = messages.Dequeue();
			view.transform.SetAsLastSibling();
		}
		messages.Enqueue(view);
		view.Init(message);
	}
}
