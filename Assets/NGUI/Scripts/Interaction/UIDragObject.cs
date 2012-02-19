//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

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
	public float scrollWheelFactor = 0f;
	public bool restrictWithinPanel = false;
	public DragEffect dragEffect = DragEffect.MomentumAndSpring;
	public float momentumAmount = 35f;

	Plane mPlane;
	Vector3 mLastPos;
	UIPanel mPanel;
	bool mPressed = false;
	Vector3 mMomentum = Vector3.zero;
	float mScroll = 0f;
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
				if (restrictWithinPanel) mBounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, target);

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
			else if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None && dragEffect == DragEffect.MomentumAndSpring)
			{
				mPanel.ConstrainTargetToBounds(target, ref mBounds, false);
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
					if (dragEffect != DragEffect.MomentumAndSpring && mPanel.clipping != UIDrawCall.Clipping.None &&
						mPanel.ConstrainTargetToBounds(target, ref mBounds, true))
					{
						mMomentum = Vector3.zero;
					}
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
	/// Apply the dragging momentum.
	/// </summary>

	void LateUpdate ()
	{
		float delta = UpdateRealTimeDelta();
		if (target == null) return;

		if (mPressed)
		{
			// Disable the spring movement
			SpringPosition sp = target.GetComponent<SpringPosition>();
			if (sp != null) sp.enabled = false;
			mScroll = 0f;
		}
		else
		{
			mMomentum += scale * (-mScroll * 0.05f);
			mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);

			if (dragEffect != DragEffect.None && mMomentum.magnitude > 0.0001f)
			{
				// Apply the momentum
				if (mPanel == null) FindPanel();

				if (mPanel != null)
				{
					SpringPosition sp = target.GetComponent<SpringPosition>();
					if (sp != null) sp.enabled = false;

					target.position += NGUIMath.SpringDampen(ref mMomentum, 9f, delta);

					if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None)
					{
						mBounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, target);
						mPanel.ConstrainTargetToBounds(target, ref mBounds, false);
					}
				}
			}
			else mScroll = 0f;
		}
	}

	/// <summary>
	/// If the object should support the scroll wheel, do it.
	/// </summary>

	void OnScroll (float delta)
	{
		if (Mathf.Sign(mScroll) != Mathf.Sign(delta)) mScroll = 0f;
		mScroll += delta * scrollWheelFactor;
	}
}