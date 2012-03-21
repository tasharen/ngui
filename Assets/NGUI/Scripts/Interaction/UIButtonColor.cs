//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Simple example script of how a button can be colored when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Color")]
public class UIButtonColor : MonoBehaviour
{
	public GameObject tweenTarget;
	public Color hover = new Color(0.6f, 1f, 0.2f, 1f);
	public Color pressed = Color.grey;
	public float duration = 0.2f;

	Color mColor;
	bool mInitDone = false;

	void OnDisable ()
	{
		if (tweenTarget != null)
		{
			TweenColor tc = tweenTarget.GetComponent<TweenColor>();

			if (tc != null)
			{
				tc.color = mColor;
				tc.enabled = false;
			}
		}
	}

	void Init ()
	{
		mInitDone = true;
		if (tweenTarget == null) tweenTarget = gameObject;
		UIWidget widget = tweenTarget.GetComponent<UIWidget>();

		if (widget != null)
		{
			mColor = widget.color;
		}
		else
		{
			Renderer ren = tweenTarget.renderer;

			if (ren != null)
			{
				mColor = ren.material.color;
			}
			else
			{
				Light lt = tweenTarget.light;

				if (lt != null)
				{
					mColor = lt.color;
				}
				else
				{
					Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " has nothing for UIButtonColor to color", this);
					enabled = false;
				}
			}
		}
	}

	void OnPress (bool isPressed)
	{
		if (!mInitDone) Init();
		if (enabled) TweenColor.Begin(tweenTarget, duration, isPressed ? pressed : mColor);
	}

	void OnHover (bool isOver)
	{
		if (!mInitDone) Init();
		if (enabled) TweenColor.Begin(tweenTarget, duration, isOver ? hover : mColor);
	}
}