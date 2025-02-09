using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ChatController : MonoBehaviour
{
	[SerializeField]
	private TwitchApi api;

	[SerializeField]
	private ChatBox chat;

	[SerializeField]
	private TokenStorage tokenStorage;

	[SerializeField]
	private string channelName;

	[SerializeField]
	private Transform moveTarget;

	public async void Start()
	{
		try
		{
			Debug.Log("Connecting to Twitch API...");
			await ConnectToTwitchApi();

			while (true)
			{
				await UniTask.Delay(TimeSpan.FromHours(1).Milliseconds, ignoreTimeScale: true);
				await CheckTokenValidity();
			}
		}
		catch (InvalidTokenException)
		{
			Debug.LogError("Invalid token. Restarting authorization process...", this);
			DisconnectApp();
			await ConnectToTwitchApi();
		}
	}

	private async UniTask ConnectToTwitchApi()
	{
		if (tokenStorage.Authorized && tokenStorage.RefreshTokenExpired)
		{
			Debug.Log("Stored refresh token expired. Disconnecting app...", this);
			DisconnectApp();
		}

		if (!tokenStorage.Authorized)
		{
			Debug.Log("Authorizing...", this);
			await Authorize();
		}

		await CheckTokenValidity();

		Debug.Log("Opening chat connection...", this);
		await OpenChat();
	}

	private async UniTask CheckTokenValidity()
	{
		var isTokenValid = await ValidateToken();
		if (!isTokenValid && !tokenStorage.TokenExpired)
		{
			throw new InvalidTokenException();
		}

		if (tokenStorage.TokenExpires)
		{
			Debug.Log("Refreshing token...", this);
			await RefreshToken();
		}
	}

	private async Task<bool> ValidateToken()
	{
		bool isValid = await api.Validate(tokenStorage.Token);
		if (!isValid) Debug.LogWarning("Token validation failed!", this);
		return isValid;
	}

	private async UniTask OpenChat()
	{
		if (string.IsNullOrEmpty(channelName))
		{
			Debug.LogError($"{nameof(channelName)} is empty", this);
			return;
		}

		Debug.Log($"Connecting to channel {channelName}...", this);
		var chatConnection = await api.OpenChat(tokenStorage.Token, channelName);

		Debug.Log($"{channelName} chat opened.", this);
		chat.Init(chatConnection,
			new PingParser(chatConnection.writer),
			new ChatMessageParser(chat),
			new DirectionParser(moveTarget)
			);
	}

	private async UniTask RefreshToken()
	{
		try
		{
			var token = await api.RefreshToken(tokenStorage.RefreshToken);
			tokenStorage.UpdateTokens(token);
		}
		catch (Exception ex)
		{
			Debug.LogError($"Error occurred while refreshing token: {ex}", this);
			DisconnectApp();
		}
	}

	private async UniTask Authorize()
	{
		var auth = await api.AuthorizeAsync();

		Application.OpenURL(auth.verification_uri);
		await UniTask.WhenAny(RegainedFocus(), WaitForUserReturn());

		var token = await api.GetToken(auth);

		tokenStorage.UpdateTokens(token);
	}

	private void DisconnectApp()
	{
		tokenStorage.Clear();
	}

	private async UniTask RegainedFocus()
	{
		await UniTask.WaitUntil(() => !Application.isFocused);
		await UniTask.WaitUntil(() => Application.isFocused);
	}

	private async UniTask WaitForUserReturn()
	{
		await UniTask.Delay(5000);
	}
}
