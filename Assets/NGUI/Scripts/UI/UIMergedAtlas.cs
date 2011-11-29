using UnityEngine;
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
}

/*
		UIMergedAtlas ma = panel.GetComponent<UIMergedAtlas>();

		// We want to figure out which textures get merged
		Dictionary<Texture2D, int> texs = new Dictionary<Texture2D, int>();
		foreach (Texture2D tex in panel.mergedTextures) texs[tex] = 1;

		foreach (UIWidget w in widgets)
		{
			Texture2D t = w.material.mainTexture;
			if (t == null) continue;

			if (!texs.ContainsKey(t))
			{
				// The texture is absent -- make note of it
				texs[t] = 0;
			}
			else if (texs[t] == 1)
			{
				// The texture is present and is being merged
				texs[t] = 2;
			}
		}

		// List all textures that are in use
		if (texs.Count > 0)
		{
			GUITools.DrawSeparator();

			foreach (KeyValuePair<Texture2D, int> val in texs)
			{
				if (val.Value == 0)
				{
					// Textures that aren't being merged should show up as red.
					GUI.backgroundColor = Color.red;

					if (GUILayout.Button("\"" + val.Key.name + "\" is separate"))
					{
						panel.mergedTextures.Add(val.Key);
						panel.Merge();
					}
				}
				else
				{
					// Textures that get merged but aren't being used should be orange.
					// Textures that get merged and are being used should be green.
					GUI.backgroundColor = (val.Value == 1) ? new Color(1f, 0.5f, 0f) : Color.green;

					if (GUILayout.Button("\"" + val.Key.name + "\" is merged"))
					{
						panel.mergedTextures.Remove(val.Key);
						panel.Merge();
					}
				}
			}
			GUI.backgroundColor = Color.white;
		}
 */