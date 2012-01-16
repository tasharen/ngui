using UnityEngine;
using AnimationOrTween;

/// <summary>
/// Attaching this to an object lets you activate tweener components on other objects.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Tween")]
public class UIButtonTween : MonoBehaviour
{
	public GameObject tweenTarget;
	public int tweenGroup = 0;
	public Trigger trigger = Trigger.OnClick;
	public Direction playDirection = Direction.Forward;
	public EnableCondition ifDisabledOnPlay = EnableCondition.DoNothing;
	public DisableCondition disableWhenFinished = DisableCondition.DoNotDisable;
	public bool includeChildren = false;

	Tweener[] mTweens;

	void Start () { if (tweenTarget == null) tweenTarget = gameObject; }

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

	void Update ()
	{
		if ((int)playDirection == (int)disableWhenFinished && mTweens != null)
		{
			bool isFinished = true;

			foreach (Tweener tw in mTweens)
			{
				if (tw.enabled)
				{
					isFinished = false;
					break;
				}
			}

			if (isFinished)
			{
				tweenTarget.SetActiveRecursively(false);
				mTweens = null;
			}
		}
	}

	/// <summary>
	/// Activate the tweeners.
	/// </summary>

	void Activate (bool forward)
	{
		GameObject go = (tweenTarget == null) ? gameObject : tweenTarget;

		// If the object is disabled, don't do anything
		if (!go.active && ifDisabledOnPlay != EnableCondition.EnableThenPlay) return;

		// Gather the tweening components
		mTweens = includeChildren ? go.GetComponentsInChildren<Tweener>() : go.GetComponents<Tweener>();

		if (mTweens.Length == 0)
		{
			// No tweeners found -- should we disable the object?
			if (disableWhenFinished != DisableCondition.DoNotDisable) tweenTarget.SetActiveRecursively(false);
		}
		else
		{
			bool activated = false;
			if (playDirection == Direction.Reverse) forward = !forward;

			// Run through all located tween components
			foreach (Tweener tw in mTweens)
			{
				// If the tweener's group matches, we can work with it
				if (tw.tweenGroup == tweenGroup)
				{
					// Ensure that the game objects are enabled
					if (!activated && mTweens.Length > 0 && !go.active)
					{
						activated = true;
						go.SetActiveRecursively(true);
					}

					// Toggle or activate the tween component
					if (playDirection == Direction.Toggle) tw.Toggle();
					else tw.Activate(forward);
				}
			}
		}
	}
}