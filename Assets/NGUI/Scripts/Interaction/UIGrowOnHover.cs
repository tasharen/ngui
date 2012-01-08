using UnityEngine;

/// <summary>
/// Simply grows the widget by the specified amount when the mouse is hovering over it.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Grow On Hover")]
public class UIGrowOnHover : MonoBehaviour
{
	public float duration = 0.25f;
	public Vector3 amount = new Vector3(1.1f, 1.1f, 1.1f);

	Vector3 mBaseScale;
	Vector3 mTargetScale;

	void Start ()
	{
		mBaseScale = transform.localScale;
		mTargetScale = NGUITools.Multiply(mBaseScale, amount);
	}

	void OnHover (bool isOver)
	{
		TweenScale.Begin(gameObject, duration, isOver ? mTargetScale : mBaseScale).method = Tweener.Method.EaseInOut;
	}
}