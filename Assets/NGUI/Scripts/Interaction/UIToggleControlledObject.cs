//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Example script showing how to activate or deactivate a game object when OnActivate event is received.
/// OnActivate event is sent out by the UIToggle script.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Toggle Controlled Object")]
public class UIToggleControlledObject : MonoBehaviour
{
	public GameObject target;
	public bool inverse = false;

	void OnEnable ()
	{
		UIToggle chk = GetComponent<UIToggle>();
		if (chk != null) OnActivate(chk.isChecked);
	}

	void OnActivate (bool isActive)
	{
		if (target != null)
		{
			NGUITools.SetActive(target, inverse ? !isActive : isActive);
			UIPanel panel = NGUITools.FindInParents<UIPanel>(target);
			if (panel != null) panel.Refresh();
		}
	}
}
