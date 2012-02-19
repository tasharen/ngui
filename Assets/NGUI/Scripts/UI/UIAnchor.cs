//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script can be used to anchor an object to the side of the screen,
/// or scale an object to always match the dimensions of the screen.
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

	public Camera uiCamera = null;
	public Side side = Side.Center;
	public bool halfPixelOffset = true;
	public bool stretchToFill = false;
	public float depthOffset = 0f;

	Transform mTrans;
	bool mIsWindows = false;

	/// <summary>
	/// Change the associated widget to be top-left aligned.
	/// </summary>

	void ChangeWidgetPivot ()
	{
		UIWidget widget = GetComponent<UIWidget>();
		if (widget != null) widget.pivot = UIWidget.Pivot.TopLeft;
	}

	/// <summary>
	/// Automatically make the widget top-left aligned if we're stretching to fill.
	/// </summary>

	void Start () { if (stretchToFill) ChangeWidgetPivot(); }

	/// <summary>
	/// Automatically find the camera responsible for drawing the widgets under this object.
	/// </summary>

	void OnEnable ()
	{
		mTrans = transform;

		mIsWindows = (Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.WindowsWebPlayer ||
			Application.platform == RuntimePlatform.WindowsEditor);

		if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
	}

	/// <summary>
	/// Anchor the object to the appropriate point.
	/// </summary>

	public void Update ()
	{
		if (uiCamera != null)
		{
			if (stretchToFill)
			{
				side = Side.TopLeft;
				if (!Application.isPlaying) ChangeWidgetPivot();
			}

			Vector3 v = new Vector3(Screen.width, Screen.height, 0f);

			if (side == Side.Center)
			{
				v.x *= uiCamera.rect.width * 0.5f;
				v.y *= uiCamera.rect.height * 0.5f;
			}
			else
			{
				if (side == Side.Right || side == Side.TopRight || side == Side.BottomRight)
				{
					v.x *= uiCamera.rect.xMax;
				}
				else if (side == Side.Top || side == Side.Center || side == Side.Bottom)
				{
					v.x *= (uiCamera.rect.xMax - uiCamera.rect.xMin) * 0.5f;
				}
				else
				{
					v.x *= uiCamera.rect.xMin;
				}

				if (side == Side.Top || side == Side.TopRight || side == Side.TopLeft)
				{
					v.y *= uiCamera.rect.yMax;
				}
				else if (side == Side.Left || side == Side.Center || side == Side.Right)
				{
					v.y *= (uiCamera.rect.yMax - uiCamera.rect.yMin) * 0.5f;
				}
				else
				{
					v.y *= uiCamera.rect.yMin;
				}
			}

			v.z = (mTrans.TransformPoint(Vector3.forward * depthOffset) -
				mTrans.TransformPoint(Vector3.zero)).magnitude * Mathf.Sign(depthOffset);

			if (uiCamera.orthographic)
			{
				v.z += (uiCamera.nearClipPlane + uiCamera.farClipPlane) * 0.5f;

				if (halfPixelOffset && mIsWindows)
				{
					v.x -= 0.5f;
					v.y += 0.5f;
				}
			}

			Vector3 newPos = uiCamera.ScreenToWorldPoint(v);
			Vector3 currPos = mTrans.position;

			// Wrapped in an 'if' so the scene doesn't get marked as 'edited' every frame
			if (newPos != currPos) mTrans.position = newPos;

			if (stretchToFill)
			{
				Vector3 localPos = mTrans.localPosition;
				Vector3 localScale = new Vector3(Mathf.Abs(localPos.x) * 2f, Mathf.Abs(localPos.y) * 2f, 1f);
				if (mTrans.localScale != localScale) mTrans.localScale = localScale;
			}
		}
	}
}