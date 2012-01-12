using UnityEngine;

/// <summary>
/// Simple example script of how a button can be offset visibly when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Offset")]
public class UIButtonOffset : MonoBehaviour
{
	public Transform target;
	public Vector3 hover = Vector3.zero;
	public Vector3 pressed = new Vector3(2f, -2f);
	public float duration = 0.2f;

	Vector3 mPos;

	void Start ()
	{
		if (target == null) target = transform;
		mPos = target.localPosition;
	}

	void OnPress (bool isPressed)
	{
		TweenPosition.Begin(target.gameObject, duration, isPressed ? mPos + pressed : mPos).method = Tweener.Method.EaseInOut;
	}

	void OnHover (bool isOver)
	{
		TweenPosition.Begin(target.gameObject, duration, isOver ? mPos + hover : mPos).method = Tweener.Method.EaseInOut;
	}
}