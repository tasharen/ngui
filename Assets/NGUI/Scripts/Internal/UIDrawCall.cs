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
		HardAlpha,	// Adjust the alpha, compatible with all devices
		SoftAlpha,	// Alpha-based clipping with a softened edge
	}

	Material		mMat;		// Material used by this screen
	Mesh			mMesh;		// Generated mesh
	MeshFilter		mFilter;	// Mesh filter for this draw call
	MeshRenderer	mRen;		// Mesh renderer for this screen
	Clipping		mClipping;	// Clipping mode
	Vector4			mClipRange;	// Clipping, if used
	Vector2			mClipSoft;	// Clipping softness

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

	public Clipping clipping { get { return mClipping; } set { mClipping = value; } }

	/// <summary>
	/// Clip range set by the panel -- used with a shader that has the "_ClipRange" property.
	/// </summary>

	public Vector4 clipRange { get { return mClipRange; } set { mClipRange = value; } }

	/// <summary>
	/// Clipping softness factor, if soft clipping is used.
	/// </summary>

	public Vector2 clipSoftness { get { return mClipSoft; } set { mClipSoft = value; } }

	/// <summary>
	/// Called just before the draw call gets rendered -- sets the clip range.
	/// This section only applies to Clipped series of shaders, such as "Unlit/Clipped Colored".
	/// </summary>

	void OnWillRenderObject ()
	{
		if (mMat != null && mMat.shader != null)
		{
			/*bool canClip = mMat.shader.name.Contains("(Clipped)");
			bool shouldClip = (mClipping != Clipping.None);

			// Only switch shaders in the editor
			if (canClip != shouldClip && Application.isPlaying)
			{
				if (canClip)
				{
					string name = mMat.shader.name;
					name = name.Replace(" (Clipped)", "");

					Shader shader = Shader.Find(name);
					if (shader != null) mMat.shader = shader;
				}
			}*/

			Vector2 sharpness = new Vector2(1000.0f, 1000.0f);
			if (mClipSoft.x > 0f) sharpness.x = mClipRange.z / mClipSoft.x;
			if (mClipSoft.y > 0f) sharpness.y = mClipRange.w / mClipSoft.y;

			switch (mClipping)
			{
				case Clipping.HardAlpha:
				{
					Shader.EnableKeyword("CLIP_METHOD_ALPHA");
					Shader.DisableKeyword("CLIP_METHOD_HARD");
					Shader.DisableKeyword("CLIP_METHOD_SOFT");
					break;
				}
				case Clipping.HardClip:
				{
					Shader.DisableKeyword("CLIP_METHOD_ALPHA");
					Shader.EnableKeyword("CLIP_METHOD_HARD");
					Shader.DisableKeyword("CLIP_METHOD_SOFT");
					break;
				}
				case Clipping.SoftAlpha:
				{
					Shader.DisableKeyword("CLIP_METHOD_ALPHA");
					Shader.DisableKeyword("CLIP_METHOD_HARD");
					Shader.EnableKeyword("CLIP_METHOD_SOFT");
					Shader.SetGlobalVector("_ClipSharpness", sharpness);
					break;
				}
				default:
				{
					// TODO: Switch to another shader
					Shader.EnableKeyword("CLIP_METHOD_ALPHA");
					Shader.DisableKeyword("CLIP_METHOD_HARD");
					Shader.DisableKeyword("CLIP_METHOD_SOFT");
					break;
				}
			}
			Shader.SetGlobalVector("_ClipRange", mClipRange);
		}
	}

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