using UnityEngine;

/// <summary>
/// Tween the object's local scale.
/// </summary>

[AddComponentMenu("NGUI/Tween/Scale")]
public class TweenScale : NTweener
{
	public Vector3 from;
	public Vector3 to;

	Transform mTrans;

	public Vector3 scale { get { return mTrans.localScale; } set { mTrans.localScale = value; } }

	void Awake () { mTrans = transform; }

	override protected void OnUpdate (float factor) { mTrans.localScale = from * (1f - factor) + to * factor; }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenScale Begin (GameObject go, float duration, Vector3 scale)
	{
		TweenScale comp = NTweener.Begin<TweenScale>(go, duration);
		comp.from = comp.scale;
		comp.to = scale;
		return comp;
	}
}