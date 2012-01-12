using UnityEngine;

/// <summary>
/// Changes the rotation of the widget based on whether the current state matches the active state.
/// DEPRECATED: This script has been deprecated as of version 1.30.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Deprecated/State Rotations")]
public class UIStateRotations : MonoBehaviour
{
	public int currentState = 0;
	public float duration = 0.5f;
	public Vector3[] rotations;

	Transform mTrans;

	void Start ()
	{
		mTrans = transform;
		Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " uses a deprecated script: " + GetType() +
			"\nConsider switching to TweenRotation instead.");
	}

	void OnState (int state)
	{
		if (currentState != state)
		{
			currentState = state;
			if (rotations == null || rotations.Length == 0) return;
			int index = Mathf.Clamp(currentState, 0, rotations.Length - 1);

			TweenRotation tc = Tweener.Begin<TweenRotation>(gameObject, duration);
			tc.method = Tweener.Method.EaseInOut;
			tc.from = mTrans.localRotation.eulerAngles;
			tc.to = rotations[index];
		}
	}
}