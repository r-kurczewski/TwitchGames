using System;

[Serializable]
public class TokenData
{
	public string accessToken;
	public DateTime accessTokenExpiresAt;
	public string refreshToken;
	public DateTime refreshTokenExpiresAt;
}
