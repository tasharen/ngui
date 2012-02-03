using UnityEngine;

/// <summary>
/// Implements common functionality for monobehaviours that wish to have a timeScale-independent deltaTime.
/// </summary>

[AddComponentMenu("NGUI/Internal/Ignore TimeScale Behaviour")]
public class IgnoreTimeScale : MonoBehaviour
{
	float mTime = 0f;
	float mDelta = 0f;

	/// <summary>
	/// Equivalent of Time.deltaTime not affected by timeScale, provided that UpdateRealTimeDelta() was called in the Update().
	/// </summary>

	public float realTimeDelta { get { return mDelta; } }

	/// <summary>
	/// Record the current time.
	/// </summary>

	void OnEnable () { mTime = Time.realtimeSinceStartup; }

	/// <summary>
	/// Update the 'realTimeDelta' parameter. Should be called once per frame.
	/// </summary>

	protected float UpdateRealTimeDelta ()
	{
		float time = Time.realtimeSinceStartup;
		mDelta = time - mTime;
		mTime = time;
		return mDelta;
	}
}