using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ChatController : MonoBehaviour
{
	[SerializeField]
	private string clientId;

	[SerializeField]
	private TwitchApi api;

	[SerializeField]
	private ChatBox chat;

	[SerializeField]
	private TokenStorage tokenStorage;

	[SerializeField]
	private string channelName;

	[SerializeField]
	private UserResponse.UserData userData;

	[SerializeField]
	private Transform moveTarget;

	private void Awake()
	{
		api = new TwitchApi(tokenStorage, clientId);
	}

	public async void Start()
	{
		try
		{
			Debug.Log("Connecting to Twitch API...");
			await ConnectToTwitchApi();

			while (Application.isPlaying)
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

		Debug.Log("Checking user validity...", this);
		await CheckTokenValidity();

		Debug.Log("Getting user data...", this);
		userData = await api.GetUser();

		Debug.Log("Opening chat connection...", this);
		await OpenChat();

		Debug.Log("Starting a poll...");
		await api.CreatePoll(new PollRequest(userData.id,"Poll example", new[] {"A", "B", "C" }, 120, true, 50));
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
		bool isValid = await api.Validate();
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
		var login = userData?.login;
		if (string.IsNullOrEmpty(login))
		{
			Debug.LogError($"{nameof(login)} is empty", this);
			return;
		}

		Debug.Log($"Connecting user {login} to channel {channelName}...", this);
		var chatConnection = await api.OpenChat(login, channelName);

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
			var token = await api.RefreshToken();
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
