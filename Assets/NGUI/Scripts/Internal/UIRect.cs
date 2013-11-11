using UnityEngine;

[ExecuteInEditMode]
public class UIRect : MonoBehaviour
{
	/// <summary>
	/// List of all the rectangles present in the scene.
	/// </summary>

	//static public BetterList<UIRect> list = new BetterList<UIRect>();

	//[HideInInspector]
	[SerializeField]
	Vector2 mLeft = Vector2.zero;
	//[HideInInspector]
	[SerializeField]
	Vector2 mBottom = Vector2.zero;
	//[HideInInspector]
	[SerializeField]
	Vector2 mRight = new Vector2(1f, 0f);
	//[HideInInspector]
	[SerializeField]
	Vector2 mTop = new Vector2(1f, 0f);

	BetterList<Transform> mParentTrans = new BetterList<Transform>();
	UIRect mParentRect;
	bool mHasParent = false;
	int mLastFrame = 0;
	float mFinalLeft = 0f;
	float mFinalBottom = 0f;
	float mFinalRight = 1f;
	float mFinalTop = 1f;
	bool mHasChanged = false;

	// Screen.width and height return the dimensions of the window that's currently being drawn, which is not very useful.
	// In OnValidate() this means inspector window's dimensions, and in OnDrawGizmos() -- scene view. We want game view.
#if UNITY_EDITOR
	float mScreenWidth = 1280f;
	float mScreenHeight = 720f;
#endif

	public float finalLeft		{ get { if (mHasChanged || mLastFrame != Time.frameCount) UpdateRect(); return mFinalLeft; } }
	public float finalBottom	{ get { if (mHasChanged || mLastFrame != Time.frameCount) UpdateRect(); return mFinalBottom; } }
	public float finalRight		{ get { if (mHasChanged || mLastFrame != Time.frameCount) UpdateRect(); return mFinalRight; } }
	public float finalTop		{ get { if (mHasChanged || mLastFrame != Time.frameCount) UpdateRect(); return mFinalTop; } }

	void OnValidate () { mHasChanged = true; }

	void Start () { FindParentRect(); }

	/// <summary>
	/// Find the parent's rectangle, if there is one.
	/// </summary>

	void FindParentRect ()
	{
		mParentRect = null;
		mParentTrans.Clear();
		Transform t = transform.parent;

		while (t != null && mParentRect == null)
		{
			mParentTrans.Add(t);
			mParentRect = t.GetComponent<UIRect>();
			t = t.parent;
		}
		mHasParent = (mParentRect != null);
		mHasChanged = true;
	}

	/// <summary>
	/// Update the rectangle if necessary.
	/// </summary>

	void Update ()
	{
#if UNITY_EDITOR
		mScreenWidth = Screen.width;
		mScreenHeight = Screen.height;
#endif
		if (mHasChanged || mLastFrame != Time.frameCount)
			UpdateRect();
	}

	/// <summary>
	/// Update the final absolute values.
	/// </summary>

	void UpdateRect ()
	{
		CheckParent();

		if (mHasChanged)
		{
			if (mParentRect != null)
			{
				Update(mParentRect.finalLeft, mParentRect.mFinalBottom, mParentRect.mFinalRight, mParentRect.mFinalTop);
			}
			else
			{
#if UNITY_EDITOR
				float x = 0.5f * mScreenWidth;
				float y = 0.5f * mScreenHeight;
#else
				float x = 0.5f * Screen.width;
				float y = 0.5f * Screen.height;
#endif
				Update(-x, -y, x, y);
			}
		}
	}

	/// <summary>
	/// Check to see if the parent has changed. If it has, find a new one.
	/// This function could be eliminated if the C# side would get the "transform hierarchy changed" notifications from C++.
	/// </summary>

	void CheckParent ()
	{
		if (mHasParent && (mParentTrans.size == 0 || !mParentRect))
		{
			FindParentRect();
		}
		else
		{
			Transform t = transform.parent;

			for (int i = 0; i < mParentTrans.size; ++i)
			{
				if (mParentTrans[i] != t)
				{
					FindParentRect();
					break;
				}
			}
		}
	}

	/// <summary>
	/// Recalculate the final absolute values given the parent's absolute values.
	/// </summary>

	void Update (float left, float bottom, float right, float top)
	{
		mHasChanged		= false;
		mLastFrame		= Time.frameCount;
		mFinalLeft		= Mathf.Round(Mathf.Lerp(left, right, mLeft.x) + mLeft.y);
		mFinalRight		= Mathf.Round(Mathf.Lerp(left, right, mRight.x) + mRight.y);
		mFinalBottom	= Mathf.Round(Mathf.Lerp(bottom, top, mBottom.x) + mBottom.y);
		mFinalTop		= Mathf.Round(Mathf.Lerp(bottom, top, mTop.x) + mTop.y);
	}

	public Vector3[] localCorners
	{
		get
		{
			if (mHasChanged || mLastFrame != Time.frameCount) UpdateRect();

			Vector3 v = Vector3.zero;// transform.localPosition;
			Vector3[] corners = new Vector3[4];

			corners[0] = new Vector3(mFinalLeft - v.x, mFinalBottom - v.y);
			corners[1] = new Vector3(mFinalLeft - v.x, mFinalTop - v.y);
			corners[2] = new Vector3(mFinalRight - v.x, mFinalTop - v.y);
			corners[3] = new Vector3(mFinalRight - v.x, mFinalBottom - v.y);

			return corners;
		}
	}

	void Lerp (out Vector3 v, ref Vector3 bl, ref Vector3 tr, ref Vector2 hr, ref Vector2 vt, ref Vector3 right, ref Vector3 up)
	{
		v = new Vector3(
			Mathf.Lerp(bl.x, tr.x, hr.x) + up.x * hr.y + up.y * vt.x,
			Mathf.Lerp(bl.y, tr.y, vt.x) + up.y * vt.y + up.x * hr.x);
	}

	public Vector3[] worldCorners
	{
		get
		{
			if (mHasChanged || mLastFrame != Time.frameCount) UpdateRect();
			Transform t = transform;
			Vector3[] corners = localCorners;

			// TODO:
			// - Scrap this shit. Start over.
			// Consider simply keeping the existing widget functionality, but integrating anchors and stretch scripts into UIWidget instead.
			// To ensure that anchors update in the correct order, use UIRect's hierarchical updating logic in UIWidget.

			for (int i = 0; i < 4; ++i) corners[i] = t.TransformDirection(corners[i]);
			return corners;
		}
	}

	void OnDrawGizmos ()
	{
		//Vector3[] corners = localCorners;

		//Vector3 size = corners[2] - corners[0];
		//Vector3 center = (corners[2] + corners[0]) * 0.5f;

		//Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.green;
		//Gizmos.DrawWireCube(center, size);
		
		//Gizmos.color = Color.clear;
		//Gizmos.DrawCube(center, size);
		Vector3[] v = worldCorners;
		Gizmos.DrawLine(v[0], v[1]);
		Gizmos.DrawLine(v[0], v[3]);
		Gizmos.DrawLine(v[1], v[2]);
		Gizmos.DrawLine(v[2], v[3]);
	}
}
