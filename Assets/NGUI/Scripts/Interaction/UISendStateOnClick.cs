using UnityEngine;

/// <summary>
/// Calls "OnState" function on all of the scripts attached to the specified
/// target when the script receives OnClick message from UIMouse.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Send State (OnClick)")]
public class UISendStateOnClick : UISend
{
	public int clickState = 1;

	void OnClick ()
	{
		Send(clickState);
	}
}