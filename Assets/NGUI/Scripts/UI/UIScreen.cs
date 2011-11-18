#define SHOW_GENERATED_GEOMETRY

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is an internally-created script used by the UI system. You shouldn't be attaching it manually.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("Internal/UI Screen")]
public class UIScreen : MonoBehaviour
{
	Material		mMat;		// Material used by this screen
	Mesh			mMesh;		// Generated mesh
	MeshFilter		mFilter;	// Mesh filter for this screen
	MeshRenderer	mRen;		// Mesh renderer for this screen

	// Whether the screen should be rebuilt next update
	bool mRebuild = false;

	// List of widgets sharing this screen's material
	List<UIWidget> mWidgets = new List<UIWidget>();

	// Cached in order to reduce memory allocations
	List<Vector3> mVerts = new List<Vector3>();
	List<Vector2> mUvs = new List<Vector2>();
	List<Color> mCols = new List<Color>();

	/// <summary>
	/// Get or create a UI panel on the specified transform or its parents.
	/// </summary>

	static public GameObject GetPanelObject (Transform trans)
	{
		UIPanel panel = null;

		while (panel == null && trans != null)
		{
			panel = trans.GetComponent<UIPanel>();
			if (panel != null) break;
			if (trans.parent == null) return trans.gameObject;
			trans = trans.parent;
		}
		return panel.gameObject;
	}

	/// <summary>
	/// Retrieve a UI screen for the specified material, creating one if necessary.
	/// </summary>

	static public UIScreen GetScreen (Transform trans, Material mat)
	{
		GameObject go = GetPanelObject(trans);
		UIScreen[] screens = go.GetComponents<UIScreen>();

		// Find an existing entry
		foreach (UIScreen s in screens) if (s.mMat == mat) return s;

		// Add the UI screen script
		UIScreen screen = go.AddComponent<UIScreen>();
		screen.mMat = mat;
		return screen;
	}

	/// <summary>
	/// Add the specified widget to the managed list.
	/// </summary>

	public void AddWidget (UIWidget widget)
	{
		//Debug.Log("Adding " + widget.name + " to " + name);

		if (widget != null && !mWidgets.Contains(widget))
		{
			mWidgets.Add(widget);
			mRebuild = true;
			if (!Application.isPlaying) LateUpdate();
		}
	}

	/// <summary>
	/// Remove the specified widget from the managed list.
	/// </summary>

	public void RemoveWidget (UIWidget widget)
	{
		//Debug.Log("Removing " + widget.name + " from " + name);

		if (mWidgets != null)
		{
			mWidgets.Remove(widget);
			mRebuild = true;
		}
	}

	/// <summary>
	/// Cleanup.
	/// </summary>

	void OnDestroy ()
	{
		if (Application.isPlaying)
		{
			if (mRen	!= null) Destroy(mRen);
			if (mFilter != null) Destroy(mFilter);
			if (mMesh	!= null) Destroy(mMesh);
		}
		else if (mMesh != null)
		{
			DestroyImmediate(mMesh);
			mMesh = null;
		}
	}

	/// <summary>
	/// Rebuild the UI.
	/// </summary>

	public void LateUpdate ()
	{
		if (mWidgets == null) return;

		// Update all widgets
		for (int i = mWidgets.Count; i > 0; )
		{
			UIWidget w = mWidgets[--i];
			if (w == null) mWidgets.RemoveAt(i);
			else mRebuild |= w.ScreenUpdate();
		}

		// No need to keep this screen if we don't have any widgets left
		if (mWidgets.Count == 0)
		{
			if (Application.isPlaying)
			{
				Destroy(this);
				return;
			}

			if (mRen != null)
			{
				DestroyImmediate(mRen);
				mRen = null;
			}

			if (mFilter != null)
			{
				DestroyImmediate(mFilter);
				mFilter = null;
			}

			if (mMesh != null)
			{
				DestroyImmediate(mMesh);
				mMesh = null;
			}
			if (this != null) DestroyImmediate(this);
			return;
		}

		// Only continue if we need to rebuild
		if (!mRebuild) return;

		// Sort all widgets back-to-front
		mWidgets.Sort(UIWidget.CompareFunc);

		// Cache all components
		if (mFilter == null) mFilter = gameObject.GetComponent<MeshFilter>();
		if (mFilter == null) mFilter = gameObject.AddComponent<MeshFilter>();
		if (mRen == null) mRen = gameObject.GetComponent<MeshRenderer>();

		if (mRen == null)
		{
			mRen = gameObject.AddComponent<MeshRenderer>();
			mRen.sharedMaterial = mMat;
		}

		// Fill the vertices and UVs
		foreach (UIWidget w in mWidgets)
		{
			int offset = mVerts.Count;
			w.OnFill(mVerts, mUvs, mCols);

			// Transform all vertices into world space
			Transform t = w.cachedTransform;

			for (int i = offset, imax = mVerts.Count; i < imax; ++i)
			{
				mVerts[i] = t.TransformPoint(mVerts[i]);
			}
		}
		int count = mVerts.Count;

		// Safety check to ensure we get valid values
		if (count > 0 && (count == mUvs.Count && count == mCols.Count) && (count % 4) == 0)
		{
			int index = 0;

			// It takes 6 indices to draw a quad of 4 vertices
			int[] indices = new int[(count >> 1) * 3];

			// Populate the index buffer
			for (int i = 0; i < count; i += 4)
			{
				indices[index++] = i;
				indices[index++] = i + 1;
				indices[index++] = i + 2;

				indices[index++] = i + 2;
				indices[index++] = i + 3;
				indices[index++] = i;
			}

			if (mMesh == null)
			{
				mMesh = new Mesh();
				mMesh.name = "UIScreen for " + mMat.name;
			}
			else
			{
				mMesh.Clear();
			}

			// Set the mesh values
			mMesh.vertices = mVerts.ToArray();
			mMesh.uv = mUvs.ToArray();
			mMesh.colors = mCols.ToArray();
			mMesh.triangles = indices;
			mMesh.RecalculateBounds();
			mFilter.mesh = mMesh;
		}
		else
		{
			Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + count);
		}

		// Cleanup
		mVerts.Clear();
		mUvs.Clear();
		mCols.Clear();

		// Don't rebuild the screen next frame
		mRebuild = false;
	}
}