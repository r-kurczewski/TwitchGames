using System;

[Serializable]
public class UserResponse
{
	public UserData[] data;

	[Serializable]
	public class UserData
	{
		public string id;
		public string login;
		public string display_name;
		public string type;
		public string broadcaster_type;
		public string description;
		public string profile_image_url;
		public string offline_image_url;
		public int view_count;
		public string created_at;
	}
}

