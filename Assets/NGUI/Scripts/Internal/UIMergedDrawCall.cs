using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is an internally-created script used by the UI system. You shouldn't be attaching it manually.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/Merged Draw Call")]
public class UIMergedDrawCall : UIDrawCall
{
	class Entry
	{
		public Texture2D tex;
		public Vector2 offset;
		public Vector2 scale;
	}

	// Combined texture used by this draw call
	Texture2D mTex;

	// List of all textures that have been merged into this draw call
	List<Entry> mEntries = new List<Entry>();

	/// <summary>
	/// Cleanup.
	/// </summary>

	void OnDestroy ()
	{
		if (mMat  != null) DestroyImmediate(mMat);
		if (mTex  != null) DestroyImmediate(mTex);
		if (mMesh != null) DestroyImmediate(mMesh);
	}

	/// <summary>
	/// Collect textures of specified materials, only returning a valid list if there are 2 or more textures to work with.
	/// </summary>

	List<Texture2D> GetTextures (List<Material> mats)
	{
		if (mats.Count < 2) return null;
		List<Texture2D> textures = new List<Texture2D>();
		Shader shader = mats[0].shader;

		// Run through all specified materials and collect their textures
		foreach (Material mat in mats)
		{
			Texture2D tex = mat.mainTexture as Texture2D;

			if (tex != null)
			{
				if (!textures.Contains(tex))
				{
					if (mat.shader != shader)
					{
						Debug.LogWarning("You must use identical shaders on all of the materials you are trying to merge");
						return null;
					}
					textures.Add(tex);
				}
			}
			else Debug.LogWarning("Material \"" + mat.name + "\" has no texture");
		}
		return (textures.Count > 1) ? textures : null;
	}

	/// <summary>
	/// Whether the draw call contains the specified texture.
	/// </summary>

	public bool Contains (Texture2D tex)
	{
		foreach (Entry ent in mEntries) if (ent.tex == tex) return true;
		return false;
	}

	/// <summary>
	/// Returns 'true' if the current list of merged textures matches the specified list, 'false' otherwise.
	/// </summary>

	bool Matches (List<Texture2D> textures)
	{
		if (mEntries.Count != textures.Count) return false;
		foreach (Texture2D tex in textures) if (!Contains(tex)) return false;
		return true;
	}

	/// <summary>
	/// Create the texture and material that can be used to draw all of the specified materials in a single draw call.
	/// </summary>

	public bool Create (List<Material> mats)
	{
		List<Texture2D> textures = GetTextures(mats);

		// If 'null' was returned, there isn't much for us to do here
		if (textures == null)
		{
			mEntries.Clear();
			return false;
		}

		// Don't redo the work if we've already done it
		if (Matches(textures)) return true;

		Shader shader = mats[0].shader;

		// Create the merged texture
		if (mTex == null) mTex = new Texture2D(1, 1);
		Rect[] rects = mTex.PackTextures(textures.ToArray(), 0);
		
		if (rects == null)
		{
			mEntries.Clear();
			Debug.LogError("Merge failed");
			return false;
		}

		// Calculate all conversion entries
		for (int i = 0; i < textures.Count; ++i)
		{
			Entry ent = new Entry();
			ent.tex = textures[i];
			Rect r = rects[i];
			ent.offset = new Vector2(r.xMin, r.yMin);
			ent.scale = new Vector2(r.width / mTex.width, r.height / mTex.height);
			mEntries.Add(ent);
		}

		// Create the material we'll be using
		if (mMat == null)
		{
			mMat = new Material(shader);
			mMat.name = "UIMergedDrawCall Material";
		}

		// Set the material's texture
		mMat.shader = shader;
		mMat.mainTexture = mTex;
		return true;
	}

	/// <summary>
	/// Adjust the specified list of UV coordinates to point to UVs within the merged texture.
	/// </summary>

	void AdjustUVs (Entry ent, List<Vector2> uvs, int offset)
	{
		for (int i = offset, imax = uvs.Count; i < imax; ++i)
		{
			Vector2 uv = uvs[i];
			uv.x = uv.x * ent.scale.x + ent.offset.x;
			uv.y = uv.y * ent.scale.y + ent.offset.y;
			uvs[i] = uv;
		}
	}

	/// <summary>
	/// Adjust the specified list of UV coordinates to point to UVs within the merged texture.
	/// </summary>

	public void AdjustUVs (Material mat, List<Vector2> uvs, int offset)
	{
		Texture2D tex = mat.mainTexture as Texture2D;
		
		if (tex != null)
		{
			foreach (Entry ent in mEntries)
			{
				if (ent.tex == tex)
				{
					AdjustUVs(ent, uvs, offset);
					return;
				}
			}
		}
	}
}