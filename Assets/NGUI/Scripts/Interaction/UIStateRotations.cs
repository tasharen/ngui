using UnityEngine;

/// <summary>
/// Changes the rotation of the widget based on whether the current state matches the active state.
/// Tip: Use UISendState to change the state of a remote object that has UIStateRotation attached.
/// </summary>

[AddComponentMenu("NGUI/Interaction/State Rotations")]
public class UIStateRotations : MonoBehaviour
{
	public int currentState = 0;
	public float animationSpeed = 8f;
	public Vector3[] rotations;

	Transform mTrans;
	Quaternion mTargetRot;

	void OnState (int state)
	{
		currentState = state;
	}

	void Start ()
	{
		mTrans = transform;
		mTargetRot = mTrans.localRotation;
	}

	void Update ()
	{
		if (rotations == null || rotations.Length == 0) return;
		int index = Mathf.Clamp(currentState, 0, rotations.Length - 1);
		mTargetRot = Quaternion.Euler(rotations[index]);
		mTrans.localRotation = Quaternion.Slerp(mTrans.localRotation, mTargetRot,
			Mathf.Clamp01(Time.deltaTime * animationSpeed));
	}
}