using UnityEngine;

/// <summary>
/// Changes the position of the widget based on whether the current state matches the active state.
/// Tip: Use UISetState to change the state of a remote widget that has UIStatePosition attached.
/// </summary>

[AddComponentMenu("UI/State Position")]
public class UIStatePosition : MonoBehaviour
{
	public int currentState = 0;
	public int activeState = 0;
	public Vector3 activePos;
	public Vector3 inactivePos;
	public float animationSpeed = 10f;

	Transform mTrans;

	void Start ()
	{
		mTrans = transform;
	}

	void OnState (int state)
	{
		currentState = state;
	}

	void Update ()
	{
		Vector3 v = (currentState == activeState) ? activePos : inactivePos;
		mTrans.localPosition = Vector3.Lerp(mTrans.localPosition, v, Mathf.Clamp01(Time.deltaTime * animationSpeed));
	}
}