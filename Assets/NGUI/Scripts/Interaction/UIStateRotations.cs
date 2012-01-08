using UnityEngine;

/// <summary>
/// Changes the rotation of the widget based on whether the current state matches the active state.
/// Tip: Use UISendState to change the state of a remote object that has UIStateRotation attached.
/// </summary>

[AddComponentMenu("NGUI/Interaction/State Rotations")]
public class UIStateRotations : MonoBehaviour
{
	public int currentState = 0;
	public float duration = 0.5f;
	public Vector3[] rotations;

	Transform mTrans;

	void Start ()
	{
		mTrans = transform;
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
			tc.from = mTrans.localRotation;
			tc.to = Quaternion.Euler(rotations[index]);
		}
	}
}