using UnityEngine;

/// <summary>
/// Calls "OnState" function on all of the scripts attached to the specified
/// target when the script receives OnHover message from UICamera.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Send State (OnHover)")]
public class UISendStateOnHover : UISend
{
	public int normalState	= 0;
	public int hoverState	= 1;

	void OnHover (bool isOver)
	{
		Send(isOver ? hoverState : normalState);
	}
}