//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's alpha.
/// </summary>

[AddComponentMenu("NGUI/Tween/Alpha")]
public class TweenAlpha : UITweener
{
	public float from = 1f;
	public float to = 1f;

	Transform mTrans;
	UIWidget mWidget;

	/// <summary>
	/// Current alpha.
	/// </summary>

	public float alpha { get { return mWidget.alpha; } set { mWidget.alpha = value; } }

	/// <summary>
	/// Find all needed components.
	/// </summary>

	void Awake () { mWidget = GetComponentInChildren<UIWidget>(); }

	/// <summary>
	/// Interpolate and update the alpha.
	/// </summary>

	override protected void OnUpdate (float factor, bool isFinished) { alpha = Mathf.Lerp(from, to, factor); }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenAlpha Begin (GameObject go, float duration, float alpha)
	{
		TweenAlpha comp = UITweener.Begin<TweenAlpha>(go, duration);
		comp.from = comp.alpha;
		comp.to = alpha;
		return comp;
	}
}