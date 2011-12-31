using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is an internally-created script used by the UI system. You shouldn't be attaching it manually.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/Draw Call")]
public class UIDrawCall : MonoBehaviour
{
	protected Material		mMat;		// Material used by this screen
	protected Mesh			mMesh;		// Generated mesh
	protected MeshFilter	mFilter;	// Mesh filter for this draw call
	protected MeshRenderer	mRen;		// Mesh renderer for this screen

	/// <summary>
	/// Material used by this screen.
	/// </summary>

	public Material material { get { return mMat; } set { mMat = value; } }

	/// <summary>
	/// The number of triangles in this draw call.
	/// </summary>

	public int triangles { get { return mMesh.vertexCount >> 1; } }

	/// <summary>
	/// Cleanup.
	/// </summary>

	void OnDestroy () { if (mMesh != null) { DestroyImmediate(mMesh); } }

	/// <summary>
	/// Set the draw call's geometry.
	/// </summary>

	public void Set (List<Vector3> verts, List<Vector3> norms, List<Vector4> tans, List<Vector2> uvs, List<Color> cols)
	{
		int count = verts.Count;

		// Safety check to ensure we get valid values
		if (count > 0 && (count == uvs.Count && count == cols.Count) && (count % 4) == 0)
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
				mMesh.name = "UIDrawCall for " + mMat.name;
			}
			else
			{
				mMesh.Clear();
			}

			// Set the mesh values
			mMesh.vertices = verts.ToArray();
			if (norms != null) mMesh.normals = norms.ToArray();
			if (tans != null) mMesh.tangents = tans.ToArray();
			mMesh.uv = uvs.ToArray();
			mMesh.colors = cols.ToArray();
			mMesh.triangles = indices;
			mMesh.RecalculateBounds();
			mFilter.mesh = mMesh;
		}
		else
		{
			if (mMesh != null) mMesh.Clear();
			Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + count);
		}
	}
}