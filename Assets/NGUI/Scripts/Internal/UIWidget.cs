using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for all UI components that should be derived from when creating new widget types.
/// </summary>

public abstract class UIWidget : MonoBehaviour
{
	public enum Pivot
	{
		TopLeft,
		Top,
		TopRight,
		Left,
		Center,
		Right,
		BottomLeft,
		Bottom,
		BottomRight,
	}

	// Cached and saved values
#if UNITY_FLASH // Unity 3.5b6 is bugged when SerializeField is mixed with prefabs (after LoadLevel)
	public Material mMat;
	public Color mColor = Color.white;
	public Pivot mPivot = Pivot.Center;
	public int mDepth = 0;
#else
	[SerializeField] Material mMat;
	[SerializeField] Color mColor = Color.white;
	[SerializeField] Pivot mPivot = Pivot.Center;
	[SerializeField] int mDepth = 0;
#endif

	Transform mTrans;
	Texture2D mTex;
	UIPanel mPanel;

	protected bool mChanged = true;
	protected bool mPlayMode = true;

	Vector3 mDiffPos;
	Quaternion mDiffRot;
	Vector3 mDiffScale;
	int mVisibleFlag = -1;

	// Widget's generated geometry
	UIGeometry mGeom = new UIGeometry();

	/// <summary>
	/// Color used by the widget.
	/// </summary>

	public Color color { get { return mColor; } set { if (mColor != value) { mColor = value; mChanged = true; } } }

	/// <summary>
	/// Set or get the value that specifies where the widget's pivot point should be.
	/// </summary>

	public Pivot pivot { get { return mPivot; } set { if (mPivot != value) { mPivot = value; mChanged = true; MakePixelPerfect(); } } }
	
	/// <summary>
	/// Depth controls the rendering order -- lowest to highest.
	/// </summary>

	public int depth { get { return mDepth; } set { if (mDepth != value) { mDepth = value; if (mPanel != null) mPanel.MarkMaterialAsChanged(mMat, true); } } }

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
	/// Returns the UI panel responsible for this widget.
	/// </summary>

	public UIPanel panel { get { CreatePanel(); return mPanel; } set { mPanel = value; } }

	/// <summary>
	/// Flag set by the UIPanel and used in optimization checks.
	/// </summary>

	public int visibleFlag { get { return mVisibleFlag; } set { mVisibleFlag = value; } }

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

	public virtual void MarkAsChanged ()
	{
		mChanged = true;

		if (enabled && gameObject.active && !Application.isPlaying && mMat != null)
		{
			panel.AddWidget(this);
			LayerCheck();
#if UNITY_EDITOR
			mPanel.CustomLateUpdate();
#endif
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
			LayerCheck();
			mPanel.AddWidget(this);
			mChanged = true;
		}
	}

	/// <summary>
	/// Check to ensure that the widget resides on the same layer as its panel.
	/// </summary>

	void LayerCheck ()
	{
		if (mPanel != null && mPanel.gameObject.layer != gameObject.layer)
		{
			Debug.LogWarning("You can't place widgets on a layer different than the UIPanel that manages them.\n" +
				"If you want to move widgets to a different layer, parent them to a new panel instead.", this);
			gameObject.layer = mPanel.gameObject.layer;
		}
	}

	/// <summary>
	/// Cache the transform.
	/// </summary>

	void Awake ()
	{
		if (GetComponents<UIWidget>().Length > 1)
		{
			Debug.LogError("Can't have more than one widget on the same game object.\nDestroying the second one.", this);
			if (Application.isPlaying) DestroyImmediate(this);
			else Destroy(this);
		}
		else
		{
			mPlayMode = Application.isPlaying;
		}
	}

	/// <summary>
	/// Mark the widget and the panel as having been changed.
	/// </summary>

	void OnEnable ()
	{
		mChanged = true;
		if (mPanel != null && mMat != null) mPanel.MarkMaterialAsChanged(mMat, false);
	}

	/// <summary>
	/// Set the depth, call the virtual start function, and sure we have a panel to work with.
	/// </summary>

	void Start ()
	{
		OnStart();
		CreatePanel();
	}

	/// <summary>
	/// Ensure that this widget has been added to a panel. It's a work-around for the problem of
	/// Unity calling OnEnable prior to parenting objects after Copy/Paste.
	/// </summary>

	void Update ()
	{
		LayerCheck();

		// Ensure we have a panel to work with by now
		if (mPanel == null) CreatePanel();
		
		// Automatically reset the Z scaling component back to 1 as it's not used
		Vector3 scale = cachedTransform.localScale;

		if (scale.z != 1f)
		{
			scale.z = 1f;
			mTrans.localScale = scale;
		}
	}

	/// <summary>
	/// Don't store any references to the panel.
	/// </summary>

	void OnDisable () { if (mPanel != null && mMat != null) mPanel.MarkMaterialAsChanged(mMat, false); mPanel = null; }

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

#if UNITY_EDITOR

	/// <summary>
	/// Draw some selectable gizmos.
	/// </summary>

	void OnDrawGizmos ()
	{
		if (visibleFlag != 0 && mPanel != null && mPanel.debugInfo == UIPanel.DebugInfo.Gizmos)
		{
			Color outline = new Color(1f, 1f, 1f, 0.2f);

			// Position should be offset by depth so that the selection works properly
			Vector3 pos = Vector3.zero;
			pos.z -= mDepth * 0.25f;

			// Widget's local size
			Vector2 size = relativeSize;
			Vector2 offset = pivotOffset;
			pos.x += (offset.x + 0.5f) * size.x;
			pos.y += (offset.y - 0.5f) * size.y;

			// Draw the gizmo
			Gizmos.matrix = cachedTransform.localToWorldMatrix;
			Gizmos.color = (UnityEditor.Selection.activeGameObject == gameObject) ? new Color(0f, 0.75f, 1f) : outline;
			Gizmos.DrawWireCube(pos, size);
			Gizmos.color = Color.clear;
			Gizmos.DrawCube(pos, size);
		}
	}
#endif

	/// <summary>
	/// Update the widget and fill its geometry if necessary. Returns whether something was changed.
	/// </summary>

	public bool UpdateGeometry (ref Matrix4x4 worldToPanel, bool parentMoved, bool generateNormals)
	{
		if (material == null) return false;
		bool retVal = OnUpdate() | mChanged;
		mChanged = false;

		if (retVal || !mGeom.hasVertices)
		{
			Vector3 offset = pivotOffset;
			Vector2 scale = relativeSize;
			offset.x *= scale.x;
			offset.y *= scale.y;

			mGeom.Clear();
			OnFill(mGeom.verts, mGeom.uvs, mGeom.cols);
			mGeom.ApplyOffset(offset);
			mGeom.ApplyTransform(worldToPanel * cachedTransform.localToWorldMatrix, generateNormals);
			return true;
		}
		else if (mGeom.hasVertices && parentMoved)
		{
			mGeom.ApplyTransform(worldToPanel * cachedTransform.localToWorldMatrix, generateNormals);
		}
		return false;
	}

	/// <summary>
	/// Append the local geometry buffers to the specified ones.
	/// </summary>

	public void WriteToBuffers (List<Vector3> v, List<Vector2> u, List<Color> c, List<Vector3> n, List<Vector4> t)
	{
		mGeom.WriteToBuffers(v, u, c, n, t);
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

		Vector3 scale = cachedTransform.localScale;

		int width  = Mathf.RoundToInt(scale.x);
		int height = Mathf.RoundToInt(scale.y);

		scale.x = width;
		scale.y = height;
		scale.z = 1f;

		if (width  % 2 == 1 && (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom)) pos.x += 0.5f;
		if (height % 2 == 1 && (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right)) pos.y -= 0.5f;

		cachedTransform.localPosition = pos;
		cachedTransform.localScale = scale;
	}

	/// <summary>
	/// Helper function that calculates the relative offset based on the current pivot.
	/// </summary>

	virtual public Vector2 pivotOffset
	{
		get
		{
			Vector2 v = Vector2.zero;

			if (mPivot == Pivot.Top || mPivot == Pivot.Center || mPivot == Pivot.Bottom) v.x = -0.5f;
			else if (mPivot == Pivot.TopRight || mPivot == Pivot.Right || mPivot == Pivot.BottomRight) v.x = -1f;

			if (mPivot == Pivot.Left || mPivot == Pivot.Center || mPivot == Pivot.Right) v.y = 0.5f;
			else if (mPivot == Pivot.BottomLeft || mPivot == Pivot.Bottom || mPivot == Pivot.BottomRight) v.y = 1f;

			return v;
		}
	}

	/// <summary>
	/// Deprecated property.
	/// </summary>

	[System.Obsolete("Use 'relativeSize' instead")]
	public Vector2 visibleSize { get { return relativeSize; } }

	/// <summary>
	/// Visible size of the widget in relative coordinates. In most cases this can remain at (1, 1).
	/// If you want to figure out the widget's size in pixels, scale this value by cachedTransform.localScale.
	/// </summary>

	virtual public Vector2 relativeSize { get { return Vector2.one; } }

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