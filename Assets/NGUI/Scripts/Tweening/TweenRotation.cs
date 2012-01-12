using UnityEngine;

/// <summary>
/// Tween the object's rotation.
/// </summary>

[AddComponentMenu("NGUI/Tween/Rotation")]
public class TweenRotation : Tweener
{
	public Vector3 from;
	public Vector3 to;

	Transform mTrans;

	public Quaternion rotation { get { return mTrans.localRotation; } set { mTrans.localRotation = value; } }

	void Awake () { mTrans = transform; }

	override protected void OnUpdate (float factor)
	{
		mTrans.localRotation = Quaternion.Slerp(Quaternion.Euler(from), Quaternion.Euler(to), factor);
	}

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenRotation Begin (GameObject go, float duration, Quaternion rot)
	{
		TweenRotation comp = Tweener.Begin<TweenRotation>(go, duration);
		comp.from = comp.rotation.eulerAngles;
		comp.to = rot.eulerAngles;
		return comp;
	}
}