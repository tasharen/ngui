//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using AnimationOrTween;

/// <summary>
/// Play the specified animation on click.
/// Sends out the "OnAnimationFinished()" notification to the target when the animation finishes.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Play Animation")]
public class UIButtonPlayAnimation : MonoBehaviour
{
	public Animation target;
	public string clipName;
	public Trigger trigger = Trigger.OnClick;
	public Direction playDirection = Direction.Forward;
	public bool resetOnPlay = false;
	public EnableCondition ifDisabledOnPlay = EnableCondition.DoNothing;
	public DisableCondition disableWhenFinished = DisableCondition.DoNotDisable;
	public string callWhenFinished;

	bool mStarted = false;
	bool mHighlighted = false;

	void Start () { mStarted = true; }

	void OnEnable () { if (mStarted && mHighlighted) OnHover(UICamera.IsHighlighted(gameObject)); }

	void OnHover (bool isOver)
	{
		if (enabled)
		{
			if ( trigger == Trigger.OnHover ||
				(trigger == Trigger.OnHoverTrue && isOver) ||
				(trigger == Trigger.OnHoverFalse && !isOver))
			{
				Play(isOver);
			}
			mHighlighted = isOver;
		}
	}

	void OnPress (bool isPressed)
	{
		if (enabled)
		{
			if ( trigger == Trigger.OnPress ||
				(trigger == Trigger.OnPressTrue && isPressed) ||
				(trigger == Trigger.OnPressFalse && !isPressed))
			{
				Play(isPressed);
			}
		}
	}

	void OnClick ()
	{
		if (enabled && trigger == Trigger.OnClick)
		{
			Play(true);
		}
	}

	/// <summary>
	/// Activate the animation.
	/// </summary>

	void Play (bool forward)
	{
		if (target == null) target = GetComponentInChildren<Animation>();

		if (target != null)
		{
			int pd = -(int)playDirection;
			Direction dir = forward ? playDirection : ((Direction)pd);
			ActiveAnimation anim = ActiveAnimation.Play(target, clipName, dir, ifDisabledOnPlay, disableWhenFinished);
			if (resetOnPlay) anim.Reset();
			if (anim != null) anim.callWhenFinished = callWhenFinished;
		}
	}
}