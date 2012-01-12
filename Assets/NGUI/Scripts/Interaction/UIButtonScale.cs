using UnityEngine;

/// <summary>
/// Simple example script of how a button can be scaled visibly when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Scale")]
public class UIButtonScale : MonoBehaviour
{
	public Transform target;
	public Vector3 hover = new Vector3(1.1f, 1.1f, 1.1f);
	public Vector3 pressed = new Vector3(1.1f, 1.1f, 1.1f);
	public float duration = 0.2f;

	Vector3 mScale;

	void Start ()
	{
		if (target == null) target = transform;
		mScale = target.localScale;
	}

	void OnPress (bool isPressed)
	{
		TweenScale.Begin(target.gameObject, duration, isPressed ? Vector3.Scale(mScale, pressed) : mScale).method = Tweener.Method.EaseInOut;
	}

	void OnHover (bool isOver)
	{
		TweenScale.Begin(target.gameObject, duration, isOver ? Vector3.Scale(mScale, hover) : mScale).method = Tweener.Method.EaseInOut;
	}
}