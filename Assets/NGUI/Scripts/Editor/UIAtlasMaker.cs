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
	Vector2 mScroll = Vector2.zero;
	string mAtlasName = "New Atlas";
	string mDelName = null;

	void OnSelectAtlas (MonoBehaviour obj)
	{
		UIAtlas a = obj as UIAtlas;

		if (mAtlas != a)
		{
			mAtlas = a;
			mAtlasName = (mAtlas != null) ? mAtlas.name : "New Atlas";
			Save();
			Repaint();
		}
	}

	void Save ()
	{
		PlayerPrefs.SetInt("NGUI Atlas", (mAtlas != null) ? mAtlas.GetInstanceID() : -1);
		PlayerPrefs.SetString("NGUI Atlas Name", mAtlasName);
	}

	void Load ()
	{
		int atlasID = PlayerPrefs.GetInt("NGUI Atlas", -1);
		if (atlasID != -1) mAtlas = EditorUtility.InstanceIDToObject(atlasID) as UIAtlas;
		mAtlasName = PlayerPrefs.GetString("NGUI Atlas Name");
	}

	/// <summary>
	/// Refresh the window on selection.
	/// </summary>

	void OnSelectionChange () { mDelName = null; Repaint(); }

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
				if (tex != null && (mAtlas == null || mAtlas.texture != tex)) textures.Add(tex);
			}
		}
		return textures;
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
	/// Pack all of the specified sprites into a single texture, updating the outer and inner rects of the sprites as needed.
	/// </summary>

	static void PackTextures (Texture2D tex, List<SpriteEntry> sprites)
	{
		Texture2D[] textures = new Texture2D[sprites.Count];
		for (int i = 0; i < sprites.Count; ++i) textures[i] = sprites[i].tex;

		Rect[] rects = tex.PackTextures(textures, 1, 4096);

		for (int i = 0; i < sprites.Count; ++i)
		{
			sprites[i].rect = NGUIMath.ConvertToPixels(rects[i], tex.width, tex.height, true);
		}
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

		if (mAtlas != null)
		{
			List<string> spriteNames = mAtlas.GetListOfSprites();
			foreach (string sp in spriteNames) spriteList.Add(sp, 0);
		}

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
	/// Replace the sprites within the atlas.
	/// </summary>

	static void ReplaceSprites (UIAtlas atlas, List<SpriteEntry> sprites)
	{
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
		atlas.MarkAsDirty();
	}

	/// <summary>
	/// Extract sprites from the atlas, adding them to the list.
	/// </summary>

	static void ExtractSprites (UIAtlas atlas, List<SpriteEntry> sprites)
	{
		// Make the atlas texture readable
		Texture2D atlasTex = NGUIEditorTools.ImportTexture(atlas.texture, true, false);

		if (atlasTex != null)
		{
			atlas.coordinates = UIAtlas.Coordinates.Pixels;

			Color32[] oldPixels = null;
			int oldWidth = atlasTex.width;
			int oldHeight = atlasTex.height;

			foreach (UIAtlas.Sprite asp in atlas.sprites)
			{
				bool found = false;

				foreach (SpriteEntry se in sprites)
				{
					if (asp.name == se.tex.name)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					// Read the atlas
					if (oldPixels == null) oldPixels = atlasTex.GetPixels32();

					Rect rect = asp.outer;
					rect.xMin = Mathf.Clamp(rect.xMin, 0f, oldWidth);
					rect.yMin = Mathf.Clamp(rect.yMin, 0f, oldHeight);
					rect.xMax = Mathf.Clamp(rect.xMax, 0f, oldWidth);
					rect.yMax = Mathf.Clamp(rect.yMax, 0f, oldHeight);

					int newWidth = Mathf.RoundToInt(rect.width);
					int newHeight = Mathf.RoundToInt(rect.height);
					if (newWidth == 0 || newHeight == 0) continue;

					Color32[] newPixels = new Color32[newWidth * newHeight];
					int xmin = Mathf.RoundToInt(rect.x);
					int ymin = Mathf.RoundToInt(oldHeight - rect.yMax);

					for (int y = 0; y < newHeight; ++y)
					{
						for (int x = 0; x < newWidth; ++x)
						{
							int newIndex = y * newWidth + x;
							int oldIndex = (ymin + y) * oldWidth + (xmin + x);
							newPixels[newIndex] = oldPixels[oldIndex];
						}
					}

					// Create a new sprite
					SpriteEntry sprite = new SpriteEntry();
					sprite.temporaryTexture = true;
					sprite.tex = new Texture2D(newWidth, newHeight);
					sprite.tex.name = asp.name;
					sprite.rect = new Rect(0f, 0f, newWidth, newHeight);
					sprite.tex.SetPixels32(newPixels);

					// Min/max coordinates are in pixels
					sprite.minX = Mathf.RoundToInt(asp.paddingLeft * newWidth);
					sprite.maxX = Mathf.RoundToInt(asp.paddingRight * newWidth);
					sprite.minY = Mathf.RoundToInt(asp.paddingBottom * newHeight);
					sprite.maxY = Mathf.RoundToInt(asp.paddingTop * newHeight);

					sprites.Add(sprite);
				}
			}
		}

		// The atlas no longer needs to be readable
		NGUIEditorTools.ImportTexture(atlas.texture, false, false);
	}

	/// <summary>
	/// Combine all sprites into a single texture and save it to disk.
	/// </summary>

	static Texture2D UpdateTexture (UIAtlas atlas, List<SpriteEntry> sprites)
	{
		// Get the texture for the atlas
		Texture2D tex = atlas.texture as Texture2D;
		string path = NGUIEditorTools.GetSaveableTexturePath(atlas);

		if (tex == null)
		{
			// Create a new texture for the atlas
			tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);

			// Pack the sprites into this texture
			PackTextures(tex, sprites);
			byte[] bytes = tex.EncodeToPNG();

			// Save the PNG data
			System.IO.File.WriteAllBytes(path, bytes);
			bytes = null;

			// Load the texture we just saved as a Texture2D
			AssetDatabase.Refresh();
			path = NGUIEditorTools.GetSaveableTexturePath(atlas);
			tex = NGUIEditorTools.ImportTexture(path, false, true);
			if (tex == null) { Debug.LogError("Failed to load the created atlas saved as " + path); return null; }
			else atlas.material.mainTexture = tex;
		}
		else
		{
			// Make the atlas readable so we can save it
			NGUIEditorTools.ImportTexture(path, true, false);

			// Pack all sprites into atlas texture
			PackTextures(tex, sprites);
			byte[] bytes = tex.EncodeToPNG();
			System.IO.File.WriteAllBytes(path, bytes);
			bytes = null;

			// Reimport the atlas, making it no longer readable
			AssetDatabase.Refresh();
			NGUIEditorTools.ImportTexture(path, false, false);
		}
		return tex;
	}

	/// <summary>
	/// Update the sprite atlas, keeping only the sprites that are on the specified list.
	/// </summary>

	void UpdateAtlas (List<SpriteEntry> sprites)
	{
		if (sprites.Count > 0)
		{
			// Combine all sprites into a single texture and save it
			UpdateTexture(mAtlas, sprites);

			// Get the path to the texture
			NGUIEditorTools.GetSaveableTexturePath(mAtlas);

			// Replace the sprites within the atlas
			ReplaceSprites(mAtlas, sprites);

			// Release the temporary textures
			ReleaseSprites(sprites);
		}
		else
		{
			mAtlas.sprites.Clear();
			string path = NGUIEditorTools.GetSaveableTexturePath(mAtlas);
			mAtlas.material.mainTexture = null;
			if (!string.IsNullOrEmpty(path)) AssetDatabase.DeleteAsset(path);
		}
	}

	/// <summary>
	/// Update the sprites within the texture atlas, preserving the sprites that have not been selected.
	/// </summary>

	void UpdateAtlas (List<Texture> textures, bool keepSprites)
	{
		// Create a list of sprites using the collected textures
		List<SpriteEntry> sprites = CreateSprites(textures);

		if (sprites.Count > 0)
		{
			// Extract sprites from the atlas, filling in the missing pieces
			if (keepSprites) ExtractSprites(mAtlas, sprites);

			// NOTE: It doesn't seem to be possible to undo writing to disk, and there also seems to be no way of
			// detecting an Undo event. Without either of these it's not possible to restore the texture saved to disk,
			// so the undo process doesn't work right. Because of this I'd rather disable it altogether until a solution is found.

			// The ability to undo this action is always useful
			//if (mAtlas.texture != null) Undo.RegisterUndo(mAtlas.texture, "Update Atlas");
			//if (mAtlas.material != null) Undo.RegisterUndo(mAtlas.material, "Update Atlas");
			//Undo.RegisterUndo(mAtlas, "Update Atlas");

			// Update the atlas
			UpdateAtlas(sprites);
		}
	}

	/// <summary>
	/// Draw the UI for this tool.
	/// </summary>

	void OnGUI ()
	{
		if (mAtlas == null) Load();

		bool create = false;
		bool update = false;
		bool replace = false;

		string prefabPath = "";
		string matPath = "";

		// If we have an atlas to work with, see if we can figure out the path for it and its material
		if (mAtlas != null && mAtlas.name == mAtlasName)
		{
			prefabPath = AssetDatabase.GetAssetPath(mAtlas.gameObject.GetInstanceID());
			if (mAtlas.material != null) matPath = AssetDatabase.GetAssetPath(mAtlas.material.GetInstanceID());
		}

		// Assume default values if needed
		if (string.IsNullOrEmpty(mAtlasName)) mAtlasName = "New Atlas";
		if (string.IsNullOrEmpty(prefabPath)) prefabPath = "Assets/" + mAtlasName + ".prefab";
		if (string.IsNullOrEmpty(matPath)) matPath = "Assets/" + mAtlasName + ".mat";

		// Try to load the prefab
		GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

		EditorGUIUtility.LookLikeControls(80f);

		GUILayout.Space(6f);
		GUILayout.BeginHorizontal();

		if (go == null)
		{
			GUI.backgroundColor = Color.green;
			create = GUILayout.Button("Create", GUILayout.Width(76f));
		}
		else
		{
			GUI.backgroundColor = Color.red;
			create = GUILayout.Button("Replace", GUILayout.Width(76f));
		}

		GUI.backgroundColor = Color.white;
		string atlasName = GUILayout.TextField(mAtlasName);
		GUILayout.EndHorizontal();

		if (mAtlasName != atlasName)
		{
			mAtlasName = atlasName;
			Save();
		}

		if (create)
		{
			// If the prefab already exists, confirm that we want to overwrite it
			if (go == null || EditorUtility.DisplayDialog("Are you sure?", "This atlas already exists. Do you want to overwrite it?", "Yes", "No"))
			{
				replace = true;

				// Try to load the material
				Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;

				// If the material doesn't exist, create it
				if (mat == null)
				{
					Shader shader = Shader.Find("Unlit/Transparent Colored");
					mat = new Material(shader);

					// Save the material
					AssetDatabase.CreateAsset(mat, matPath);
					AssetDatabase.Refresh();

					// Load the material so it's usable
					mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
				}

				if (go == null)
				{
					// Create a new prefab for the atlas
					Object prefab = EditorUtility.CreateEmptyPrefab(prefabPath);

					// Create a new game object for the atlas
					go = new GameObject(mAtlasName);
					go.AddComponent<UIAtlas>().material = mat;

					// Update the prefab
					EditorUtility.ReplacePrefab(go, prefab);
					DestroyImmediate(go);
					AssetDatabase.Refresh();

					// Select the atlas
					go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
					mAtlas = go.GetComponent<UIAtlas>();
					Save();
				}
			}
		}

		ComponentSelector.Draw<UIAtlas>("...or select", mAtlas, OnSelectAtlas);

		List<Texture> textures = GetSelectedTextures();

		if (mAtlas != null)
		{
			if (textures.Count > 0)
			{
				GUI.backgroundColor = Color.green;
				update = GUILayout.Button("Add/Update All");
				GUI.backgroundColor = Color.white;
			}
			else
			{
				GUILayout.Label("You can reveal more options by selecting\none or more textures in the Project View\nwindow.");
			}
		}

		Dictionary<string, int> spriteList = GetSpriteList(textures);

		if (spriteList.Count > 0)
		{
			NGUIEditorTools.DrawHeader("Sprites");
			GUILayout.Space(-7f);

			mScroll = GUILayout.BeginScrollView(mScroll);

			string delSprite = null;
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
				else
				{
					if (string.IsNullOrEmpty(mDelName) || mDelName != iter.Key)
					{
						// If we have not yet selected a sprite for deletion, show a small "X" button
						if (GUILayout.Button("X", GUILayout.Width(22f))) mDelName = iter.Key;
					}
					else
					{
						GUI.backgroundColor = Color.red;

						if (GUILayout.Button("Delete", GUILayout.Width(60f)))
						{
							// Confirmation button clicked on -- delete this sprite
							delSprite = iter.Key;
							mDelName = null;
						}
						GUI.backgroundColor = Color.white;
					}
				}

				GUILayout.EndHorizontal();
				Rect rect = GUILayoutUtility.GetLastRect();
				GUI.Box(rect, "");
			}
			GUILayout.EndScrollView();

			// If this sprite was marked for deletion, remove it from the atlas
			if (!string.IsNullOrEmpty(delSprite))
			{
				foreach (UIAtlas.Sprite sp in mAtlas.sprites)
				{
					if (sp.name == delSprite)
					{
						mAtlas.sprites.Remove(sp);
						List<SpriteEntry> sprites = new List<SpriteEntry>();
						ExtractSprites(mAtlas, sprites);
						UpdateAtlas(sprites);
						mDelName = null;
						return;
					}
				}
			}
			else if (update) UpdateAtlas(textures, true);
			else if (replace) UpdateAtlas(textures, false);
			return;
		}
	}
}