using UnityEngine;

/// <summary>
/// Simple example script of how a button can be offset visibly when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Offset")]
public class UIButtonOffset : MonoBehaviour
{
	public Transform tweenTarget;
	public Vector3 hover = Vector3.zero;
	public Vector3 pressed = new Vector3(2f, -2f);
	public float duration = 0.2f;

	Vector3 mPos;

	void Awake ()
	{
		if (tweenTarget == null) tweenTarget = transform;
		mPos = tweenTarget.localPosition;
	}

	void OnPress (bool isPressed)
	{
		if (enabled) TweenPosition.Begin(tweenTarget.gameObject, duration, isPressed ? mPos + pressed : mPos).method = UITweener.Method.EaseInOut;
	}

	void OnHover (bool isOver)
	{
		if (enabled) TweenPosition.Begin(tweenTarget.gameObject, duration, isOver ? mPos + hover : mPos).method = UITweener.Method.EaseInOut;
	}
}