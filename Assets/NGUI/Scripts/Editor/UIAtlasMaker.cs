using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Atlas maker lets you create atlases from a bunch of small textures. It's an alternative to using the external Texture Packer.
/// </summary>

public class UIAtlasMaker : EditorWindow
{
	class SpriteEntry
	{
		public Texture2D tex;	// Sprite texture -- original texture or a temporary texture
		public Rect rect;		// Sprite's outer rectangle within the generated texture atlas
		public int minX = 0;	// Padding, if any (set if the sprite is trimmed)
		public int maxX = 0;
		public int minY = 0;
		public int maxY = 0;
		public bool temporaryTexture = false;	// Whether the texture is temporary and should be deleted
	}

	UIAtlas mAtlas;

	void OnSelectAtlas (MonoBehaviour obj)
	{
		UIAtlas a = obj as UIAtlas;

		if (mAtlas != a)
		{
			mAtlas = a;
			Save();
			Repaint();
		}
	}

	void Save ()
	{
		PlayerPrefs.SetInt("NGUI Atlas", (mAtlas != null) ? mAtlas.GetInstanceID() : -1);
	}

	void Load ()
	{
		int atlasID = PlayerPrefs.GetInt("NGUI Atlas", -1);
		if (atlasID != -1) mAtlas = EditorUtility.InstanceIDToObject(atlasID) as UIAtlas;
	}

	/// <summary>
	/// Refresh the window on selection.
	/// </summary>

	void OnSelectionChange () { Repaint(); }

	/// <summary>
	/// Helper function that retrieves the list of currently selected textures.
	/// </summary>

	List<Texture> GetSelectedTextures ()
	{
		List<Texture> textures = new List<Texture>();

		if (Selection.objects != null && Selection.objects.Length > 0)
		{
			Object[] objects = EditorUtility.CollectDependencies(Selection.objects);

			foreach (Object o in objects)
			{
				Texture tex = o as Texture;
				if (tex != null && mAtlas.texture != tex) textures.Add(tex);
			}
		}
		return textures;
	}

	/// <summary>
	/// Helper function that creates a single sprite list from both the atlas's sprites as well as selected textures.
	/// Dictionary value meaning:
	/// 0 = No change
	/// 1 = Update
	/// 2 = Add
	/// </summary>

	Dictionary<string, int> GetSpriteList (List<Texture> textures)
	{
		Dictionary<string, int> spriteList = new Dictionary<string, int>();
		List<string> spriteNames = mAtlas.GetListOfSprites();
		foreach (string sp in spriteNames) spriteList.Add(sp, 0);

		// If we have textures to work with, include them as well
		if (textures.Count > 0)
		{
			List<string> texNames = new List<string>();
			foreach (Texture tex in textures) texNames.Add(tex.name);
			texNames.Sort();

			foreach (string tex in texNames)
			{
				if (spriteList.ContainsKey(tex)) spriteList[tex] = 1;
				else spriteList.Add(tex, 2);
			}
		}
		return spriteList;
	}

	/// <summary>
	/// Draw the UI for this tool.
	/// </summary>

	void OnGUI ()
	{
		if (mAtlas == null) Load();

		EditorGUIUtility.LookLikeControls(80f);

		GUILayout.Space(6f);

		ComponentSelector.Draw<UIAtlas>(mAtlas, OnSelectAtlas);

		if (mAtlas != null)
		{
			List<Texture> textures = GetSelectedTextures();
			bool update = false;
			bool replace = false;

			if (textures.Count > 0)
			{
				GUILayout.BeginHorizontal();
				GUI.backgroundColor = textures.Count > 0 ? Color.green : Color.grey;
				update = GUILayout.Button("Add/Update " + textures.Count + (textures.Count > 1 ? " sprites" : " sprite"));
				GUI.backgroundColor = textures.Count > 0 ? Color.red : Color.grey;
				replace = GUILayout.Button("Replace All");
				GUI.backgroundColor = Color.white;
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.Label("Select one or more textures to work with\nin the Project View window.");
			}

			NGUIEditorTools.DrawHeader("Sprites");

			Dictionary<string, int> spriteList = GetSpriteList(textures);

			int index = 0;
			foreach (KeyValuePair<string, int> iter in spriteList)
			{
				++index;
				GUILayout.BeginHorizontal();
				GUILayout.Label(index.ToString(), GUILayout.Width(24f));
				GUILayout.Label(iter.Key);

				if (iter.Value == 2)
				{
					GUI.color = Color.green;
					GUILayout.Label("Add", GUILayout.Width(27f));
					GUI.color = Color.white;
				}
				else if (iter.Value == 1)
				{
					GUI.color = Color.cyan;
					GUILayout.Label("Update", GUILayout.Width(45f));
					GUI.color = Color.white;
				}

				GUILayout.EndHorizontal();
				Rect rect = GUILayoutUtility.GetLastRect();
				GUI.Box(rect, "");
			}

			if (update) UpdateAtlas(textures);
			else if (replace) ReplaceAtlas(textures);
			return;
		}
	}

	/// <summary>
	/// Load the specified list of textures as Texture2Ds, fixing their import properties as necessary.
	/// </summary>

	static List<Texture2D> LoadTextures (List<Texture> textures)
	{
		List<Texture2D> list = new List<Texture2D>();

		foreach (Texture tex in textures)
		{
			Texture2D t2 = NGUIEditorTools.ImportTexture(tex, true, false);
			if (t2 != null) list.Add(t2);
		}
		return list;
	}

	/// <summary>
	/// Add a new sprite to the atlas, given the texture it's coming from and the packed rect within the atlas.
	/// </summary>

	static UIAtlas.Sprite AddSprite (List<UIAtlas.Sprite> sprites, SpriteEntry se)
	{
		UIAtlas.Sprite sprite = null;

		// See if this sprite already exists
		foreach (UIAtlas.Sprite sp in sprites)
		{
			if (sp.name == se.tex.name)
			{
				sprite = sp;
				break;
			}
		}

		if (sprite != null)
		{
			float x0 = sprite.inner.xMin - sprite.outer.xMin;
			float y0 = sprite.inner.yMin - sprite.outer.yMin;
			float x1 = sprite.outer.xMax - sprite.inner.xMax;
			float y1 = sprite.outer.yMax - sprite.inner.yMax;

			sprite.outer = se.rect;
			sprite.inner = se.rect;

			sprite.inner.xMin = Mathf.Max(sprite.inner.xMin + x0, sprite.outer.xMin);
			sprite.inner.yMin = Mathf.Max(sprite.inner.yMin + y0, sprite.outer.yMin);
			sprite.inner.xMax = Mathf.Min(sprite.inner.xMax - x1, sprite.outer.xMax);
			sprite.inner.yMax = Mathf.Min(sprite.inner.yMax - y1, sprite.outer.yMax);
		}
		else
		{
			sprite = new UIAtlas.Sprite();
			sprite.name = se.tex.name;
			sprite.outer = se.rect;
			sprite.inner = se.rect;
			sprites.Add(sprite);
		}

		float width  = Mathf.Max(1f, sprite.outer.width);
		float height = Mathf.Max(1f, sprite.outer.height);

		// Sprite's padding values are relative to width and height
		sprite.paddingLeft	 = se.minX / width;
		sprite.paddingRight  = se.maxX / width;
		sprite.paddingTop	 = se.maxY / height;
		sprite.paddingBottom = se.minY / height;
		return sprite;
	}

	/// <summary>
	/// Create a list of sprites using the specified list of textures.
	/// </summary>

	static List<SpriteEntry> CreateSprites (List<Texture> textures)
	{
		List<SpriteEntry> list = new List<SpriteEntry>();

		foreach (Texture tex in textures)
		{
			Texture2D oldTex = NGUIEditorTools.ImportTexture(tex, true, false);
			if (oldTex == null) continue;

			Color32[] pixels = oldTex.GetPixels32();

			int xmin = oldTex.width;
			int xmax = 0;
			int ymin = oldTex.height;
			int ymax = 0;
			int oldWidth = oldTex.width;
			int oldHeight = oldTex.height;

			for (int y = 0, yw = oldHeight; y < yw; ++y)
			{
				for (int x = 0, xw = oldWidth; x < xw; ++x)
				{
					Color32 c = pixels[y * xw + x];
					
					if (c.a != 0)
					{
						if (y < ymin) ymin = y;
						if (y > ymax) ymax = y;
						if (x < xmin) xmin = x;
						if (x > xmax) xmax = x;
					}
				}
			}

			int newWidth  = (xmax - xmin) + 1;
			int newHeight = (ymax - ymin) + 1;

			if (newWidth > 0 && newHeight > 0)
			{
				SpriteEntry sprite = new SpriteEntry();
				sprite.rect = new Rect(0f, 0f, oldTex.width, oldTex.height);

				if (newWidth == oldWidth && newHeight == oldHeight)
				{
					sprite.tex = oldTex;
					sprite.temporaryTexture = false;
				}
				else
				{
					Color32[] newPixels = new Color32[newWidth * newHeight];

					for (int y = 0; y < newHeight; ++y)
					{
						for (int x = 0; x < newWidth; ++x)
						{
							int newIndex = y * newWidth + x;
							int oldIndex = (ymin + y) * oldWidth + (xmin + x);
							newPixels[newIndex] = pixels[oldIndex];
						}
					}

					// Create a new texture
					sprite.temporaryTexture = true;
					sprite.tex = new Texture2D(newWidth, newHeight);
					sprite.tex.name = oldTex.name;
					sprite.tex.SetPixels32(newPixels);

					// Remember the padding offset
					sprite.minX = xmin;
					sprite.maxX = oldWidth - newWidth - xmin;
					sprite.minY = ymin;
					sprite.maxY = oldHeight - newHeight - ymin;
				}

				list.Add(sprite);
			}
		}
		return list;
	}

	/// <summary>
	/// Release all temporary textures created for the sprites.
	/// </summary>

	static void ReleaseSprites (List<SpriteEntry> sprites)
	{
		foreach (SpriteEntry se in sprites)
		{
			if (se.temporaryTexture)
			{
				NGUITools.Destroy(se.tex);
				se.tex = null;
			}
		}
		Resources.UnloadUnusedAssets();
	}

	/// <summary>
	/// Pack all of the specified sprites into a single texture, updating the outer and inner rects of the sprites as needed.
	/// </summary>

	static Texture2D PackTextures (List<SpriteEntry> sprites)
	{
		Texture2D atlasTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

		Texture2D[] textures = new Texture2D[sprites.Count];
		for (int i = 0; i < sprites.Count; ++i) textures[i] = sprites[i].tex;

		Rect[] rects = atlasTexture.PackTextures(textures, 1, 4096);

		for (int i = 0; i < sprites.Count; ++i)
		{
			sprites[i].rect = NGUIMath.ConvertToPixels(rects[i], atlasTexture.width, atlasTexture.height, true);
		}
		return atlasTexture;
	}

	/// <summary>
	/// Replace the sprites within the atlas and change the atlas texture to the specified one.
	/// </summary>

	static void ReplaceAtlas (UIAtlas atlas, List<SpriteEntry> sprites, Texture2D newAtlasTexture)
	{
		Undo.RegisterUndo(atlas.texture, "Replace Atlas");

		// Get the list of sprites we'll be updating
		List<UIAtlas.Sprite> spriteList = atlas.sprites;
		List<UIAtlas.Sprite> kept = new List<UIAtlas.Sprite>();

		// The atlas must be in pixels
		atlas.coordinates = UIAtlas.Coordinates.Pixels;

		// Run through all the textures we added and add them as sprites to the atlas
		for (int i = 0; i < sprites.Count; ++i)
		{
			SpriteEntry se = sprites[i];
			UIAtlas.Sprite sprite = AddSprite(spriteList, se);
			kept.Add(sprite);
		}

		// Remove unused sprites
		for (int i = spriteList.Count; i > 0; )
		{
			UIAtlas.Sprite sp = spriteList[--i];
			if (!kept.Contains(sp)) spriteList.RemoveAt(i);
		}

		// The material used by the atlas should now use the new texture
		atlas.material.mainTexture = newAtlasTexture;
		atlas.MarkAsDirty();
	}

	/// <summary>
	/// Replace the contents of the atlas with the specified group of sprites.
	/// </summary>

	void ReplaceAtlas (List<Texture> textures)
	{
		// Create a list of sprites using the collected textures
		List<SpriteEntry> sprites = CreateSprites(textures);

		if (sprites.Count > 0)
		{
			// The ability to undo this action is always useful
			Undo.RegisterUndo(mAtlas, "Replace Atlas");
			Undo.RegisterUndo(mAtlas.material, "Replace Atlas");

			// Create a new texture for the atlas, encode it into PNG format and destroy it
			Texture2D atlasTexture = PackTextures(sprites);
			byte[] bytes = atlasTexture.EncodeToPNG();
			NGUITools.Destroy(atlasTexture);

			// Save the PNG data
			string path = NGUIEditorTools.GetSaveableTexturePath(mAtlas);
			System.IO.File.WriteAllBytes(path, bytes);
			bytes = null;

			// Load the texture we just saved as a Texture2D
			AssetDatabase.Refresh();
			atlasTexture = NGUIEditorTools.ImportTexture(path, false, true);
			if (atlasTexture == null) { Debug.LogError("Failed to load the created atlas saved as " + path); return; }

			// Replace the sprites within the atlas and change its texture
			ReplaceAtlas(mAtlas, sprites, atlasTexture);

			// Bring up a confirmation dialog
			int result = EditorUtility.DisplayDialogComplex("Atlas Creation Result",
				sprites.Count + " textures were packed into a " + atlasTexture.width + "x" + atlasTexture.height + " atlas, saved as " + path,
				"OK", "Select the Atlas", "Select the Texture");

			// Select the object or the atlas if requested
			if		(result == 1) Selection.activeObject = mAtlas.gameObject;
			else if (result == 2) Selection.activeObject = atlasTexture;
		}

		// Release the temporary textures
		ReleaseSprites(sprites);
	}

	/// <summary>
	/// Update the sprites within the texture atlas, preserving the sprites that have not been selected.
	/// </summary>

	void UpdateAtlas (List<Texture> textures)
	{
		Debug.Log("TODO");
	}
}