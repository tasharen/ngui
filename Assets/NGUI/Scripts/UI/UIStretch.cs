//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script can be used to stretch objects relative to the screen's width and height.
/// The most obvious use would be to create a full-screen background by attaching it to a sprite.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Stretch")]
public class UIStretch : MonoBehaviour
{
	public enum Style
	{
		None,
		Horizontal,
		Vertical,
		Both,
		BasedOnHeight,
	}

	/// <summary>
	/// Camera used to determine the anchor bounds. Set automatically if none was specified.
	/// </summary>

	public Camera uiCamera = null;

	/// <summary>
	/// Widget used to determine the container's bounds. Overwrites the camera-based anchoring if the value was specified.
	/// </summary>

	public UIWidget widgetContainer = null;

	/// <summary>
	/// Panel used to determine the container's bounds. Overwrites the widget-based anchoring if the value was specified.
	/// </summary>

	public UIPanel panelContainer = null;
	public Style style = Style.None;
	public Vector2 relativeSize = Vector2.one;

	Transform mTrans;
	UIRoot mRoot;
	Animation mAnim;

	void Awake () { mAnim = animation; }

	void Start ()
	{
		if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
		mRoot = NGUITools.FindInParents<UIRoot>(gameObject);
	}

	void Update ()
	{
		if (mAnim != null && mAnim.isPlaying) return;

		if (style != Style.None)
		{
			if (mTrans == null) mTrans = transform;

			Rect rect = new Rect();

			if (panelContainer != null)
			{
				if (panelContainer.clipping == UIDrawCall.Clipping.None)
				{
					// Panel has no clipping -- just use the screen's dimensions
					rect.xMin = -Screen.width * 0.5f;
					rect.yMin = -Screen.height * 0.5f;
					rect.xMax = -rect.xMin;
					rect.yMax = -rect.yMin;
				}
				else
				{
					// Panel has clipping -- use it as the rect
					Vector4 pos = panelContainer.clipRange;
					rect.x = pos.x - (pos.z * 0.5f);
					rect.y = pos.y - (pos.w * 0.5f);
					rect.width = pos.z;
					rect.height = pos.w;
				}
			}
			else if (widgetContainer != null)
			{
				// Widget is used -- use its bounds as the container's bounds
				Transform t = widgetContainer.cachedTransform;
				Vector3 ls = t.localScale;
				Vector3 lp = t.localPosition;

				Vector3 size = widgetContainer.relativeSize;
				Vector3 offset = widgetContainer.pivotOffset;
				offset.y -= 1f;

				offset.x *= (widgetContainer.relativeSize.x * ls.x);
				offset.y *= (widgetContainer.relativeSize.y * ls.y);

				rect.x = lp.x + offset.x;
				rect.y = lp.y + offset.y;

				rect.width = size.x * ls.x;
				rect.height = size.y * ls.y;
			}
			else if (uiCamera != null)
			{
				rect = uiCamera.pixelRect;
			}
			else return;
			
			
			float rectWidth  = rect.width;
			float rectHeight = rect.height;
			float adj = (mRoot != null) ? mRoot.pixelSizeAdjustment : 1f;

			if (adj != 1f && rectHeight > 1f)
			{
				float scale = mRoot.activeHeight / rectHeight;
				rectWidth *= scale;
				rectHeight *= scale;
			}

			Vector3 localScale = mTrans.localScale;

			if (style == Style.BasedOnHeight)
			{
				localScale.x = relativeSize.x * rectHeight;
				localScale.y = relativeSize.y * rectHeight;
			}
			else
			{
				if (style == Style.Both || style == Style.Horizontal)	localScale.x = relativeSize.x * rectWidth;
				if (style == Style.Both || style == Style.Vertical)		localScale.y = relativeSize.y * rectHeight;
			}

			if (mTrans.localScale != localScale) mTrans.localScale = localScale;
		}
	}
}