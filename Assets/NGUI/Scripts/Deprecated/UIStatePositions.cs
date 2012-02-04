using UnityEngine;

/// <summary>
/// Changes the position of the widget based on whether the current state matches the active state.
/// DEPRECATED: This script has been deprecated as of version 1.30.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Deprecated/State Positions")]
public class UIStatePositions : MonoBehaviour
{
	public int currentState = 0;
	public float duration = 0.5f;
	public Vector3[] positions;

	Transform mTrans;

	void Start ()
	{
		mTrans = transform;
		Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " uses a deprecated script: " + GetType() +
			"\nConsider switching to TweenPosition instead.", this);
	}

	void OnState (int state)
	{
		if (currentState != state)
		{
			currentState = state;
			if (positions == null || positions.Length == 0) return;
			int index = Mathf.Clamp(currentState, 0, positions.Length - 1);

			TweenPosition tc = NTweener.Begin<TweenPosition>(gameObject, duration);
			tc.method = NTweener.Method.EaseInOut;
			tc.from = mTrans.localPosition;
			tc.to = positions[index];
		}
	}
}