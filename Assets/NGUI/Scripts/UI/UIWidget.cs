using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for all UI components that should be derived from when creating new widget types.
/// </summary>

public abstract class UIWidget : MonoBehaviour
{
	// Widget to copy sprite coordinates from -- reset after copying
	public UIWidget copyFrom = null;

	// When set to 'true', the size will be set automatically from the widget's scale
	public bool sizeMatchesScale = false;

	// Material should ideally be the same for all UI widgets -- generally this will be the UI texture atlas
	public Material material = null;

	// Color tint applied to this widget
	public Color color = Color.white;

	// Whether the depth is calculated automatically
	public bool autoDepth = true;

	// Depth controls the rendering order -- lowest to highest
	public int depth = 0;

	// Groups can be used to break up the rendering into separate batches
	public int group = 0;

	// Cached for efficiency, used by UIScreen
	[HideInInspector] public Transform mTrans;

	// Cached values, used to check if anything has changed
	Material mMat;
	Color mColor;
	UIScreen mScreen;
	Vector3 mPos;
	Quaternion mRot;
	Vector3 mScale;
	bool mAutoDepth = true;
	int mLayer = 0;
	int mGroup = 0;
	int mDepth = 0;

	/// <summary>
	/// Static widget comparison function used for Z-sorting.
	/// </summary>

	static public int CompareFunc (UIWidget left, UIWidget right)
	{
		if (left.mDepth > right.mDepth) return 1;
		if (left.mDepth < right.mDepth) return -1;
		return 0;
	}

	/// <summary>
	/// Helper function that automatically calculates depth of this widget based on the hierarchy.
	/// </summary>

	public int CalculateDepth ()
	{
		Transform t = mTrans;
		int val = 0;

		for (;;)
		{
			t = t.parent;
			if (t == null) break;
			++val;
		}
		return val;
	}

	/// <summary>
	/// Register this widget with the manager.
	/// </summary>

	void Start ()
	{
		if (mScreen == null && material != null)
		{
			mColor		= color;
			mTrans		= transform;
			mPos		= mTrans.position;
			mRot		= mTrans.rotation;
			mScale		= mTrans.lossyScale;

			if (autoDepth) depth = CalculateDepth();
			mAutoDepth	= autoDepth;
			mDepth		= depth;
			mScreen		= UIScreen.GetScreen(mMat = material, mLayer = gameObject.layer, mGroup = group, true);

			OnStart();
			mScreen.AddWidget(this);
		}
	}

	/// <summary>
	/// Unregister this widget.
	/// </summary>

	void OnDestroy ()
	{
		if (material != null && mScreen != null)
		{
			mScreen.RemoveWidget(this);
		}
		mScreen = null;
	}

	/// <summary>
	/// Work-around for Unity not calling OnEnable in some cases.
	/// </summary>

	void Update ()
	{
		if (mScreen == null) Start();

		if (sizeMatchesScale)
		{
			OnMatchScale(transform.localScale);
			sizeMatchesScale = false;
		}

		if (copyFrom != null)
		{
			OnCopyFrom(copyFrom);
			copyFrom = null;
		}
	}

	/// <summary>
	/// Enable and Disable functionality should mirror Start and Destroy.
	/// </summary>

	void OnEnable () { Start(); }
	void OnDisable () { OnDestroy(); }

	/// <summary>
	/// Check to see if anything has changed, and if it has, mark the screen as having changed.
	/// </summary>

	public bool ScreenUpdate ()
	{
		// Call the virtual function
		bool retVal = OnUpdate();

		// If the material or layer has changed, act accordingly
		if (mMat != material || mGroup != group || mLayer != gameObject.layer || mColor != color)
		{
			if (mMat != null) mScreen.RemoveWidget(this);

			mMat	= material;
			mColor	= color;
			mPos	= mTrans.position;
			mRot	= mTrans.rotation;
			mScale	= mTrans.lossyScale;
			mLayer	= gameObject.layer;
			mDepth	= depth;
			mGroup	= group;
			retVal	= true;

			if (mMat != null)
			{
				mScreen = UIScreen.GetScreen(mMat, mLayer, mGroup, true);
				mScreen.AddWidget(this);
			}
		}
		// Check to see if the position, rotation or scale has changed
		else if (mMat != null)
		{
			Vector3 pos = mTrans.position;
			Quaternion rot = mTrans.rotation;
			Vector3 scale = mTrans.lossyScale;

			if (mAutoDepth != autoDepth)
			{
				mAutoDepth = autoDepth;
				if (mAutoDepth) depth = CalculateDepth();
			}

			if (mDepth != depth || mPos != pos || mRot != rot || mScale != scale)
			{
				mPos		= pos;
				mRot		= rot;
				mScale		= scale;
				mAutoDepth	= autoDepth;
				mDepth		= depth;
				retVal		= true;
			}
		}
		return retVal;
	}

	/// <summary>
	/// Virtual version of the Start function.
	/// </summary>

	virtual public void OnStart () { }

	/// <summary>
	/// Virtual version of the Update function. Should return 'true' if the widget has changed visually.
	/// </summary>

	virtual public bool OnUpdate () { return false; }

	/// <summary>
	/// Should set the widget's size to match the specified scale.
	/// </summary>

	virtual protected void OnMatchScale (Vector3 scale) { }

	/// <summary>
	/// Called when a "copy from" widget gets assigned
	/// </summary>

	virtual protected void OnCopyFrom (UIWidget widget) { color = widget.color; }

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>

	virtual public void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols) { }
}