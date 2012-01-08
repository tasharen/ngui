using UnityEngine;

/// <summary>
/// Changes the position of the widget based on whether the current state matches the active state.
/// Tip: Use UISendState to change the state of a remote object that has UIStatePosition attached.
/// </summary>

[AddComponentMenu("NGUI/Interaction/State Positions")]
public class UIStatePositions : MonoBehaviour
{
	public int currentState = 0;
	public float duration = 0.5f;
	public Vector3[] positions;

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
			if (positions == null || positions.Length == 0) return;
			int index = Mathf.Clamp(currentState, 0, positions.Length - 1);

			TweenPosition tc = Tweener.Begin<TweenPosition>(gameObject, duration);
			tc.method = Tweener.Method.EaseInOut;
			tc.from = mTrans.localPosition;
			tc.to = positions[index];
		}
	}
}