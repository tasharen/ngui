using UnityEngine;

/// <summary>
/// Changes the rotation of the widget based on whether the current state matches the active state.
/// Tip: Use UISetState to change the state of a remote widget that has UIStateRotation attached.
/// </summary>

[AddComponentMenu("UI/State Rotation")]
public class UIStateRotation : MonoBehaviour
{
	public int currentState = 0;
	public int activeState = 0;
	public Vector3 activeRot;
	public Vector3 inactiveRot;
	public float animationSpeed = 10f;

	Transform mTrans;
	Quaternion mTargetRot;

	void Start ()
	{
		mTrans = transform;
		mTargetRot = mTrans.localRotation;
	}

	void OnState (int state)
	{
		currentState = state;
	}

	void Update ()
	{
		mTargetRot = Quaternion.Euler((currentState == activeState) ? activeRot : inactiveRot);
		mTrans.localRotation = Quaternion.Slerp(mTrans.localRotation, mTargetRot,
			Mathf.Clamp01(Time.deltaTime * animationSpeed));
	}
}