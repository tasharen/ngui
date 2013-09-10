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
		
		if (chk != null)
		{
			UIToggle.current = chk;
			Toggle();
			UIToggle.current = null;
		}
	}

	public void Toggle ()
	{
		if (target != null)
		{
			NGUITools.SetActive(target, inverse ? !UIToggle.current.value : UIToggle.current.value);
			UIPanel panel = NGUITools.FindInParents<UIPanel>(target);
			if (panel != null) panel.Refresh();
		}
	}

	// Legacy functionality, kept for backwards compatibility
	void OnActivate (bool state) { Toggle(); }
}
