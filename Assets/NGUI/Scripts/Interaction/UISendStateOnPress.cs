using UnityEngine;

/// <summary>
/// Calls "OnState" function on all of the scripts attached to the specified
/// target when the script receives OnPress message from UIMouse.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Send State (OnPress)")]
public class UISendStateOnPress : UISend
{
	public int normalState	= 0;
	public int pressedState = 1;

	void OnPress (bool pressed)
	{
		Send(pressed ? pressedState : normalState);
	}
}