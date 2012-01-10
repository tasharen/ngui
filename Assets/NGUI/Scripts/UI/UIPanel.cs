using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Panel is responsible for collecting, sorting and updating widgets in addition to generating widgets' geometry.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Panel")]
public class UIPanel : MonoBehaviour
{
	// Whether normals and tangents will be generated for all meshes
	public bool generateNormals = false;

	// Whether selectable gizmos will be shown for widgets under this panel
	public bool showGizmos = true;

	// Whether generated geometry is shown or hidden
	[SerializeField] bool mDebug = false;

#if UNITY_FLASH // Unity 3.5b6 is bugged when SerializeField is mixed with prefabs (after LoadLevel)
	// Clipping rectangle
	public UIDrawCall.Clipping mClipping = UIDrawCall.Clipping.None;
	public Vector4 mClipRange = Vector4.zero;
	public Vector2 mClipSoftness = new Vector2(40f, 40f);
#else
	// Clipping rectangle
	[SerializeField] UIDrawCall.Clipping mClipping = UIDrawCall.Clipping.None;
	[SerializeField] Vector4 mClipRange = Vector4.zero;
	[SerializeField] Vector2 mClipSoftness = new Vector2(40f, 40f);
#endif

	// List of all widgets managed by this panel
	List<UIWidget> mWidgets = new List<UIWidget>();

	// Widgets using these materials will be rebuilt next frame
	List<Material> mChanged = new List<Material>();

	// List of UI Screens created on hidden and invisible game objects
	List<UIDrawCall> mDrawCalls = new List<UIDrawCall>();

	// Cached in order to reduce memory allocations
	List<Vector3> mVerts = new List<Vector3>();
	List<Vector3> mNorms = new List<Vector3>();
	List<Vector4> mTans = new List<Vector4>();
	List<Vector2> mUvs = new List<Vector2>();
	List<Color> mCols = new List<Color>();

	Transform mTrans;

#if UNITY_EDITOR
	// Screen size, saved for gizmos, since Screen.width and Screen.height returns the Scene view's dimensions in OnDrawGismos.
	Vector2 mScreenSize = Vector2.one;
#endif

	/// <summary>
	/// Whether the panel's generated geometry will be hidden or not.
	/// </summary>

	public bool debug
	{
		get
		{
			return mDebug;
		}
		set
		{
			if (mDebug != value)
			{
				mDebug = value;
				List<UIDrawCall> list = drawCalls;
				HideFlags flags = mDebug ? HideFlags.DontSave | HideFlags.NotEditable : HideFlags.HideAndDontSave;

				foreach (UIDrawCall dc in list)
				{
					GameObject go = dc.gameObject;
					go.active = false;
					go.hideFlags = flags;
					go.active = true;
				}
			}
		}
	}

	/// <summary>
	/// Clipping method used by all draw calls.
	/// </summary>

	public UIDrawCall.Clipping clipping { get { return mClipping; } set { if (mClipping != value) { mClipping = value; UpdateClippingRect(); } } }

	/// <summary>
	/// Rectangle used for clipping (used with a valid shader)
	/// </summary>

	public Vector4 clipRange
	{
		get
		{
			return mClipRange;
		}
		set
		{
			if (mClipRange != value)
			{
				mClipRange = value;
				UpdateClippingRect();
			}
		}
	}

	/// <summary>
	/// Clipping softness is used if the clipped style is set to "Soft".
	/// </summary>

	public Vector2 clipSoftness { get { return mClipSoftness; } set { if (mClipSoftness != value) { mClipSoftness = value; UpdateClippingRect(); } } }

	/// <summary>
	/// Widgets managed by this panel.
	/// </summary>

	public List<UIWidget> widgets { get { return mWidgets; } }

	/// <summary>
	/// Retrieve the list of all active draw calls, removing inactive ones in the process.
	/// </summary>

	public List<UIDrawCall> drawCalls
	{
		get
		{
			for (int i = mDrawCalls.Count; i > 0; )
			{
				UIDrawCall dc = mDrawCalls[--i];
				if (dc == null) mDrawCalls.RemoveAt(i);
			}
			return mDrawCalls;
		}
	}

	/// <summary>
	/// Helper function that marks the specified material as having changed so its mesh is rebuilt next frame.
	/// </summary>

	void MarkAsChanged (Material mat) { if (!mChanged.Contains(mat)) mChanged.Add(mat); }

	/// <summary>
	/// Update the clipping rect in the shaders.
	/// </summary>

	void UpdateClippingRect ()
	{
		Vector4 range = Vector4.zero;

		if (mTrans == null) mTrans = transform;
		Vector3 scale = mTrans.lossyScale;

		if (mClipping != UIDrawCall.Clipping.None)
		{
			range = new Vector4(mClipRange.x, mClipRange.y, mClipRange.z * 0.5f, mClipRange.w * 0.5f);
		}

		if (range.z == 0f) range.z = Screen.width * 0.5f;
		if (range.w == 0f) range.w = Screen.height * 0.5f;

		RuntimePlatform platform = Application.platform;

		if (platform == RuntimePlatform.WindowsPlayer ||
			platform == RuntimePlatform.WindowsWebPlayer ||
			platform == RuntimePlatform.WindowsEditor)
		{
			range.x -= 0.5f;
			range.y += 0.5f;
		}

		range.x *= scale.x;
		range.y *= scale.y;
		range.z *= scale.x;
		range.w *= scale.y;

		Vector2 softness = mClipSoftness;
		softness.x *= scale.x;
		softness.y *= scale.y;

		foreach (UIDrawCall dc in mDrawCalls)
		{
			dc.clipping = mClipping;
			dc.clipRange = range;
			dc.clipSoftness = softness;
		}
	}

	/// <summary>
	/// Add the specified widget to the managed list.
	/// </summary>

	public void AddWidget (UIWidget w)
	{
		if (w == null || mWidgets.Contains(w)) return;
		Material mat = w.material;
		if (mat == null) return;
		mWidgets.Add(w);
		MarkAsChanged(w.material);
	}

	/// <summary>
	/// Remove the specified widget from the managed list.
	/// </summary>

	public void RemoveWidget (UIWidget w) { if (w != null && mWidgets.Remove(w)) MarkAsChanged(w.material); }

	/// <summary>
	/// Get or create a UIScreen responsible for drawing the widgets using the specified material.
	/// </summary>

	UIDrawCall GetDrawCall (Material mat, bool createIfMissing)
	{
		foreach (UIDrawCall dc in drawCalls) if (dc.material == mat) return dc;

		UIDrawCall sc = null;

		if (createIfMissing)
		{
#if UNITY_EDITOR
			// If we're in the editor, create the game object with hide flags set right away
			GameObject go = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags("_UIDrawCall [" + mat.name + "]",
				mDebug ? HideFlags.DontSave | HideFlags.NotEditable : HideFlags.HideAndDontSave);
#else
			GameObject go = new GameObject("_UIDrawCall [" + mat.name + "]");
			go.hideFlags = HideFlags.HideAndDontSave;
#endif

			go.layer = gameObject.layer;
			sc = go.AddComponent<UIDrawCall>();
			sc.material = mat;

			mDrawCalls.Add(sc);
		}
		return sc;
	}

	/// <summary>
	/// Mark all widgets as having been changed so the draw calls get re-created.
	/// </summary>

	void OnEnable () { foreach (UIWidget w in mWidgets) MarkAsChanged(w.material); }

	/// <summary>
	/// Destroy all draw calls we've created when this script gets disabled.
	/// </summary>

	void OnDisable ()
	{
		for (int i = mDrawCalls.Count; i > 0; )
		{
			UIDrawCall dc = mDrawCalls[--i];
			if (dc != null) DestroyImmediate(dc.gameObject);
		}
		mDrawCalls.Clear();
		mChanged.Clear();
	}

	/// <summary>
	/// Update all widgets and rebuild the draw calls if necessary.
	/// </summary>

	public void LateUpdate ()
	{
		// Update all widgets
		for (int i = mWidgets.Count; i > 0; )
		{
			UIWidget w = mWidgets[--i];
			if (w == null) mWidgets.RemoveAt(i);
			else if (w.PanelUpdate()) MarkAsChanged(w.material);
		}

		// If something has changed we have more work to be done
		if (mChanged.Count > 0)
		{
			// Sort all widgets based on their depth
			mWidgets.Sort(UIWidget.CompareFunc);
			foreach (Material mat in mChanged) Rebuild(mat);

			// Run through all the materials that have been marked as changed and rebuild them
			mChanged.Clear();
		}

		// Update the clipping rects
		UpdateClippingRect();

#if UNITY_EDITOR
		mScreenSize = new Vector2(Screen.width, Screen.height);
#endif
	}

#if UNITY_EDITOR

	/// <summary>
	/// Draw a visible pink outline for the clipped area.
	/// </summary>

	void OnDrawGizmos ()
	{
		if (showGizmos && mClipping != UIDrawCall.Clipping.None)
		{
			Vector2 size = new Vector2(mClipRange.z, mClipRange.w);

			if (size.x == 0f) size.x = mScreenSize.x;
			if (size.y == 0f) size.y = mScreenSize.y;

			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(new Vector2(mClipRange.x, mClipRange.y), size);
		}
	}
#endif

	/// <summary>
	/// Set the draw call's geometry responsible for the specified material.
	/// </summary>

	void Rebuild (Material mat)
	{
		foreach (UIWidget w in mWidgets)
		{
			if (w.material != mat || w.color.a < 0.001f) continue;
			if (!w.enabled || !w.gameObject.active) continue;
			int index = mVerts.Count;

			// Fill the geometry
			w.OnFill(mVerts, mUvs, mCols);

			Vector3 offset = w.pivotOffset;
			Vector2 scale = w.visibleSize;
			offset.x *= scale.x;
			offset.y *= scale.y;

			// Transform all vertices into world space
			Transform t = w.cachedTransform;

			if (generateNormals)
			{
				Vector3 normal = t.TransformDirection(Vector3.back);
				Vector3 tangent = t.TransformDirection(Vector3.right);
				Vector4 tan4 = new Vector4(tangent.x, tangent.y, tangent.z, -1f);

				for (int i = index, imax = mVerts.Count; i < imax; ++i)
				{
					mVerts[i] = t.TransformPoint(mVerts[i] + offset);
					mNorms.Add(normal);
					mTans.Add(tan4);
				}
			}
			else
			{
				for (int i = index, imax = mVerts.Count; i < imax; ++i)
				{
					mVerts[i] = t.TransformPoint(mVerts[i] + offset);
				}
			}
		}

		if (mVerts.Count > 0)
		{
			// Rebuild the draw call's mesh
			UIDrawCall dc = GetDrawCall(mat, true);
			dc.Set(mVerts, generateNormals ? mNorms : null, generateNormals ? mTans : null, mUvs, mCols);
		}
		else
		{
			// There is nothing to draw for this material -- eliminate the draw call
			UIDrawCall dc = GetDrawCall(mat, false);

			if (dc != null)
			{
				mDrawCalls.Remove(dc);
				DestroyImmediate(dc.gameObject);
			}
		}

		// Cleanup
		mVerts.Clear();
		mNorms.Clear();
		mTans.Clear();
		mUvs.Clear();
		mCols.Clear();
	}

	/// <summary>
	/// Find the UIPanel responsible for handling the specified transform, creating a new one if necessary.
	/// </summary>

	static public UIPanel Find (Transform trans) { return Find(trans, true); }

	/// <summary>
	/// Find the UIPanel responsible for handling the specified transform.
	/// </summary>

	static public UIPanel Find (Transform trans, bool createIfMissing)
	{
		UIPanel panel = null;

		while (panel == null && trans != null)
		{
			panel = trans.GetComponent<UIPanel>();
			if (panel != null) break;
			if (trans.parent == null) break;
			trans = trans.parent;
		}

		if (createIfMissing && panel == null)
		{
			panel = trans.gameObject.AddComponent<UIPanel>();
		}
		return panel;
	}
}