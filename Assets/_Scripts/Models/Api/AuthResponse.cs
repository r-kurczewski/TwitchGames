using System;

[Serializable]
public class AuthResponse
{
	public string device_code;
	public int expires_in;
	public int interval;
	public string user_code;
	public string verification_uri;
}
