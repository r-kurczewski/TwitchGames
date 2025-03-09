using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public static class UnityWebRequestHelper
{
	private const string clientIdHeader = "Client-Id";
	private const string authorizationHeader = "Authorization";

	public static async UniTask<T> SendRequest<T>(this UnityWebRequest req)
	{
		await req.SendWebRequest();

		if (req.result != UnityWebRequest.Result.Success)
		{
			throw new UnityWebRequestException(req);
		}

		var json = req.downloadHandler.text;
		var result = JsonConvert.DeserializeObject<T>(json);
		return result;
	}

	public static async UniTask<HttpStatusCode> SendRequest(this UnityWebRequest req)
	{
		try
		{
			await req.SendWebRequest();
			return (HttpStatusCode)req.responseCode;
		}
		catch(UnityWebRequestException ex)
		{
			Debug.LogWarning($"Error sending request: {ex.Message}");
			return (HttpStatusCode)ex.ResponseCode;
		}
	}

	public static void AddAuthorization(this UnityWebRequest req, string token)
	{
		req.SetRequestHeader(authorizationHeader, $"Bearer {token}");
	}
	public static void AddClientIdHeader(this UnityWebRequest req, string clientId)
	{
		req.SetRequestHeader(clientIdHeader, clientId);
	}
}
