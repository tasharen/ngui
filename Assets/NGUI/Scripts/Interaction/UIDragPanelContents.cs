//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections;

/// <summary>
/// Allows dragging of the specified target panel's contents by mouse or touch.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Drag Panel Contents")]
public class UIDragPanelContents : IgnoreTimeScale
{
	public enum DragEffect
	{
		None,
		Momentum,
		MomentumAndSpring,
	}

	/// <summary>
	/// This panel's contents will be dragged by the script.
	/// </summary>

	public UIPanel panel;

	/// <summary>
	/// Scale value applied to the drag delta. Set X or Y to 0 to disallow dragging in that direction.
	/// </summary>

	public Vector3 scale = Vector3.one;

	/// <summary>
	/// Effect the scroll wheel will have on the momentum.
	/// </summary>

	public float scrollWheelFactor = 0f;

	/// <summary>
	/// Whether the dragging will be restricted to be within the parent panel's bounds.
	/// </summary>

	public bool restrictWithinPanel = false;

	/// <summary>
	/// Whether dragging will be disabled if the contents fit.
	/// </summary>

	public bool disableDragIfFits = false;

	/// <summary>
	/// Effect to apply when dragging.
	/// </summary>

	public DragEffect dragEffect = DragEffect.MomentumAndSpring;

	/// <summary>
	/// How much momentum gets applied when the press is released after dragging.
	/// </summary>

	public float momentumAmount = 35f;

	Plane mPlane;
	Vector3 mLastPos;
	bool mPressed = false;
	Vector3 mMomentum = Vector3.zero;
	float mScroll = 0f;
	Bounds mBounds;
	bool mCalculatedBounds = false;
	bool mShouldMove = false;

	/// <summary>
	/// Calculate the bounds used by the widgets.
	/// </summary>

	Bounds bounds
	{
		get
		{
			if (!mCalculatedBounds && panel != null)
			{
				mCalculatedBounds = true;
				Transform t = panel.cachedTransform;
				mBounds = NGUIMath.CalculateRelativeWidgetBounds(t, t);
			}
			return mBounds;
		}
	}

	/// <summary>
	/// Whether the contents of the panel should actually be draggable depends on whether they fit or not.
	/// </summary>

	bool shouldMove
	{
		get
		{
			if (!disableDragIfFits) return true;
			if (panel == null) panel = NGUITools.FindInParents<UIPanel>(gameObject);
			if (panel == null) return false;

			Vector4 clip = panel.clipRange;
			Bounds b = bounds;

			float hx = clip.z * 0.5f;
			float hy = clip.w * 0.5f;

			if (!Mathf.Approximately(scale.x, 0f))
			{
				if (b.min.x < clip.x - hx) return true;
				if (b.max.x > clip.x + hx) return true;
			}

			if (!Mathf.Approximately(scale.y, 0f))
			{
				if (b.min.y < clip.y - hy) return true;
				if (b.max.y > clip.y + hy) return true;
			}
			return false;
		}
	}

	/// <summary>
	/// Create a plane on which we will be performing the dragging.
	/// </summary>

	void OnPress (bool pressed)
	{
		if (enabled && gameObject.active && panel != null)
		{
			mCalculatedBounds = false;
			mShouldMove = shouldMove;
			if (!mShouldMove) return;
			mPressed = pressed;

			if (pressed)
			{
				// Remove all momentum on press
				mMomentum = Vector3.zero;
				mScroll = 0f;

				// Disable the spring movement
				DisableSpring();

				// Remember the hit position
				mLastPos = UICamera.lastHit.point;

				// Create the plane to drag along
				Transform trans = UICamera.currentCamera.transform;
				mPlane = new Plane((panel != null ? panel.cachedTransform.rotation : trans.rotation) * Vector3.back, mLastPos);
			}
			else if (restrictWithinPanel && panel.clipping != UIDrawCall.Clipping.None && dragEffect == DragEffect.MomentumAndSpring)
			{
				RestrictWithinBounds();
			}
		}
	}

	/// <summary>
	/// Drag the object along the plane.
	/// </summary>

	void OnDrag (Vector2 delta)
	{
		if (enabled && gameObject.active && panel != null && mShouldMove)
		{
			UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;

			Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);
			float dist = 0f;

			if (mPlane.Raycast(ray, out dist))
			{
				Vector3 currentPos = ray.GetPoint(dist);
				Vector3 offset = currentPos - mLastPos;
				mLastPos = currentPos;

				if (offset.x != 0f || offset.y != 0f)
				{
					Transform t = panel.cachedTransform;
					offset = t.InverseTransformDirection(offset);
					offset.Scale(scale);
					offset = t.TransformDirection(offset);
				}

				// Adjust the momentum
				mMomentum = Vector3.Lerp(mMomentum, offset * (realTimeDelta * momentumAmount), 0.5f);

				// Move the panel
				MoveAbsolute(offset);

				// We want to constrain the UI to be within bounds
				if (restrictWithinPanel &&
					panel.clipping != UIDrawCall.Clipping.None &&
					dragEffect != DragEffect.MomentumAndSpring)
				{
					RestrictWithinBounds();
				}
			}
		}
	}

	/// <summary>
	/// Restrict the panel's contents to be within the panel's bounds.
	/// </summary>

	void RestrictWithinBounds ()
	{
		Vector3 constraint = panel.CalculateConstrainOffset(bounds.min, bounds.max);

		if (constraint.magnitude > 0.001f)
		{
			if (dragEffect == DragEffect.MomentumAndSpring)
			{
				// Spring back into place
				SpringPanel.Begin(panel.gameObject, panel.cachedTransform.localPosition + constraint, 13f);
			}
			else
			{
				// Jump back into place
				MoveRelative(constraint);
				mMomentum = Vector3.zero;
				mScroll = 0f;
			}
		}
		else
		{
			// Remove the spring as it's no longer needed
			DisableSpring();
		}
	}

	/// <summary>
	/// Disable the spring movement.
	/// </summary>

	void DisableSpring ()
	{
		SpringPanel sp = panel.GetComponent<SpringPanel>();
		if (sp != null) sp.enabled = false;
	}

	/// <summary>
	/// Move the panel by the specified amount.
	/// </summary>

	void MoveRelative (Vector3 relative)
	{
		panel.cachedTransform.localPosition += relative;
		Vector4 cr = panel.clipRange;
		cr.x -= relative.x;
		cr.y -= relative.y;
		panel.clipRange = cr;
	}

	/// <summary>
	/// Move the panel by the specified amount.
	/// </summary>

	void MoveAbsolute (Vector3 absolute)
	{
		Transform t = panel.cachedTransform;
		Vector3 a = t.InverseTransformPoint(absolute);
		Vector3 b = t.InverseTransformPoint(Vector3.zero);
		MoveRelative(a - b);
	}

	/// <summary>
	/// Apply the dragging momentum.
	/// </summary>

	void LateUpdate ()
	{
		float delta = UpdateRealTimeDelta();
		if (panel == null) return;

		if (mShouldMove && !mPressed)
		{
			mMomentum += scale * (-mScroll * 0.05f);

			if (mMomentum.magnitude > 0.0001f)
			{
				mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);

				// Move the panel
				MoveAbsolute(NGUIMath.SpringDampen(ref mMomentum, 9f, delta));

				// Restrict the contents to be within the panel's bounds
				if (restrictWithinPanel && panel.clipping != UIDrawCall.Clipping.None) RestrictWithinBounds();
			}
			else mScroll = 0f;
		}
	}

	/// <summary>
	/// If the object should support the scroll wheel, do it.
	/// </summary>

	void OnScroll (float delta)
	{
		mShouldMove = shouldMove;

		if (enabled && gameObject.active)
		{
			if (Mathf.Sign(mScroll) != Mathf.Sign(delta)) mScroll = 0f;
			mScroll += delta * scrollWheelFactor;
		}
	}
}