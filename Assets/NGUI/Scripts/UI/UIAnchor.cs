using UnityEngine;

/// <summary>
/// Attach this script to a game object if you want it to be anchored to the side of the screen,
/// maintaining its position as the resolution and aspect ratio changes.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("UI/Anchor")]
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
	}

	public Camera hudCamera = null;
	public Side side = Side.BottomLeft;
	public Vector3 offset = Vector3.zero;
	public bool stretchToFill = false;

	Transform mTrans;

	void Start ()
	{
		mTrans = transform;

		if (hudCamera == null)
		{
			Transform t = mTrans;

			while (hudCamera == null && t != null)
			{
				hudCamera = t.GetComponent<Camera>();
				t = t.parent;
			}
		}
	}

	void Update ()
	{
		if (hudCamera != null)
		{
			Vector3 v = offset;

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

			// Wrapped in an 'if' so the scene doesnt get marked as 'edited' every frame
			Vector3 newPos = hudCamera.ScreenToWorldPoint(v);
			Vector3 currPos = mTrans.position;
			if ((newPos - currPos).sqrMagnitude > 0.001) mTrans.position = newPos;

			if (stretchToFill && side == Side.TopLeft)
			{
				Vector3 localPos = mTrans.localPosition;
				Vector3 localScale = new Vector3(Mathf.Abs(localPos.x) * 2f, Mathf.Abs(localPos.y) * 2f, 1f);
				if ((mTrans.localScale - localScale).sqrMagnitude > 0.001f) mTrans.localScale = localScale;
			}
		}
	}
}