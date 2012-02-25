using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attaching this script to a widget makes it react to key events such as tab, up, down, etc.
/// </summary>

[RequireComponent(typeof(Collider))]
[AddComponentMenu("NGUI/Interaction/Button Keys")]
public class UIButtonKeys : MonoBehaviour
{
	public bool startsSelected = false;
	public UIButtonKeys selectOnClick;
	public UIButtonKeys selectOnUp;
	public UIButtonKeys selectOnDown;
	public UIButtonKeys selectOnLeft;
	public UIButtonKeys selectOnRight;

	void Start ()
	{
		if (startsSelected && (UICamera.selectedObject == null || !UICamera.selectedObject.active))
		{
			UICamera.selectedObject = gameObject;
		}
	}

	void OnKey (KeyCode key)
	{
		switch (key)
		{
		case KeyCode.LeftArrow:
			if (selectOnLeft != null) UICamera.selectedObject = selectOnLeft.gameObject;
			break;
		case KeyCode.RightArrow:
			if (selectOnRight != null) UICamera.selectedObject = selectOnRight.gameObject;
			break;
		case KeyCode.UpArrow:
			if (selectOnUp != null) UICamera.selectedObject = selectOnUp.gameObject;
			break;
		case KeyCode.DownArrow:
			if (selectOnDown != null) UICamera.selectedObject = selectOnDown.gameObject;
			break;
		case KeyCode.Tab:
			if (selectOnRight != null) UICamera.selectedObject = selectOnRight.gameObject;
			else if (selectOnDown != null) UICamera.selectedObject = selectOnDown.gameObject;
			break;
		}
	}

	void OnClick ()
	{
		if (selectOnClick != null)
		{
			UICamera.selectedObject = selectOnClick.gameObject;
		}
	}
}