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

	protected bool mChanged = true;
	protected bool mPlayMode = true;

	bool mStarted = false;
	Vector3 mPos;
	Quaternion mRot;
	Vector3 mScale;

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
				mColor = value;
				mChanged = true;
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
				mCentered = value;
				mChanged = true;
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
				mDepth = value;
				mChanged = true;
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
	/// Return the widget's final scale.
	/// </summary>

	public Vector2 finalScale
	{
		get
		{
			Transform t = cachedTransform;
			return t.localScale;
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
	/// Tell the panel responsible for the widget that something has changed and the buffers need to be rebuilt.
	/// </summary>

	public void MarkAsChanged ()
	{
		mChanged = true;

		if (enabled && gameObject.active && !Application.isPlaying && mMat != null)
		{
			panel.AddWidget(this);
			mPanel.LateUpdate();
		}
	}

	/// <summary>
	/// Ensure we have a panel referencing this widget.
	/// </summary>

	void CreatePanel ()
	{
		if (mPanel == null && mMat != null)
		{
			mPanel = UIPanel.Find(cachedTransform);
			mPanel.AddWidget(this);
			mChanged = true;
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

	void Awake () { mPlayMode = Application.isPlaying; }

	/// <summary>
	/// Calculate depth automatically.
	/// </summary>

	void Start ()
	{
		mStarted = true;
		if (mDepth == 0) mDepth = CalculateDepth();
		if (mMat != null) panel.AddWidget(this);
		OnStart();
	}

	bool mRecentlyEnabled = false;

	/// <summary>
	/// Mark the widget as having been changed so it gets rebuilt.
	/// </summary>

	void OnEnable ()
	{
		mChanged = true;
		if (mTrans == null) mTrans = transform;
		if (mMat != null)
		{
			if (mStarted && mMat != null) panel.AddWidget(this);
			else mRecentlyEnabled = true;
		}
	}

	/// <summary>
	/// Ensure that this widget has been added to a panel. It's a work-around for the problem of
	/// Unity calling OnEnable prior to parenting objects after Copy/Paste.
	/// </summary>

	void Update ()
	{
		if (mRecentlyEnabled)
		{
			mRecentlyEnabled = false;
			if (mMat != null) panel.AddWidget(this);
		}
	}

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
	/// Unregister this widget.
	/// </summary>

	void OnDestroy ()
	{
		if (mPanel != null)
		{
			mPanel.RemoveWidget(this);
			mPanel = null;
		}
	}

	/// <summary>
	/// Draw some selectable gizmos.
	/// </summary>

	void OnDrawGizmos ()
	{
		if (mPanel != null && mPanel.showGizmos)
		{
			Color outline = new Color(1f, 1f, 1f, 0.2f);

			Vector3 scale = cachedTransform.lossyScale;

			Vector3 offset = scale;
			offset.x = 0f;
			offset.y = 0f;
			offset.z *= mDepth;

			Vector3 pos = cachedTransform.position - cachedTransform.TransformDirection(offset);

			if (!centered) pos += new Vector3(scale.x, -scale.y, scale.z) * 0.5f;

			Gizmos.color = outline;
			Gizmos.DrawWireCube(pos, scale);
			Gizmos.color = Color.clear;
			Gizmos.DrawCube(pos, scale);
		}
	}  

	/// <summary>
	/// Update the widget.
	/// </summary>

	public bool PanelUpdate ()
	{
		if (material == null) return false;

		Transform t = cachedTransform;

		bool retVal = OnUpdate() | mChanged;

		if (retVal)
		{
			// If something has changed, simply update the cached values
			mPos = t.position;
			mRot = t.rotation;
			mScale = t.lossyScale;
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
		mChanged = false;
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
	/// Virtual Start() functionality for widgets.
	/// </summary>

	virtual protected void OnStart () { }

	/// <summary>
	/// Virtual version of the Update function. Should return 'true' if the widget has changed visually.
	/// </summary>

	virtual public bool OnUpdate () { return false; }

	/// <summary>
	/// Virtual function called by the UIPanel that fills the buffers.
	/// </summary>

	virtual public void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols) { }
}