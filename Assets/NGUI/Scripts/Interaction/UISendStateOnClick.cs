using UnityEngine;

/// <summary>
/// Calls "OnState" function on all of the scripts attached to the specified target (or this game object
/// if none was specified). Use it in conjunction with scripts like UIStateColor, UIStatePosition,
/// and UIStateRotation to set up click-triggered transitions, such as showing/hiding windows, moving
/// game objects or UI components around, changing color, and more.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Send State (OnClick)")]
public class UISendStateOnClick : UISend
{
	public int clickState = 0;

	void OnClick ()
	{
		Send(clickState);
	}
}