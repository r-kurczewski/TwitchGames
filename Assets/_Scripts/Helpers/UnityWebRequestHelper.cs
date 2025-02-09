using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public static class UnityWebRequestHelper
{
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
		await req.SendWebRequest();
		return (HttpStatusCode)req.responseCode;
	}

	public static void AddAuthorization(this UnityWebRequest req, string token)
	{
		req.SetRequestHeader("Authorization", $"Bearer {token}");
	}
}
