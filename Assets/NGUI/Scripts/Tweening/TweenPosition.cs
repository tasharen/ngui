using UnityEngine;

/// <summary>
/// Tween the object's position.
/// </summary>

[AddComponentMenu("NGUI/Tween/Position")]
public class TweenPosition : NTweener
{
	public Vector3 from;
	public Vector3 to;

	Transform mTrans;

	public Vector3 position { get { return mTrans.localPosition; } set { mTrans.localPosition = value; } }

	void Awake () { mTrans = transform; }

	override protected void OnUpdate (float factor) { mTrans.localPosition = from * (1f - factor) + to * factor; }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenPosition Begin (GameObject go, float duration, Vector3 pos)
	{
		TweenPosition comp = NTweener.Begin<TweenPosition>(go, duration);
		comp.from = comp.position;
		comp.to = pos;
		return comp;
	}
}