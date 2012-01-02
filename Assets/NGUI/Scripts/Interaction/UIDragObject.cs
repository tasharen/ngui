using UnityEngine;

/// <summary>
/// Allows dragging of the specified target object by mouse or touch.
/// Example usage: attach to the title bar of a window and set the 'target' to be the window itself.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Drag Object")]
public class UIDragObject : MonoBehaviour
{
	/// <summary>
	/// Target object that will be dragged.
	/// </summary>

	public Transform target;

	Plane mPlane;
	Vector3 mLastPos;

	/// <summary>
	/// Create a plane on which we will be performing the dragging.
	/// </summary>

	void OnPress (bool pressed)
	{
		if (pressed && target != null)
		{
			mLastPos = UICamera.lastHit.point;
			Transform trans = UICamera.lastCamera.transform;
			mPlane = new Plane(trans.rotation * Vector3.back, mLastPos);
		}
	}

	/// <summary>
	/// Drag the object along the plane.
	/// </summary>

	void OnDrag (Vector2 delta)
	{
		if (target != null)
		{
			Ray ray = UICamera.lastCamera.ScreenPointToRay(UICamera.lastTouchPosition);
			float dist = 0f;
			
			if (mPlane.Raycast(ray, out dist))
			{
				Vector3 currentPos = ray.GetPoint(dist);
				target.position += currentPos - mLastPos;
				mLastPos = currentPos;
			}
		}
	}
}