//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script can be used to anchor an object to the side or corner of the screen, panel, or a widget.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Anchor")]
public class UIAnchor : MonoBehaviour
{
	public enum Side
	{
		BottomLeft,
		Left,
		TopLeft,
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		Center,
	}

	bool mIsWindows = false;

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

	/// <summary>
	/// Side or corner to anchor to.
	/// </summary>

	public Side side = Side.Center;

	/// <summary>
	/// Whether a half-pixel offset will be applied on windows machines. Most of the time you'll want to leave this as 'true'.
	/// This value is only used if the widget and panel containers were not specified.
	/// </summary>

	public bool halfPixelOffset = true;

	/// <summary>
	/// Depth offset applied to the anchored widget. Mainly useful for 3D UIs.
	/// </summary>

	public float depthOffset = 0f;

	/// <summary>
	/// Relative offset value, if any. For example "0.25" with 'side' set to Left, means 25% from the left side.
	/// </summary>

	public Vector2 relativeOffset = Vector2.zero;

	Animation mAnim;
	UIRoot mRoot;

	void Awake () { mAnim = animation; }

	/// <summary>
	/// Automatically find the camera responsible for drawing the widgets under this object.
	/// </summary>

	void Start ()
	{
		mRoot = NGUITools.FindInParents<UIRoot>(gameObject);
		mIsWindows = (Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.WindowsWebPlayer ||
			Application.platform == RuntimePlatform.WindowsEditor);

		if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
	}

	/// <summary>
	/// Anchor the object to the appropriate point.
	/// </summary>

	void Update ()
	{
		if (mAnim != null && mAnim.enabled && mAnim.isPlaying) return;

		Rect rect = new Rect();
		bool useCamera = false;

		if (panelContainer != null)
		{
			if (panelContainer.clipping == UIDrawCall.Clipping.None)
			{
				// Panel has no clipping -- just use the screen's dimensions
				float ratio = (mRoot != null) ? mRoot.manualHeight / Screen.height * 0.5f : 0.5f;
				rect.xMin = -Screen.width * ratio;
				rect.yMin = -Screen.height * ratio;
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
			useCamera = true;
			rect = uiCamera.pixelRect;
		}
		else return;

		float cx = (rect.xMin + rect.xMax) * 0.5f;
		float cy = (rect.yMin + rect.yMax) * 0.5f;
		Vector3 v = new Vector3(cx, cy, depthOffset);

		if (side != Side.Center)
		{
			if (side == Side.Right || side == Side.TopRight || side == Side.BottomRight) v.x = rect.xMax;
			else if (side == Side.Top || side == Side.Center || side == Side.Bottom) v.x = cx;
			else v.x = rect.xMin;

			if (side == Side.Top || side == Side.TopRight || side == Side.TopLeft) v.y = rect.yMax;
			else if (side == Side.Left || side == Side.Center || side == Side.Right) v.y = cy;
			else v.y = rect.yMin;
		}

		float width  = rect.width;
		float height = rect.height;

		v.x += relativeOffset.x * width;
		v.y += relativeOffset.y * height;

		if (useCamera)
		{
			if (uiCamera.orthographic)
			{
				v.x = Mathf.RoundToInt(v.x);
				v.y = Mathf.RoundToInt(v.y);

				if (halfPixelOffset && mIsWindows)
				{
					v.x -= 0.5f;
					v.y += 0.5f;
				}
			}

			// Convert from screen to world coordinates, since the two may not match (UIRoot set to manual size)
			v = uiCamera.ScreenToWorldPoint(v);

			// Wrapped in an 'if' so the scene doesn't get marked as 'edited' every frame
			if (transform.position != v) transform.position = v;
		}
		else
		{
			v.x = Mathf.RoundToInt(v.x);
			v.y = Mathf.RoundToInt(v.y);

			float oldZ = v.z;
			if (widgetContainer != null)
			{
				v = widgetContainer.transform.parent.TransformPoint(v);
				
			}
			else if (panelContainer != null)
			{
				v = panelContainer.transform.parent.TransformPoint(v);
			}

			v.z = oldZ;

			// Wrapped in an 'if' so the scene doesn't get marked as 'edited' every frame
			if (transform.position != v) transform.position = v;
		}
	}
}