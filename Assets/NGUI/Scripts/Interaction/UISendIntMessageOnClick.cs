using UnityEngine;

/// <summary>
/// Same as UISendMessage, but passes the specified integer component when calling the target function.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Send Int Message (On Click)")]
public class UISendIntMessageOnClick : UISend
{
	public string functionName = "OnSendMessage";
	public int valueToSend = 0;

	void OnClick ()
	{
		Send(functionName, valueToSend);
	}
}