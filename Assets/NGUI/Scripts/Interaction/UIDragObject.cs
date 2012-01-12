using UnityEngine;

/// <summary>
/// Allows dragging of the specified target object by mouse or touch, optionally limiting it to be within the UIPanel's clipped rectangle.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Drag Object")]
public class UIDragObject : MonoBehaviour
{
	/// <summary>
	/// Target object that will be dragged.
	/// </summary>

	public Transform target;
	public Vector3 scale = Vector3.one;
	public bool restrictWithinPanel = false;

	Plane mPlane;
	Vector3 mLastPos;
	UIPanel mPanel;

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
		if (restrictWithinPanel && mPanel == null)
		{
			mPanel = (target != null) ? UIPanel.Find(target.transform, false) : null;
			if (mPanel == null) restrictWithinPanel = false;
		}

		if (target != null)
		{
			Ray ray = UICamera.lastCamera.ScreenPointToRay(UICamera.lastTouchPosition);
			float dist = 0f;

			if (mPlane.Raycast(ray, out dist))
			{
				Vector3 currentPos = ray.GetPoint(dist);
				Vector3 offset = currentPos - mLastPos;

				if (offset.x != 1f || offset.y != 1f)
				{
					offset = target.InverseTransformDirection(offset);
					offset.Scale(scale);
					offset = target.TransformDirection(offset);
				}

				target.position += offset;

				if (restrictWithinPanel && mPanel != null && mPanel.clipping != UIDrawCall.Clipping.None)
				{
					Bounds bounds = NGUITools.CalculateRelativeWidgetBounds(mPanel.transform, target);
					Vector4 range = mPanel.clipRange;

					float offsetX = range.z * 0.5f;
					float offsetY = range.w * 0.5f;

					Vector2 minRect = new Vector2(bounds.min.x, bounds.min.y);
					Vector2 maxRect = new Vector2(bounds.max.x, bounds.max.y);
					Vector2 minArea = new Vector2(range.x - offsetX, range.y - offsetY);
					Vector2 maxArea = new Vector2(range.x + offsetX, range.y + offsetY);

					offset = NGUITools.ConstrainRect(minRect, maxRect, minArea, maxArea);

					target.localPosition += offset;
				}
				mLastPos = currentPos;
			}
		}
	}
}