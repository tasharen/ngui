using UnityEngine;

/// <summary>
/// Calls "OnState" function on all of the scripts attached to the specified
/// target when the script receives OnClick message from UICamera.
/// DEPRECATED: This script has been deprecated as of version 1.30.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Deprecated/Send State (OnClick)")]
public class UISendStateOnClick : UISend
{
	public int clickState = 1;

	void Start ()
	{
		Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " uses a deprecated script: " + GetType() +
			"\nConsider switching to UIButtonScale, UIButtonColor, UIButtonOffset or UIButtonTween instead.");
	}

	void OnClick ()
	{
		Send(clickState);
	}
}