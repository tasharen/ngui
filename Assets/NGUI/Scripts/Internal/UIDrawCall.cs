using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is an internally-created script used by the UI system. You shouldn't be attaching it manually.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/Draw Call")]
public class UIDrawCall : MonoBehaviour
{
	public enum Clipping
	{
		None,
		HardClip,	// Uses the hardware clip() function -- may be slow on some mobile devices
		AlphaClip,	// Adjust the alpha, compatible with all devices
		SoftClip,	// Alpha-based clipping with a softened edge
	}

	Transform		mTrans;		// Cached transform
	Material		mMat;		// Material used by this screen
	Mesh			mMesh;		// Generated mesh
	MeshFilter		mFilter;	// Mesh filter for this draw call
	MeshRenderer	mRen;		// Mesh renderer for this screen
	Clipping		mClipping;	// Clipping mode
	Vector4			mClipRange;	// Clipping, if used
	Vector2			mClipSoft;	// Clipping softness
	Material		mInst;		// Instantiated material, if necessary
	bool			mReset		= true;

	/// <summary>
	/// Transform is cached for speed and efficiency.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	/// <summary>
	/// Material used by this screen.
	/// </summary>

	public Material material { get { return mMat; } set { mMat = value; } }

	/// <summary>
	/// The number of triangles in this draw call.
	/// </summary>

	public int triangles { get { return mMesh.vertexCount >> 1; } }

	/// <summary>
	/// Clipping used by the draw call
	/// </summary>

	public Clipping clipping { get { return mClipping; } set { if (mClipping != value) { mClipping = value; mReset = true; } } }

	/// <summary>
	/// Clip range set by the panel -- used with a shader that has the "_ClipRange" property.
	/// </summary>

	public Vector4 clipRange { get { return mClipRange; } set { mClipRange = value; } }

	/// <summary>
	/// Clipping softness factor, if soft clipping is used.
	/// </summary>

	public Vector2 clipSoftness { get { return mClipSoft; } set { mClipSoft = value; } }

	/// <summary>
	/// Convenience function that ensures that a custom material has been created.
	/// </summary>

	Material customMaterial { get { if (mInst == null) { mInst = new Material(mMat); mRen.sharedMaterial = mInst; } return mInst; } }

	/// <summary>
	/// This function is called when it's clear that the object will be rendered.
	/// We want to set the shader used by the material, creating a copy of the material in the process.
	/// We also want to update the material's properties before it's actually used.
	/// </summary>

	void OnWillRenderObject ()
	{
		if (mReset)
		{
			mReset = false;

			if (mMat != null && mMat.shader != null)
			{
				bool useClipping = (mClipping != Clipping.None);

				// If we should be using clipping we should check to see if we can automatically locate the shader
				if (useClipping)
				{
					const string hard	= " (HardClip)";
					const string alpha	= " (AlphaClip)";
					const string soft	= " (SoftClip)";

					// Figure out the normal shader's name
					string shaderName = mMat.shader.name;
					shaderName = shaderName.Replace(hard, "");
					shaderName = shaderName.Replace(alpha, "");
					shaderName = shaderName.Replace(soft, "");

					Shader shader = null;

					// Try to find the new shader
					if		(mClipping == Clipping.HardClip)	shader = Shader.Find(shaderName + hard);
					else if (mClipping == Clipping.AlphaClip)	shader = Shader.Find(shaderName + alpha);
					else if (mClipping == Clipping.SoftClip)	shader = Shader.Find(shaderName + soft);

					// If there is a valid shader, assign it to the custom material
					if (shader != null) customMaterial.shader = shader;
					else useClipping = false;
				}

				// If we shouldn't be using clipping, revert back to the original material
				if (!useClipping)
				{
					mRen.sharedMaterial = mMat;

					if (mInst != null)
					{
						DestroyImmediate(mInst);
						mInst = null;
					}
				}
			}
		}

		if (mInst != null)
		{
			mInst.SetVector("_ClipRange", mClipRange);

			Vector2 sharpness = new Vector2(1000.0f, 1000.0f);
			if (mClipSoft.x > 0f) sharpness.x = mClipRange.z / mClipSoft.x;
			if (mClipSoft.y > 0f) sharpness.y = mClipRange.w / mClipSoft.y;
			mInst.SetVector("_ClipSharpness", sharpness);
		}
	}

	/// <summary>
	/// Cleanup.
	/// </summary>

	void OnDestroy ()
	{
		if (mMesh != null) DestroyImmediate(mMesh);
		if (mInst != null) DestroyImmediate(mInst);
	}

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

			if (verts.Count < 65000)
			{
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
				Debug.LogError("Too many vertices on one panel: " + verts.Count);
			}
		}
		else
		{
			if (mMesh != null) mMesh.Clear();
			Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + count);
		}
	}
}