using UnityEngine;

/// <summary>
/// Allows dragging of the specified target object by mouse or touch, optionally limiting it to be within the UIPanel's clipped rectangle.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Drag Object")]
public class UIDragObject : MonoBehaviour
{
	public enum DragEffect
	{
		None,
		Momentum,
		MomentumAndSpring,
	}

	/// <summary>
	/// Target object that will be dragged.
	/// </summary>

	public Transform target;
	public Vector3 scale = Vector3.one;
	public bool restrictWithinPanel = false;
	public DragEffect dragEffect = DragEffect.MomentumAndSpring;

	Plane mPlane;
	Vector3 mLastPos;
	UIPanel mPanel;
	bool mPressed = false;
	Vector3 mMomentum = Vector3.zero;

	/// <summary>
	/// Create a plane on which we will be performing the dragging.
	/// </summary>

	void OnPress (bool pressed)
	{
		mPressed = pressed;

		if (pressed)
		{
			mMomentum = Vector3.zero;

			if (target != null)
			{
				SpringPosition sp = target.GetComponent<SpringPosition>();
				if (sp != null) sp.enabled = false;
				mLastPos = UICamera.lastHit.point;
				Transform trans = UICamera.lastCamera.transform;
				mPlane = new Plane(trans.rotation * Vector3.back, mLastPos);
			}
		}
		else if (dragEffect == DragEffect.MomentumAndSpring)
		{
			ConstrainToBounds(false);
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

				if (offset.x != 0f || offset.y != 0f)
				{
					offset = target.InverseTransformDirection(offset);
					offset.Scale(scale);
					offset = target.TransformDirection(offset);
				}

				mMomentum = Vector3.Lerp(mMomentum, offset, 0.5f);

				target.position += offset;
				if (dragEffect != DragEffect.MomentumAndSpring && ConstrainToBounds(true)) mMomentum = Vector3.zero;
				mLastPos = currentPos;

				SpringPosition sp = target.GetComponent<SpringPosition>();
				if (sp != null) sp.enabled = false;
			}
		}
	}

	/// <summary>
	/// Calculate the offset needed to be constrained within the panel's bounds.
	/// </summary>

	Vector3 CalculateConstrainOffset ()
	{
		Bounds bounds = NGUITools.CalculateRelativeWidgetBounds(mPanel.transform, target);
		Vector4 range = mPanel.clipRange;

		float offsetX = range.z * 0.5f;
		float offsetY = range.w * 0.5f;

		Vector2 minRect = new Vector2(bounds.min.x, bounds.min.y);
		Vector2 maxRect = new Vector2(bounds.max.x, bounds.max.y);
		Vector2 minArea = new Vector2(range.x - offsetX, range.y - offsetY);
		Vector2 maxArea = new Vector2(range.x + offsetX, range.y + offsetY);

		return NGUITools.ConstrainRect(minRect, maxRect, minArea, maxArea);
	}

	/// <summary>
	/// Constrain the current target position to be within panel bounds.
	/// </summary>

	bool ConstrainToBounds (bool immediate)
	{
		if (mPanel != null && restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None)
		{
			Vector3 offset = CalculateConstrainOffset();

			if (offset.magnitude > 0f)
			{
				if (immediate)
				{
					target.localPosition += offset;
				}
				else
				{
					SpringPosition.Begin(target.gameObject, target.localPosition + offset, 13f);
				}
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Apply the dragging momentum.
	/// </summary>

	void Update ()
	{
		if (dragEffect != DragEffect.None && !mPressed && target != null && mMomentum.magnitude > 0.005f)
		{
			target.position += mMomentum;
			mMomentum = Vector3.Lerp(mMomentum, Vector3.zero, Time.deltaTime * 9f);
			ConstrainToBounds(false);
		}
	}
}