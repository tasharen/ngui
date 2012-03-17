//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Allows dragging of the camera object and restricts camera's movement to be within bounds of the area created by the rootForBounds colliders.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Drag Camera")]
public class UIDragCamera : IgnoreTimeScale
{
	/// <summary>
	/// Target object that will be dragged.
	/// </summary>

	public Camera target;

	/// <summary>
	/// Root object that will be used for drag-limiting bounds.
	/// </summary>

	public Transform rootForBounds;

	/// <summary>
	/// Scale value applied to the drag delta. Set X or Y to 0 to disallow dragging in that direction.
	/// </summary>

	public Vector2 scale = Vector2.one;

	/// <summary>
	/// Effect the scroll wheel will have on the momentum.
	/// </summary>

	public float scrollWheelFactor = 0f;

	/// <summary>
	/// Effect to apply when dragging.
	/// </summary>

	public UIDragObject.DragEffect dragEffect = UIDragObject.DragEffect.MomentumAndSpring;

	/// <summary>
	/// How much momentum gets applied when the press is released after dragging.
	/// </summary>

	public float momentumAmount = 35f;

	Transform mTrans;
	bool mPressed = false;
	Vector3 mMomentum = Vector3.zero;
	Bounds mBounds;
	float mScroll = 0f;

	/// <summary>
	/// Cache the transform.
	/// </summary>

	void Start ()
	{
		// Before 1.44 'target' was a transform
		if (((Component)target) is Transform) target = target.camera;
		if (target != null) mTrans = target.transform;
		else enabled = false;
	}

	/// <summary>
	/// Calculate the bounds of all widgets under this game object.
	/// </summary>

	void OnPress (bool isPressed)
	{
		if (rootForBounds != null)
		{
			mPressed = isPressed;

			if (isPressed)
			{
				// Update the bounds
				mBounds = NGUIMath.CalculateAbsoluteWidgetBounds(rootForBounds);

				// Remove all momentum on press
				mMomentum = Vector3.zero;
				mScroll = 0f;

				// Disable the spring movement
				SpringPosition sp = target.GetComponent<SpringPosition>();
				if (sp != null) sp.enabled = false;
			}
			else if (dragEffect == UIDragObject.DragEffect.MomentumAndSpring)
			{
				ConstrainToBounds(false);
			}
		}
	}

	/// <summary>
	/// Drag event receiver.
	/// </summary>

	void OnDrag (Vector2 delta)
	{
		if (target != null)
		{
			UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;

			Vector3 offset = Vector3.Scale((Vector3)delta, -scale);
			mTrans.localPosition += offset;

			// Adjust the momentum
			mMomentum = Vector3.Lerp(mMomentum, offset * (realTimeDelta * momentumAmount), 0.5f);

			// Constrain the UI to the bounds, and if done so, eliminate the momentum
			if (dragEffect != UIDragObject.DragEffect.MomentumAndSpring && ConstrainToBounds(true))
			{
				mMomentum = Vector3.zero;
				mScroll = 0f;
			}
		}
	}

	/// <summary>
	/// Calculate the offset needed to be constrained within the panel's bounds.
	/// </summary>

	Vector3 CalculateConstrainOffset ()
	{
		if (target == null || rootForBounds == null) return Vector3.zero;

		Vector3 bottomLeft = new Vector3(target.rect.xMin * Screen.width, target.rect.yMin * Screen.height, 0f);
		Vector3 topRight   = new Vector3(target.rect.xMax * Screen.width, target.rect.yMax * Screen.height, 0f);

		bottomLeft = target.ScreenToWorldPoint(bottomLeft);
		topRight = target.ScreenToWorldPoint(topRight);

		Vector2 minRect = new Vector2(mBounds.min.x, mBounds.min.y);
		Vector2 maxRect = new Vector2(mBounds.max.x, mBounds.max.y);

		return NGUIMath.ConstrainRect(minRect, maxRect, bottomLeft, topRight);
	}

	/// <summary>
	/// Constrain the current camera's position to be within the viewable area's bounds.
	/// </summary>

	public bool ConstrainToBounds (bool immediate)
	{
		if (mTrans != null && rootForBounds != null)
		{
			Vector3 offset = CalculateConstrainOffset();

			if (offset.magnitude > 0f)
			{
				if (immediate)
				{
					mTrans.position -= offset;
				}
				else
				{
					SpringPosition sp = SpringPosition.Begin(target.gameObject, mTrans.position - offset, 13f);
					sp.ignoreTimeScale = true;
					sp.worldSpace = true;
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
		if (target == null) return;
		float delta = UpdateRealTimeDelta();

		if (mPressed)
		{
			// Disable the spring movement
			SpringPosition sp = target.GetComponent<SpringPosition>();
			if (sp != null) sp.enabled = false;
			mScroll = 0f;
		}
		else
		{
			mMomentum += (Vector3)scale * (mScroll * 20f);
			mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);

			if (mMomentum.magnitude > 0.01f)
			{
				// Apply the momentum
				mTrans.localPosition += NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
				mBounds = NGUIMath.CalculateAbsoluteWidgetBounds(rootForBounds);

				if (!ConstrainToBounds(dragEffect == UIDragObject.DragEffect.None))
				{
					SpringPosition sp = target.GetComponent<SpringPosition>();
					if (sp != null) sp.enabled = false;
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
		if (enabled && gameObject.active)
		{
			if (Mathf.Sign(mScroll) != Mathf.Sign(delta)) mScroll = 0f;
			mScroll += delta * scrollWheelFactor;
		}
	}
}