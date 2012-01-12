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
	public bool includeChildren = false;

	void Start () { if (tweenTarget == null) tweenTarget = gameObject; }

	void Activate (bool forward)
	{
		GameObject go = (tweenTarget == null) ? gameObject : tweenTarget;
		Tweener[] tween = includeChildren ? go.GetComponentsInChildren<Tweener>() : go.GetComponents<Tweener>();

		if (direction == Direction.Toggle)
		{
			foreach (Tweener tw in tween) tw.Toggle();
		}
		else
		{
			if (direction == Direction.Reverse) forward = !forward;
			foreach (Tweener tw in tween) tw.Activate(forward);
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
}