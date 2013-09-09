//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Example script showing how to activate or deactivate a MonoBehaviour when OnActivate event is received.
/// OnActivate event is sent out by the UIToggle script.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Toggle Controlled Component")]
public class UIToggleControlledComponent : MonoBehaviour
{
	public MonoBehaviour target;
	public bool inverse = false;

	bool mUsingDelegates = false;

	void Start ()
	{
		UIToggle chk = GetComponent<UIToggle>();

		if (chk != null)
		{
			mUsingDelegates = true;
			chk.onStateChange += OnActivateDelegate;
		}
	}

	void OnActivateDelegate (bool isActive)
	{
		if (enabled && target != null) target.enabled = inverse ? !isActive : isActive;
	}

	/// <summary>
	/// Legacy functionality -- keeping it for backwards compatibility.
	/// </summary>

	void OnActivate (bool isActive) { if (!mUsingDelegates) OnActivateDelegate(isActive); }
}
