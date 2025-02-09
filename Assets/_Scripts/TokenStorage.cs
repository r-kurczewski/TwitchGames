using Gley.AllPlatformsSave;
using System;
using System.IO;
using UnityEngine;
using SaveManager = Gley.AllPlatformsSave.API;

public class TokenStorage : MonoBehaviour
{
	private const string tokenFile = "token.json";

	private readonly TimeSpan expirationThreshold = TimeSpan.FromMinutes(10);

	private readonly TimeSpan refreshTokenExpiry = TimeSpan.FromDays(30);

	private TokenData _tokenData;

	private string TokenPath => Path.Combine(Application.persistentDataPath, tokenFile);

	[SerializeField]
	private bool encrypt = false;

	private TokenData TokenData
	{
		get
		{
			if (_tokenData == null)
			{
				SaveManager.Load<TokenData>(TokenPath, TokenLoading, encrypt);
			}
			return _tokenData;
		}
		set
		{
			SaveManager.Save(value, TokenPath, TokenSaving, encrypt);
			_tokenData = value;
		}
	}

	public string Token => TokenData?.accessToken;

	public bool Authorized => !string.IsNullOrEmpty(Token);

	public string RefreshToken => TokenData.refreshToken;

	public bool TokenExpires => TokenData.accessTokenExpiresAt + expirationThreshold < DateTime.UtcNow;

	public bool TokenExpired => TokenData.accessTokenExpiresAt < DateTime.UtcNow;

	public bool RefreshTokenExpired => TokenData.refreshTokenExpiresAt < DateTime.UtcNow;

	public bool IsTokenValid => Token is not null && !TokenExpires;

	public void UpdateTokens(TokenResponse tokens)
	{
		var now = DateTime.UtcNow;
		TokenData = new TokenData
		{
			accessToken = tokens.access_token,
			refreshToken = tokens.refresh_token,
			accessTokenExpiresAt = now + TimeSpan.FromSeconds(tokens.expires_in),
			refreshTokenExpiresAt = now + refreshTokenExpiry
		};
	}

	public void Clear()
	{
		SaveManager.ClearFile(TokenPath);
		_tokenData = null;
	}

	private void TokenLoading(TokenData data, SaveResult saveResult, string message)
	{
		if (saveResult == SaveResult.Success)
		{
			_tokenData = data;
			Debug.Log("TokenData loaded.");
		}
		else if (saveResult is SaveResult.EmptyData)
		{
			_tokenData = null;
		}
		else
		{
			Debug.LogWarning($"Could not load token: {message}");
		}
	}

	private void TokenSaving(SaveResult saveResult, string message)
	{
		if (saveResult is SaveResult.Success)
		{
			Debug.Log("TokenData saved");
		}
		else Debug.LogError(message);
	}
}