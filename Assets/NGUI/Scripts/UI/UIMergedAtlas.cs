/*using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Merged Atlas")]
public class UIMergedAtlas : MonoBehaviour
{
	[System.Serializable]
	class Entry
	{
		public Texture2D tex;
		public Vector2 offset;
		public Vector2 scale;
	}

	[SerializeField] Material mMat;
	[SerializeField] List<Entry> mEntries = new List<Entry>();

	public Material material { get { return mMat; } set { mMat = value; } }

	/// <summary>
	/// Returns the list of entries, removing invalid entries in the process.
	/// </summary>

	List<Entry> entries
	{
		get
		{
			for (int i = mEntries.Count; i > 0; )
			{
				if (mEntries[--i].tex == null)
				{
					mEntries.RemoveAt(i);
				}
			}
			return mEntries;
		}
	}

	/// <summary>
	/// List of used textures.
	/// </summary>

	public Texture2D[] textures
	{
		get
		{
			if (mEntries.Count == 0) return null;
			Texture2D[] texs = new Texture2D[mEntries.Count];
			List<Entry> ents = entries;
			for (int i = 0, imax = ents.Count; i < imax; ++i) texs[i] = ents[i].tex;
			return texs;
		}
	}

	/// <summary>
	/// Whether the specified texture is already present in the atlas.
	/// </summary>

	public bool Contains (Texture2D tex)
	{
		foreach (Entry ent in mEntries)
		{
			if (ent.tex == tex) return true;
		}
		return false;
	}

	/// <summary>
	/// Add the specified texture to the merged atlas.
	/// </summary>

	public bool AddTexture (Texture2D tex)
	{
		if (Contains(tex)) return false;
		Entry ent = new Entry();
		ent.tex = tex;
		mEntries.Add(ent);
		Remerge();
		return true;
	}

	/// <summary>
	/// Remove the specified texture from the atlas.
	/// </summary>

	public bool RemoveTexture (Texture2D tex)
	{
		foreach (Entry ent in mEntries)
		{
			if (ent.tex == tex)
			{
				mEntries.Remove(ent);
				Remerge();
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Replace the specified texture.
	/// </summary>

	public bool ReplaceTexture (Texture2D existing, Texture2D final)
	{
		foreach (Entry ent in mEntries)
		{
			if (ent.tex == existing)
			{
				if (final == null || Contains(final)) mEntries.Remove(ent);
				else ent.tex = final;
				Remerge();
				return true;
			}
		}
		return false;
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

	public void AdjustUVs (Texture2D tex, List<Vector2> uvs, int offset)
	{
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

	/// <summary>
	/// Remerge the multiple textures into a single one.
	/// </summary>

	void Remerge ()
	{
		Debug.Log("TODO");
	}
}*/