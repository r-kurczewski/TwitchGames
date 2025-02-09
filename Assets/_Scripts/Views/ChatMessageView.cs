using TMPro;
using UnityEngine;

public class ChatMessageView : MonoBehaviour
{
	public TMP_Text message;

	public void Init(string message)
	{
		this.message.text = message;
	}
}
