using UnityEngine;

/// <summary>
/// Same as UISendMessage, but passes the specified integer component when calling the target function.
/// DEPRECATED: This script has been deprecated as of version 1.30.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Deprecated/Send Int Message (On Click)")]
public class UISendIntMessageOnClick : UISend
{
	public string functionName = "OnSendMessage";
	public int valueToSend = 0;

	void Start ()
	{
		Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " uses a deprecated script: " + GetType() +
			"\nConsider using UIButtonMessage or UIButtonTween instead.", this);
	}

	void OnClick ()
	{
		Send(functionName, valueToSend);
	}
}