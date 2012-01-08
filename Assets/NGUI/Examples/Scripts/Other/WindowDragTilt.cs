using UnityEngine;

/// <summary>
/// Attach this script to a child of a draggable window to make it tilt as it's dragged.
/// Look at how it's used in Example 6.
/// </summary>

[AddComponentMenu("NGUI/Examples/Window Drag Tilt")]
public class WindowDragTilt : MonoBehaviour
{
	public float tiltAmount = 100f;
	public bool smoothen = true;

	Vector3 mLastPos;
	Transform mTrans;

	void OnEnable ()
	{
		mTrans = transform;
		mLastPos = mTrans.position;
	}

	void Update ()
	{
		Vector3 delta = mTrans.position - mLastPos;
		mLastPos = mTrans.position;
		Quaternion targetRot = Quaternion.Euler(0f, 0f, -delta.x * tiltAmount);
		mTrans.localRotation = smoothen ? Quaternion.Slerp(mTrans.localRotation, targetRot, Time.deltaTime * 10f) : targetRot;
	}
}