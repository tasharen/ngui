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
	public EnableCondition ifDisabledOnPlay = EnableCondition.DoNothing;
	public DisableCondition disableWhenFinished = DisableCondition.DoNotDisable;
	public string callWhenFinished;

	void Start () { if (target == null) target = GetComponentInChildren<Animation>(); }

	void OnHover (bool isOver)
	{
		if (enabled && trigger == Trigger.OnHover)
		{
			Activate(isOver);
		}
	}

	void OnPress (bool isPressed)
	{
		if (enabled && trigger == Trigger.OnPress)
		{
			Activate(isPressed);
		}
	}

	void OnClick ()
	{
		if (enabled && trigger == Trigger.OnClick)
		{
			Activate(true);
		}
	}

	/// <summary>
	/// Activate the animation.
	/// </summary>

	void Activate (bool forward)
	{
		if (target != null)
		{
			int pd = -(int)playDirection;
			Direction dir = forward ? playDirection : ((Direction)pd);
			ActiveAnimation anim = ActiveAnimation.Play(target, clipName, dir, ifDisabledOnPlay, disableWhenFinished);
			if (anim != null) anim.callWhenFinished = callWhenFinished;
		}
	}
}