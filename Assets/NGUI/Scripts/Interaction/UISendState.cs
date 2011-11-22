using UnityEngine;

/// <summary>
/// Calls "OnState" function on all of the scripts attached to the specified
/// target when the script receives OnHover or OnPress messages from UIMouse.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Send State")]
public class UISendState : UISend
{
	public int normalState	= 0;
	public int hoverState	= 1;
	public int pressedState = 2;

	void OnHover (bool isOver)
	{
		Send(isOver ? hoverState : normalState);
	}

	void OnPress (bool pressed)
	{
		Send(pressed ? pressedState : normalState);
	}
}