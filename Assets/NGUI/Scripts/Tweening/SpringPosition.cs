using UnityEngine;

/// <summary>
/// Spring-like motion -- the farther away the object is from the target, the stronger the pull.
/// </summary>

[AddComponentMenu("NGUI/Tween/Spring Position")]
public class SpringPosition : MonoBehaviour
{
	public Vector3 target = Vector3.zero;
	public float strength = 10f;

	Transform mTrans;
	float mThreshold = 0f;

	void Start () { mTrans = transform; }

	/// <summary>
	/// Advance toward the target position.
	/// </summary>

	void Update ()
	{
		if (mThreshold == 0f) mThreshold = (target - mTrans.localPosition).magnitude * 0.005f;
		mTrans.localPosition = Vector3.Lerp(mTrans.localPosition, target, Time.deltaTime * strength);
		if (mThreshold >= (target - mTrans.localPosition).magnitude) enabled = false;
	}

	/// <summary>
	/// Start the tweening process.
	/// </summary>

	static public SpringPosition Begin (GameObject go, Vector3 pos, float strength)
	{
		SpringPosition sp = go.GetComponent<SpringPosition>();
		if (sp == null) sp = go.AddComponent<SpringPosition>();
		sp.target = pos;
		sp.strength = strength;
		sp.mThreshold = 0f;
		sp.enabled = true;
		return sp;
	}
}