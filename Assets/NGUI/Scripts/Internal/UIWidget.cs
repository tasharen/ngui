using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for all UI components that should be derived from when creating new widget types.
/// </summary>

public abstract class UIWidget : MonoBehaviour
{
	// Cached and saved values
	[SerializeField] Color mColor = Color.white;
	[SerializeField] bool mCentered = true;
	[SerializeField] int mDepth = 0;

	Transform mTrans;
	UIDrawCall mScreen;
	Material mMat;
	Texture2D mTex;

	protected bool mPlayMode = true;
	protected Vector3 mPos;
	protected Quaternion mRot;
	protected Vector3 mScale;
	protected bool mIsDirty = false;

	/// <summary>
	/// Color used by the widget.
	/// </summary>

	public Color color
	{
		get
		{
			return mColor;
		}
		set
		{
			if (mColor != value)
			{
				mIsDirty = true;
				mColor = value;
			}
		}
	}

	/// <summary>
	/// Whether the widget is centered or top-left aligned.
	/// </summary>

	public bool centered
	{
		get
		{
			return mCentered;
		}
		set
		{
			if (mCentered != value)
			{
				mIsDirty = true;
				mCentered = value;
			}
		}
	}
	
	/// <summary>
	/// Depth controls the rendering order -- lowest to highest.
	/// </summary>

	public int depth
	{
		get
		{
			return mDepth;
		}
		set
		{
			if (mDepth != value)
			{
				mIsDirty = true;
				mDepth = value;
			}
		}
	}

	/// <summary>
	/// Transform gets cached for speed.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	/// <summary>
	/// Returns the material used by this widget.
	/// </summary>

	public Material material
	{
		get
		{
			return mMat;
		}
		set
		{
			if (mMat != value)
			{
				RemoveFromScreen(true);
				mMat = value;
				mTex = (mMat != null) ? mMat.mainTexture as Texture2D : null;
				mIsDirty = true;
			}
		}
	}

	/// <summary>
	/// Returns the texture used to draw this widget.
	/// </summary>

	public Texture2D mainTexture
	{
		get
		{
			if (mTex == null)
			{
				Material mat = material;
				if (mat != null) mTex = mat.mainTexture as Texture2D;
			}
			return mTex;
		}
	}

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
	/// Helper function, removes the widget from the rendering screen.
	/// </summary>

	void RemoveFromScreen (bool update)
	{
		if (mMat != null && mScreen != null)
		{
			mScreen.RemoveWidget(this);
			if (update && !mPlayMode) mScreen.LateUpdate();
			mScreen = null;
		}
	}

	/// <summary>
	/// If we haven't created a UI screen yet, do so now.
	/// </summary>

	void AddToScreen ()
	{
		if (mMat != null && mScreen == null)
		{
			mScreen = UIPanel.GetScreen(cachedTransform, mMat);
			mScreen.AddWidget(this);
			mIsDirty = true;
		}
	}

	/// <summary>
	/// Helper function that automatically calculates depth of this widget based on the hierarchy.
	/// </summary>

	int CalculateDepth ()
	{
		Transform t = transform;
		int val = 0;

		for (;;)
		{
			t = t.parent;
			if (t == null) break;
			++val;
			UIWidget w = t.GetComponent<UIWidget>();
			if (w != null) return w.depth + val;
		}
		return val;
	}

	/// <summary>
	/// Cache the transform.
	/// </summary>

	void Awake ()
	{
		if (mDepth == 0) mDepth = CalculateDepth();
		mTrans = transform;
		mPlayMode = Application.isPlaying;
		OnAwake();
	}

	/// <summary>
	/// Unregister this widget.
	/// </summary>

	void OnDestroy () { RemoveFromScreen(false); }

	/// <summary>
	/// Unregister this widget.
	/// </summary>

	void OnDisable () { RemoveFromScreen(false); }

	/// <summary>
	/// Always ensure that the widget has a screen that's referencing it.
	/// </summary>

	void Update () { if (mScreen == null) AddToScreen(); }

	/// <summary>
	/// Force-refresh the widget. Only meant to be executed from the edit mode.
	/// </summary>

	public void Refresh ()
	{
		if (!mPlayMode)
		{
			Update();
			if (mScreen != null) mScreen.LateUpdate();
		}
	}

	/// <summary>
	/// Check to see if anything has changed, and if it has, mark the screen as having changed.
	/// </summary>

	public bool ScreenUpdate ()
	{
		if (mMat == null) return false;

		// Call the virtual function
		bool retVal = OnScreenUpdate();

		// If there is no material to work with, there is no point in updating the pos/rot/scale
		if (mMat != null)
		{
			if (mIsDirty)
			{
				// If something has changed, simply update the cached values
				mPos = mTrans.position;
				mRot = mTrans.rotation;
				mScale = mTrans.lossyScale;
				retVal = true;
			}
			else
			{
				Vector3 pos = mTrans.position;
				Quaternion rot = mTrans.rotation;
				Vector3 scale = mTrans.lossyScale;

				// Check to see if the position, rotation or scale has changed
				if (mPos != pos || mRot != rot || mScale != scale)
				{
					mPos = pos;
					mRot = rot;
					mScale = scale;
					retVal = true;
				}
			}
		}
		mIsDirty = false;
		return retVal;
	}

	/// <summary>
	/// Make the widget pixel-perfect.
	/// </summary>

	virtual public void MakePixelPerfect ()
	{
		Vector3 pos = cachedTransform.localPosition;
		pos.x = Mathf.RoundToInt(pos.x);
		pos.y = Mathf.RoundToInt(pos.y);
		pos.z = Mathf.RoundToInt(pos.z);
		cachedTransform.localPosition = pos;

		Vector3 scale = cachedTransform.localScale;
		scale.x = Mathf.RoundToInt(scale.x);
		scale.y = Mathf.RoundToInt(scale.y);
		scale.z = 1f;
		cachedTransform.localScale = scale;
	}

	/// <summary>
	/// Virtual Awake functionality.
	/// </summary>

	virtual public void OnAwake () { }

	/// <summary>
	/// Virtual version of the Update function. Should return 'true' if the widget has changed visually.
	/// </summary>

	virtual public bool OnScreenUpdate () { return false; }

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>

	virtual public void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols) { }
}