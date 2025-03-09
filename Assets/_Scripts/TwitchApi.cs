using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

public class TwitchApi
{
	private const string AuthUrl = "https://id.twitch.tv/oauth2/device";
	private const string TokenUrl = "https://id.twitch.tv/oauth2/token";
	private const string UsersUrl = "https://api.twitch.tv/helix/users";
	private const string ValidateUrl = "https://id.twitch.tv/oauth2/validate";
	private const string PollsUrl = "https://api.twitch.tv/helix/polls";

	private const string clientIdKey = "client_id";
	private const string refreshTokenKey = "refresh_token";
	private const string scopesKey = "scopes";
	private const string deviceCodeKey = "device_code";
	private const string grantTypeKey = "grant_type";

	private const string deviceCodeGrantType = "urn:ietf:params:oauth:grant-type:device_code";
	private const string refreshTokenGrantType = "refresh_token";
	private const string jsonContentType = "application/json";

	private TokenStorage tokenStorage;
	private string clientId;

	private string Scopes => string.Join(" ",
		"chat:read",
		"channel:manage:polls"
		//,"chat:edit"
		);

	public TwitchApi(TokenStorage tokenStorage, string clientId)
	{
		this.tokenStorage = tokenStorage;
		this.clientId = clientId;
	}

	public async UniTask<AuthResponse> AuthorizeAsync()
	{
		var fields = new Dictionary<string, string>()
		{
			{clientIdKey, clientId},
			{scopesKey, Scopes}
		};

		using var req = UnityWebRequest.Post(AuthUrl, fields);

		var res = await req.SendRequest<AuthResponse>();
		return res;
	}

	public async UniTask<TokenResponse> GetToken(AuthResponse authResponse)
	{
		var fields = new Dictionary<string, string>()
		{
			{clientIdKey, clientId },
			{scopesKey, Scopes },
			{deviceCodeKey, authResponse.device_code },
			{grantTypeKey, deviceCodeGrantType }
		};
		using var req = UnityWebRequest.Post(TokenUrl, fields);
		var res = await req.SendRequest<TokenResponse>();

		return res;
	}

	public async UniTask<UserResponse.UserData> GetUser()
	{
		using var req = UnityWebRequest.Get(UsersUrl);
		req.AddAuthorization(tokenStorage.Token);
		req.AddClientIdHeader(clientId);

		var response = await req.SendRequest<UserResponse>();
		var userData = response.data.Single();
		return userData;
	}

	public async UniTask<TokenResponse> RefreshToken()
	{
		var fields = new Dictionary<string, string>()
		{
			{ clientIdKey, clientId},
			{grantTypeKey, refreshTokenGrantType },
			{refreshTokenKey, tokenStorage.RefreshToken }
		};
		using var req = UnityWebRequest.Post(TokenUrl, fields);
		var res = await req.SendRequest<TokenResponse>();
		return res;
	}

	public async UniTask<bool> Validate()
	{
		using var req = UnityWebRequest.Get(ValidateUrl);
		req.AddAuthorization(tokenStorage.Token);

		var statusCode = await req.SendRequest();
		return statusCode == HttpStatusCode.OK;
	}

	public async UniTask<ChatConnection> OpenChat(string login, string channelName)
	{
		var client = new TcpClient("irc.chat.twitch.tv", 6667);
		var reader = new StreamReader(client.GetStream());
		var writer = new StreamWriter(client.GetStream());

		await writer.WriteLineAsync($"PASS oauth:{tokenStorage.Token}");
		await writer.WriteLineAsync($"NICK {login.ToLower()}");
		await writer.WriteLineAsync($"JOIN #{channelName.ToLower()}");
		await writer.FlushAsync();

		return new ChatConnection(channelName, reader, writer, client);
	}

	public async UniTask CreatePoll(PollRequest request)
	{
		var data = JsonUtility.ToJson(request);

		using var req = UnityWebRequest.Post(PollsUrl, data, jsonContentType);
		req.AddAuthorization(tokenStorage.Token);
		req.AddClientIdHeader(clientId);

		await req.SendRequest();
	}
}