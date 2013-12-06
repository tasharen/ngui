//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script makes it possible to resize the specified widget by dragging on the object this script is attached to.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Drag-Resize Widget")]
public class UIDragResize : MonoBehaviour
{
	/// <summary>
	/// Widget that will be dragged.
	/// </summary>

	public UIWidget target;

	/// <summary>
	/// Widget's pivot that will be dragged
	/// </summary>

	public UIWidget.Pivot pivot = UIWidget.Pivot.BottomRight;

	/// <summary>
	/// Minimum width the widget will be allowed to shrink to when resizing.
	/// </summary>

	public int minWidth = 100;

	/// <summary>
	/// Minimum height the widget will be allowed to shrink to when resizing.
	/// </summary>

	public int minHeight = 100;

	Plane mPlane;
	Vector3 mRayPos;
	Vector3 mLocalPos;
	int mWidth = 0;
	int mHeight = 0;
	bool mDragging = false;

	/// <summary>
	/// Start the dragging operation.
	/// </summary>

	void OnDragStart ()
	{
		if (target != null)
		{
			Vector3[] corners = target.worldCorners;
			mPlane = new Plane(corners[0], corners[1], corners[3]);
			Ray ray = UICamera.currentRay;
			float dist;

			if (mPlane.Raycast(ray, out dist))
			{
				mRayPos = ray.GetPoint(dist);
				mLocalPos = target.cachedTransform.localPosition;
				mWidth = target.width;
				mHeight = target.height;
				mDragging = true;
			}
		}
	}

	/// <summary>
	/// Adjust the widget's dimensions.
	/// </summary>

	void OnDrag (Vector2 delta)
	{
		if (mDragging && target != null)
		{
			float dist;
			Ray ray = UICamera.currentRay;

			if (mPlane.Raycast(ray, out dist))
			{
				NGUIMath.AdjustWidget(target, mLocalPos, mWidth, mHeight,
					ray.GetPoint(dist) - mRayPos, pivot, minWidth, minHeight);
			}
		}
	}

	/// <summary>
	/// End the resize operation.
	/// </summary>

	void OnDragEnd () { mDragging = false; }
}
