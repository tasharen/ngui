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

	public Camera hudCamera = null;
	public Side side = Side.BottomLeft;
	public bool halfPixelOffset = true;
	public bool stretchToFill = false;

	Transform mTrans;
	bool mIsWindows = false;

	/// <summary>
	/// Automatically find the camera responsible for drawing the widgets under this object.
	/// </summary>

	void OnEnable ()
	{
		mTrans = transform;

		mIsWindows = (Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.WindowsWebPlayer ||
			Application.platform == RuntimePlatform.WindowsEditor);

		if (hudCamera == null)
		{
			int layerMask = 1 << gameObject.layer;
			Camera[] cameras = GameObject.FindSceneObjectsOfType(typeof(Camera)) as Camera[];

			foreach (Camera cam in cameras)
			{
				if ((cam.cullingMask & layerMask) != 0)
				{
					hudCamera = cam;
					break;
				}
			}
		}
	}

	/// <summary>
	/// Anchor the object to the appropriate point.
	/// </summary>

	public void Update ()
	{
		if (hudCamera != null)
		{
			Vector3 v = Vector3.zero;

			if (side == Side.Center)
			{
				v.x += Screen.width * hudCamera.rect.width * 0.5f;
				v.y += (Screen.height - v.y) * hudCamera.rect.height * 0.5f;
			}
			else
			{
				if (side == Side.Right || side == Side.TopRight || side == Side.BottomRight)
				{
					v.x += Screen.width * hudCamera.rect.xMax;
				}
				else
				{
					v.x += Screen.width * hudCamera.rect.xMin;
				}

				if (side == Side.Top || side == Side.TopRight || side == Side.TopLeft)
				{
					v.y = (Screen.height - v.y) * hudCamera.rect.yMax;
				}
				else
				{
					v.y = (Screen.height - v.y) * hudCamera.rect.yMin;
				}
			}

			if (halfPixelOffset && mIsWindows && hudCamera.orthographic)
			{
				v.x -= 0.5f;
				v.y += 0.5f;
			}

			Vector3 newPos = hudCamera.ScreenToWorldPoint(v);
			Vector3 currPos = mTrans.position;

			// Wrapped in an 'if' so the scene doesn't get marked as 'edited' every frame
			if (newPos != currPos) mTrans.position = newPos;

			if (stretchToFill && side == Side.TopLeft)
			{
				Vector3 localPos = mTrans.localPosition;
				Vector3 localScale = new Vector3(Mathf.Abs(localPos.x) * 2f, Mathf.Abs(localPos.y) * 2f, 1f);
				if (mTrans.localScale != localScale) mTrans.localScale = localScale;
			}
		}
	}
}