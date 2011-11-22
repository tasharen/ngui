using UnityEngine;

/// <summary>
/// Changes the position of the widget based on whether the current state matches the active state.
/// Tip: Use UISendState to change the state of a remote object that has UIStatePosition attached.
/// </summary>

[AddComponentMenu("NGUI/Interaction/State Positions")]
public class UIStatePositions : MonoBehaviour
{
	public int currentState = 0;
	public float animationSpeed = 8f;
	public Vector3[] positions;

	Transform mTrans;

	void OnState (int state)
	{
		currentState = state;
	}

	void Start ()
	{
		mTrans = transform;
	}

	void Update ()
	{
		if (positions == null || positions.Length == 0) return;
		int index = Mathf.Clamp(currentState, 0, positions.Length - 1);
		mTrans.localPosition = Vector3.Lerp(mTrans.localPosition, positions[index], Mathf.Clamp01(Time.deltaTime * animationSpeed));
	}
}