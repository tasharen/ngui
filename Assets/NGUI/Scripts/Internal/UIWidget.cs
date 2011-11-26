using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for all UI components that should be derived from when creating new widget types.
/// </summary>

public abstract class UIWidget : MonoBehaviour
{
	// Cached and saved values
	[SerializeField] Material mMat;
	[SerializeField] Color mColor = Color.white;
	[SerializeField] bool mCentered = true;
	[SerializeField] int mDepth = 0;

	Transform mTrans;
	Texture2D mTex;
	UIPanel mPanel;

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
				mIsDirty = true;

				if (mPanel != null)
				{
					mPanel.RemoveWidget(this);
					mPanel = null;
				}
				
				mMat = value;
				mTex = (mMat != null) ? mMat.mainTexture as Texture2D : null;

				CreatePanel();
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
	/// Returns the UI panel responsible for this widget.
	/// </summary>

	public UIPanel panel
	{
		get
		{
			CreatePanel();
			return mPanel;
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
	/// Ensure we have a panel referencing this widget.
	/// </summary>

	void CreatePanel ()
	{
		if (mPanel == null && mMat != null && enabled && gameObject.active)
		{
			mIsDirty = true;
			mPanel = UIPanel.Find(cachedTransform);
			mPanel.AddWidget(this);
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
		mPlayMode = Application.isPlaying;
		OnAwake();
	}

	/// <summary>
	/// Unregister this widget.
	/// </summary>

	void OnDestroy () { OnDisable(); }

	/// <summary>
	/// Unregister this widget.
	/// </summary>

	void OnDisable ()
	{
		if (mPanel != null)
		{
			mPanel.RemoveWidget(this);
			mPanel = null;
		}
	}

	/// <summary>
	/// Always ensure the widget has a panel responsible for it.
	/// </summary>

	void Update ()
	{
		CreatePanel();

		// Update the widget in edit mode, and if something changes -- refresh it
		if (!mPlayMode && CustomUpdate())
		{
			mIsDirty = true;
			Refresh();
		}
	}

	/// <summary>
	/// Force-refresh the widget. Only meant to be executed from the edit mode.
	/// </summary>

	public void Refresh () { if (!mPlayMode && mMat != null && panel != null) mPanel.Refresh(mMat); }

	/// <summary>
	/// Check to see if anything has changed.
	/// </summary>

	public bool CustomUpdate ()
	{
		if (mMat == null) return false;

		// Call the virtual function
		bool retVal = OnUpdate();

		Transform t = cachedTransform;

		if (mIsDirty)
		{
			// If something has changed, simply update the cached values
			mPos = t.position;
			mRot = t.rotation;
			mScale = t.lossyScale;
			retVal = true;
		}
		else
		{
			Vector3 pos = t.position;
			Quaternion rot = t.rotation;
			Vector3 scale = t.lossyScale;

			// Check to see if the position, rotation or scale has changed
			if (mPos != pos || mRot != rot || mScale != scale)
			{
				mPos = pos;
				mRot = rot;
				mScale = scale;
				retVal = true;
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

	virtual protected void OnAwake () { }

	/// <summary>
	/// Virtual version of the Update function. Should return 'true' if the widget has changed visually.
	/// </summary>

	virtual public bool OnUpdate () { return false; }

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>

	virtual public void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols) { }
}