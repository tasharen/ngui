using UnityEngine;
using System.Collections;

/// <summary>
/// Allows dragging of the specified target object by mouse or touch, optionally limiting it to be within the UIPanel's clipped rectangle.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Drag Object")]
public class UIDragObject : IgnoreTimeScale
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
	public float momentumAmount = 35f;

	Plane mPlane;
	Vector3 mLastPos;
	UIPanel mPanel;
	bool mPressed = false;
	Vector3 mMomentum = Vector3.zero;
	Bounds mBounds;

	/// <summary>
	/// Find the panel responsible for this object.
	/// </summary>

	void FindPanel ()
	{
		mPanel = (target != null) ? UIPanel.Find(target.transform, false) : null;
		if (mPanel == null) restrictWithinPanel = false;
	}

	/// <summary>
	/// Create a plane on which we will be performing the dragging.
	/// </summary>

	void OnPress (bool pressed)
	{
		if (target != null)
		{
			mPressed = pressed;

			if (pressed)
			{
				if (restrictWithinPanel && mPanel == null) FindPanel();

				// Calculate the bounds
				if (restrictWithinPanel) mBounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.transform, target);

				// Remove all momentum on press
				mMomentum = Vector3.zero;

				// Disable the spring movement
				SpringPosition sp = target.GetComponent<SpringPosition>();
				if (sp != null) sp.enabled = false;

				// Remember the hit position
				mLastPos = UICamera.lastHit.point;

				// Create the plane to drag along
				Transform trans = UICamera.lastCamera.transform;
				mPlane = new Plane((mPanel != null ? mPanel.cachedTransform.rotation : trans.rotation) * Vector3.back, mLastPos);
			}
			else if (restrictWithinPanel && dragEffect == DragEffect.MomentumAndSpring)
			{
				ConstrainToBounds(false);
			}
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
				Vector3 offset = currentPos - mLastPos;
				mLastPos = currentPos;

				if (offset.x != 0f || offset.y != 0f)
				{
					offset = target.InverseTransformDirection(offset);
					offset.Scale(scale);
					offset = target.TransformDirection(offset);
				}

				// Adjust the momentum
				mMomentum = Vector3.Lerp(mMomentum, offset * (realTimeDelta * momentumAmount), 0.5f);

				// We want to constrain the UI to be within bounds
				if (restrictWithinPanel)
				{
					// Adjust the position and bounds
					Vector3 localPos = target.localPosition;
					target.position += offset;
					mBounds.center = mBounds.center + (target.localPosition - localPos);

					// Constrain the UI to the bounds, and if done so, eliminate the momentum
					if (dragEffect != DragEffect.MomentumAndSpring && ConstrainToBounds(true)) mMomentum = Vector3.zero;
				}
				else
				{
					// Adjust the position
					target.position += offset;
				}
			}
		}
	}

	/// <summary>
	/// Calculate the offset needed to be constrained within the panel's bounds.
	/// </summary>

	Vector3 CalculateConstrainOffset ()
	{
		Vector4 range = mPanel.clipRange;

		float offsetX = range.z * 0.5f;
		float offsetY = range.w * 0.5f;

		Vector2 minRect = new Vector2(mBounds.min.x, mBounds.min.y);
		Vector2 maxRect = new Vector2(mBounds.max.x, mBounds.max.y);
		Vector2 minArea = new Vector2(range.x - offsetX, range.y - offsetY);
		Vector2 maxArea = new Vector2(range.x + offsetX, range.y + offsetY);

		return NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea);
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
					mBounds.center += offset;
				}
				else
				{
					SpringPosition sp = SpringPosition.Begin(target.gameObject, target.localPosition + offset, 13f);
					sp.ignoreTimeScale = true;
					sp.worldSpace = false;
				}
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Apply the dragging momentum.
	/// </summary>

	void LateUpdate ()
	{
		float delta = UpdateRealTimeDelta();

		if (mPressed)
		{
			// Disable the spring movement
			SpringPosition sp = target.GetComponent<SpringPosition>();
			if (sp != null) sp.enabled = false;
		}
		else if (dragEffect != DragEffect.None && target != null && mMomentum.magnitude > 0.005f)
		{
			// Apply the momentum
			if (mPanel == null) FindPanel();

			if (mPanel != null)
			{
				target.position += NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
				mBounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.transform, target);
				ConstrainToBounds(false);
			}
		}
	}
}