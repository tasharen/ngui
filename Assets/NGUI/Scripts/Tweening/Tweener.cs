using UnityEngine;

/// <summary>
/// Base class for all tweening operations.
/// </summary>

public abstract class Tweener : MonoBehaviour
{
	public enum Method
	{
		Linear,
		EaseIn,
		EaseOut,
		EaseInOut,
	}

	/// <summary>
	/// Tweening method used.
	/// </summary>

	public Method method = Method.Linear;
	public float duration = 1f;
	public bool loop = false;

	float mDuration = 0f;
	float mAmountPerDelta = 1f;
	float mFactor = 0f;

	/// <summary>
	/// Update the tweening factor and call the virtual update function.
	/// </summary>

	void Update ()
	{
		if (mDuration != duration)
		{
			mDuration = duration;
			mAmountPerDelta = Mathf.Abs((duration > 0f) ? 1f / duration : 1000f);
		}

		mFactor += mAmountPerDelta * Time.deltaTime;
		float val = Mathf.Clamp01(mFactor);

		if (method == Method.EaseIn)
		{
			val = 1f - Mathf.Sin(0.5f * Mathf.PI * (1f - val));
		}
		else if (method == Method.EaseOut)
		{
			val = Mathf.Sin(0.5f * Mathf.PI * val);
		}
		else if (method == Method.EaseInOut)
		{
			const float pi2 = Mathf.PI * 2f;
			val = val - Mathf.Sin(val * pi2) / pi2;
		}

		OnUpdate(val);

		if (mFactor > 1f)
		{
			mFactor = 1f;
			if (loop) mAmountPerDelta = -mAmountPerDelta;
			else enabled = false;
		}
		else if (mFactor < 0f)
		{
			mFactor = 0f;
			if (loop) mAmountPerDelta = -mAmountPerDelta;
			else enabled = false;
		}
	}

	/// <summary>
	/// Actual tweening logic should go here.
	/// </summary>

	abstract protected void OnUpdate (float factor);

	/// <summary>
	/// Starts the tweening operation.
	/// </summary>

	static public T Begin<T> (GameObject go, float duration) where T : Tweener
	{
		T comp = go.GetComponent<T>();
		if (comp == null) comp = go.AddComponent<T>();
		comp.duration = duration;
		comp.mFactor = 0f;
		comp.enabled = true;
		comp.loop = false;
		return comp;
	}
}