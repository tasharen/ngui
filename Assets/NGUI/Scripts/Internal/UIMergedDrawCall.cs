/*using UnityEngine;
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

	public bool Create (Shader shader, List<Texture2D> textures)
	{
		// If 'null' was returned, there isn't much for us to do here
		if (textures == null)
		{
			mEntries.Clear();
			return false;
		}

		// Don't redo the work if we've already done it
		if (Matches(textures))
		{
			if (mMat.shader != shader) mMat.shader = shader;
			return true;
		}

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
}*/