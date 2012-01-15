using UnityEngine;

/// <summary>
/// Attaching this to an object lets you activate tweener components on other objects.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Tween")]
public class UIButtonTween : MonoBehaviour
{
	public enum Trigger
	{
		OnClick,
		OnHover,
		OnPress,
	}

	public enum Direction
	{
		Forward,
		Reverse,
		Toggle,
	}

	public GameObject tweenTarget;
	public Trigger trigger = Trigger.OnClick;
	public Direction direction = Direction.Forward;
	public bool enableIfDisabled = false;
	public bool disableWhenDone = false;
	public bool includeChildren = false;

	Tweener[] mTweens;

	void Start () { if (tweenTarget == null) tweenTarget = gameObject; }

	void Activate (bool forward)
	{
		GameObject go = (tweenTarget == null) ? gameObject : tweenTarget;

		if (!go.active)
		{
			if (!enableIfDisabled) return;
			go.SetActiveRecursively(true);
		}

		mTweens = includeChildren ? go.GetComponentsInChildren<Tweener>() : go.GetComponents<Tweener>();

		if (mTweens.Length == 0)
		{
			if (disableWhenDone) tweenTarget.SetActiveRecursively(false);
		}
		else if (direction == Direction.Toggle)
		{
			foreach (Tweener tw in mTweens) tw.Toggle();
		}
		else
		{
			if (direction == Direction.Reverse) forward = !forward;
			foreach (Tweener tw in mTweens) tw.Activate(forward);
		}
	}

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
		if (disableWhenDone && mTweens != null)
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
}