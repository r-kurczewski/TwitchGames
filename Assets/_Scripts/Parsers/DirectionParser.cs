using System;
using UnityEngine;

public class DirectionParser : BaseMessageParser
{
	private const string downAction = "down";
	private const string upAction = "up";
	private const string leftAction = "left";
	private const string rightAction = "right";

	private Transform target;

	public DirectionParser(Transform moveTarget)
	{
		target = moveTarget;
	}

	public override bool Parse(string line)
	{
		bool match = false;
		Vector2 move = Vector2.zero;
		(var message, _) = GetMessageData(line);
		if (ContainsAction(message, upAction))
		{
			move += Vector2.up;
			match = true;
		}
		if (ContainsAction(message, downAction))
		{
			move += Vector2.down;
			match = true;
		}
		if (ContainsAction(message, leftAction))
		{
			move += Vector2.left;
			match = true;
		}
		if (ContainsAction(message, rightAction))
		{
			move += Vector2.right;
			match = true;
		}
		target.localPosition += (Vector3)move.normalized * 25;
		return match;
	}

	private bool ContainsAction(string message, string actionName)
	{
		return message.Contains(actionName, StringComparison.InvariantCultureIgnoreCase);
	}
}

