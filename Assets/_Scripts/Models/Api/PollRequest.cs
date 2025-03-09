using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class PollRequest
{
	public string broadcaster_id;
	public string title;
	public List<PollChoice> choices;
	public int duration;
	public bool channel_points_voting_enabled;
	public int channel_points_per_vote;

	public PollRequest(string broadcasterId, string title, IList<string> choices, int duration)
	{
		this.broadcaster_id = broadcasterId;
		this.title = title;
		this.choices = choices.Select(choice => new PollChoice(choice)).ToList();
		this.duration = duration;
		channel_points_voting_enabled = false;
		channel_points_per_vote = 0;
	}

	public PollRequest(
		string broadcasterId,
		string title,
		IList<string> choices,
		int duration,
		bool channel_points_voting_enabled,
		int channel_points_per_vote) 
		: this(broadcasterId, title, choices, duration)
	{
		this.channel_points_voting_enabled = channel_points_voting_enabled;
		this.channel_points_per_vote = channel_points_per_vote;
	}

	[Serializable]
	public class PollChoice
	{
		public string title;

		public PollChoice(string title)
		{
			this.title = title;
		}
	}
}
