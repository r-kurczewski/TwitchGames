using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;


public class TwitchApi : MonoBehaviour
{
	private const string AuthUrl = "https://id.twitch.tv/oauth2/device";
	private const string TokenUrl = "https://id.twitch.tv/oauth2/token";
	private const string UsersUrl = "https://api.twitch.tv/helix/users";
	private const string ValidateUrl = "https://id.twitch.tv/oauth2/validate";

	private const string clientIdKey = "client_id";
	private const string refreshTokenKey = "refresh_token";
	private const string scopesKey = "scopes";
	private const string deviceCodeKey = "device_code";
	private const string grantTypeKey = "grant_type";
	private const string deviceCodeGrantType = "urn:ietf:params:oauth:grant-type:device_code";
	private const string refreshTokenGrantType = "refresh_token";

	private const string clientIdHeader = "Client-Id";

	public string clientId;

	private StreamReader reader;
	private StreamWriter writer;
	private TcpClient client;

	private string Scopes => string.Join(" ",
		"chat:read"
		//,"chat:edit"
		);

	[SerializeField]
	private UserResponse.UserData userData;


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

	public async UniTask<UserResponse.UserData> GetUser(string token)
	{
		using var req = UnityWebRequest.Get(UsersUrl);
		req.AddAuthorization(token);
		req.SetRequestHeader(clientIdHeader, clientId);

		var response = await req.SendRequest<UserResponse>();
		userData = response.data.First();
		return userData;
	}

	public async UniTask<TokenResponse> RefreshToken(string refreshToken)
	{
		var fields = new Dictionary<string, string>()
		{
			{ clientIdKey, clientId},
			{grantTypeKey, refreshTokenGrantType },
			{refreshTokenKey, refreshToken }
		};
		using var req = UnityWebRequest.Post(TokenUrl, fields);
		var res = await req.SendRequest<TokenResponse>();
		return res;
	}

	public async UniTask<bool> Validate(string token)
	{
		using var req = UnityWebRequest.Get(ValidateUrl);
		req.AddAuthorization(token);

		var statusCode = await req.SendRequest();
		return statusCode == HttpStatusCode.OK;
	}

	public async UniTask<ChatConnection> OpenChat(string token, string channelName)
	{
		var login = (await GetUser(token)).login.ToLower();

		client = new TcpClient("irc.chat.twitch.tv", 6667);
		reader = new StreamReader(client.GetStream());
		writer = new StreamWriter(client.GetStream());

		writer.WriteLine($"PASS oauth:{token}");
		writer.WriteLine($"NICK {login}");
		writer.WriteLine($"JOIN #{channelName.ToLower()}");
		writer.Flush();

		return new ChatConnection(channelName, reader, writer, client);
	}
}