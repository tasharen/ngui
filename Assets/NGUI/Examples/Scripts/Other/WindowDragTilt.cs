using UnityEngine;

/// <summary>
/// Attach this script to a child of a draggable window to make it tilt as it's dragged.
/// Look at how it's used in Example 6.
/// </summary>

[AddComponentMenu("NGUI/Examples/Window Drag Tilt")]
public class WindowDragTilt : MonoBehaviour
{
	public int updateOrder = 0;
	public float tiltAmount = 100f;
	public bool smoothen = true;

	Vector3 mLastPos;
	Transform mTrans;

	void Start ()
	{
		UpdateManager.AddCoroutine(this, updateOrder, CoroutineUpdate);
	}

	void OnEnable ()
	{
		mTrans = transform;
		mLastPos = mTrans.position;
	}

	void CoroutineUpdate (float delta)
	{
		Vector3 deltaPos = mTrans.position - mLastPos;
		mLastPos = mTrans.position;
		Quaternion targetRot = Quaternion.Euler(0f, 0f, -deltaPos.x * tiltAmount);
		mTrans.localRotation = smoothen ? Quaternion.Slerp(mTrans.localRotation, targetRot, delta * 10f) : targetRot;
	}
}