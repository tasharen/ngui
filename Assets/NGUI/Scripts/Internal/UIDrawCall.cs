using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is an internally-created script used by the UI system. You shouldn't be attaching it manually.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/UI Draw Call")]
public class UIDrawCall : MonoBehaviour
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
	/// Material used by this screen.
	/// </summary>

	public Material material { get { return mMat; } set { mMat = value; } }

	/// <summary>
	/// Number of widgets managed by this draw call.
	/// </summary>

	public int widgets { get { return mWidgets.Count; } }

	/// <summary>
	/// The number of triangles in this draw call.
	/// </summary>

	public int triangles { get { return mMesh.vertexCount >> 1; } }

	/// <summary>
	/// Add the specified widget to the managed list.
	/// </summary>

	public void AddWidget (UIWidget widget)
	{
		if (widget != null && !mWidgets.Contains(widget))
		{
			//Debug.Log("Adding " + widget.name + " to " + name);
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
		if (mWidgets != null && mWidgets.Remove(widget))
		{
			//Debug.Log("Removing " + widget.name + " from " + name);
			mRebuild = true;
			LateUpdate();
		}
	}

	/// <summary>
	/// Cleanup.
	/// </summary>

	void OnDestroy () { if (mMesh != null) DestroyImmediate(mMesh); }

	/// <summary>
	/// Rebuild the UI.
	/// </summary>

	public void LateUpdate ()
	{
		// Update all widgets
		for (int i = mWidgets.Count; i > 0; )
		{
			UIWidget w = mWidgets[--i];
			if (w == null) mWidgets.RemoveAt(i);
			else mRebuild |= w.CustomUpdate();
		}

		if (mWidgets.Count == 0)
		{
			DestroyImmediate(gameObject);
		}
		else if (mRebuild)
		{
			RefillGeometry();
			RebuildMeshes();

			// Cleanup
			mVerts.Clear();
			mUvs.Clear();
			mCols.Clear();
			mRebuild = false;
		}
	}

	/// <summary>
	/// Refill the arrays.
	/// </summary>

	void RefillGeometry ()
	{
		// Sort all widgets back-to-front
		mWidgets.Sort(UIWidget.CompareFunc);

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
	}

	/// <summary>
	/// Rebuild the meshes.
	/// </summary>

	void RebuildMeshes ()
	{
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

			// Cache all components
			if (mFilter == null) mFilter = gameObject.GetComponent<MeshFilter>();
			if (mFilter == null) mFilter = gameObject.AddComponent<MeshFilter>();
			if (mRen == null) mRen = gameObject.GetComponent<MeshRenderer>();

			if (mRen == null)
			{
				mRen = gameObject.AddComponent<MeshRenderer>();
				mRen.sharedMaterial = mMat;
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
	}
}