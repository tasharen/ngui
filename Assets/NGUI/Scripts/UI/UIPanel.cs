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
	[SerializeField] bool mHidden = true;

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
	/// Whether the panel's generated geometry will be hidden or not.
	/// </summary>

	public bool hidden
	{
		get
		{
			return mHidden;
		}
		set
		{
			if (mHidden != value)
			{
				mHidden = value;
				List<UIDrawCall> list = drawCalls;
				HideFlags flags = mHidden ? HideFlags.HideAndDontSave : HideFlags.DontSave | HideFlags.NotEditable;

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
	/// Helper function that marks the specified material as having changed so its mesh is rebuilt next frame.
	/// </summary>
	/// <param name="mat"></param>

	void MarkAsChanged (Material mat) { if (!mChanged.Contains(mat)) mChanged.Add(mat); }

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
				mHidden ? HideFlags.HideAndDontSave : HideFlags.DontSave | HideFlags.NotEditable);
#else
			GameObject go = new GameObject("_UIDrawCall [" + mat.name + "]");
			go.hideFlags = mHidden ? HideFlags.HideAndDontSave : HideFlags.DontSave | HideFlags.NotEditable;
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
	}

	/// <summary>
	/// Set the draw call's geometry responsible for the specified material.
	/// </summary>

	void Rebuild (Material mat)
	{
		foreach (UIWidget w in mWidgets)
		{
			if (w.material != mat || w.color.a < 0.001f) continue;
			if (!w.enabled || !w.gameObject.active) continue;
			int offset = mVerts.Count;

			// Fill the geometry
			w.OnFill(mVerts, mUvs, mCols);

			// Transform all vertices into world space
			Transform t = w.cachedTransform;

			if (generateNormals)
			{
				Vector3 normal = t.TransformDirection(Vector3.back);
				Vector3 tangent = t.TransformDirection(Vector3.right);
				Vector4 tan4 = new Vector4(tangent.x, tangent.y, tangent.z, -1f);

				for (int i = offset, imax = mVerts.Count; i < imax; ++i)
				{
					mVerts[i] = t.TransformPoint(mVerts[i]);
					mNorms.Add(normal);
					mTans.Add(tan4);
				}
			}
			else
			{
				for (int i = offset, imax = mVerts.Count; i < imax; ++i)
				{
					mVerts[i] = t.TransformPoint(mVerts[i]);
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
	/// Find a UIPanel responsible for handling the specified transform.
	/// </summary>

	static public UIPanel Find (Transform trans)
	{
		UIPanel panel = null;

		while (panel == null && trans != null)
		{
			panel = trans.GetComponent<UIPanel>();
			if (panel != null) break;
			if (trans.parent == null) break;
			trans = trans.parent;
		}

		if (panel == null)
		{
			panel = trans.gameObject.AddComponent<UIPanel>();
			//Debug.Log("Adding UIPanel for " + NGUITools.GetHierarchy(trans.gameObject));
		}
		return panel;
	}
}