using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Atlas maker lets you create atlases from a bunch of small textures. It's an alternative to using the external Texture Packer.
/// </summary>

public class UIAtlasMaker : EditorWindow
{
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
	/// Update the sprites within the texture atlas, preserving the sprites that have not been selected.
	/// </summary>

	void UpdateAtlas (List<Texture> textures)
	{
		Debug.Log("TODO");
	}

	/// <summary>
	/// Fix the import settings for the specified texture, re-importing it if necessary.
	/// </summary>

	Texture2D LoadTexture (string path, bool forInput, bool force)
	{
		if (!string.IsNullOrEmpty(path))
		{
			TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
			if (ti == null) return null;

			if (forInput)
			{
				if (force ||
					ti.mipmapEnabled ||
					!ti.isReadable ||
					ti.maxTextureSize < 4096 ||
					ti.filterMode != FilterMode.Point ||
					ti.wrapMode != TextureWrapMode.Clamp ||
					ti.npotScale != TextureImporterNPOTScale.None)
				{
					ti.mipmapEnabled = false;
					ti.isReadable = true;
					ti.maxTextureSize = 4096;
					ti.textureFormat = TextureImporterFormat.ARGB32;
					ti.filterMode = FilterMode.Point;
					ti.wrapMode = TextureWrapMode.Clamp;
					ti.npotScale = TextureImporterNPOTScale.None;
					AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
				}
			}
			else if (force || 
				!ti.mipmapEnabled ||
				ti.isReadable ||
				ti.maxTextureSize < 4096 ||
				ti.anisoLevel < 4 ||
				ti.filterMode != FilterMode.Trilinear ||
				ti.wrapMode != TextureWrapMode.Clamp ||
				ti.npotScale != TextureImporterNPOTScale.ToNearest)
			{
				ti.mipmapEnabled = true;
				ti.isReadable = false;
				ti.maxTextureSize = 4096;
				ti.textureFormat = TextureImporterFormat.ARGB32;
				ti.filterMode = FilterMode.Trilinear;
				ti.anisoLevel = 4;
				ti.wrapMode = TextureWrapMode.Clamp;
				ti.npotScale = TextureImporterNPOTScale.ToNearest;
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			}
			return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
		}
		return null;
	}

	/// <summary>
	/// Load the specified list of textures as Texture2Ds, fixing their import properties as necessary.
	/// </summary>

	List<Texture2D> LoadTextures (List<Texture> textures)
	{
		List<Texture2D> list = new List<Texture2D>();

		foreach (Texture tex in textures)
		{
			string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
			Texture2D t2 = LoadTexture(path, true, false);
			if (t2 != null) list.Add(t2);
		}
		return list;
	}

	/// <summary>
	/// Calculate the new inner rect, given the old inner rect, as well as the before and after outer rects.
	/// </summary>

	static Rect UpdateInnerRect (Rect inner, Rect outerBefore, Rect outerNow)
	{
		float offsetX = inner.xMin - outerBefore.xMin;
		float offsetY = inner.yMin - outerBefore.yMin;
		float sizeX = inner.width;
		float sizeY = inner.height;

		if (Mathf.Approximately(outerNow.width, outerBefore.width))
		{
			// The sprite has not been rotated or it's a square
			return new Rect(outerNow.xMin + offsetX, outerNow.yMin + offsetY, sizeX, sizeY);
		}
		else if (Mathf.Approximately(outerNow.width, outerBefore.height))
		{
			// The sprite was rotated since the last time it was imported
			return new Rect(outerNow.xMin + offsetY, outerNow.yMin + offsetX, sizeY, sizeX);
		}

		Debug.LogWarning("No match: " + inner + " " + outerBefore + " " + outerNow);
		return outerNow;
	}

	/// <summary>
	/// Add a new sprite to the atlas, given the texture it's coming from and the packed rect within the atlas.
	/// </summary>

	static UIAtlas.Sprite AddSprite (List<UIAtlas.Sprite> sprites, Texture2D tex, Rect rect)
	{
		UIAtlas.Sprite sprite = null;

		// See if this sprite already exists
		foreach (UIAtlas.Sprite sp in sprites)
		{
			if (sp.name == tex.name)
			{
				sprite = sp;
				break;
			}
		}

		if (sprite != null)
		{
			sprite.inner = UpdateInnerRect(sprite.inner, sprite.outer, rect);
			sprite.outer = rect;
		}
		else
		{
			sprite = new UIAtlas.Sprite();
			sprite.name = tex.name;
			sprite.outer = rect;
			sprite.inner = rect;
			sprites.Add(sprite);
		}
		return sprite;
	}

	/// <summary>
	/// Replace the contents of the atlas with the specified group of sprites.
	/// </summary>

	void ReplaceAtlas (List<Texture> textures)
	{
		// Load all textures as Texture2Ds
		List<Texture2D> textureList = LoadTextures(textures);

		if (textureList.Count > 0)
		{
			// The ability to undo this action is always useful
			Undo.RegisterUndo(mAtlas, "Replace Atlas");
			Undo.RegisterUndo(mAtlas.material, "Replace Atlas");

			// Path where the texture atlas will be saved
			string path = "Assets/" + mAtlas.name + ".png";

			// If the atlas already has a texture, overwrite its texture
			if (mAtlas.texture != null)
			{
				Undo.RegisterUndo(mAtlas.texture, "Replace Atlas");
				path = AssetDatabase.GetAssetPath(mAtlas.texture.GetInstanceID());
				int dot = path.LastIndexOf('.');
				path = path.Substring(0, dot) + ".png";
			}

			// Create a new texture for the atlas
			Texture2D atlasTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

			// Pack the textures into the texture atlas
			Rect[] rects = atlasTexture.PackTextures(textureList.ToArray(), 1, 4096);

			// Encode the atlas into a PNG we can save
			byte[] bytes = atlasTexture.EncodeToPNG();

			// The temporary texture is no longer needed
			NGUITools.Destroy(atlasTexture);

			// Save the PNG to the disk
			System.IO.File.WriteAllBytes(path, bytes);
			bytes = null;

			// Load the texture we just saved as a Texture2D
			atlasTexture = LoadTexture(path, false, true);

			if (atlasTexture == null)
			{
				Debug.LogError("Failed to load the created atlas saved as " + path);
				return;
			}

			// Get the list of sprites we'll be updating
			List<UIAtlas.Sprite> sprites = mAtlas.sprites;
			List<UIAtlas.Sprite> kept = new List<UIAtlas.Sprite>();

			// The atlas must be in pixels
			mAtlas.coordinates = UIAtlas.Coordinates.Pixels;

			// Run through all the textures we added and add them as sprites to the atlas
			for (int i = 0; i < textureList.Count; ++i)
			{
				Rect rect = rects[i];
				rect = NGUIMath.ConvertToPixels(rect, atlasTexture.width, atlasTexture.height, true);
				UIAtlas.Sprite sprite = AddSprite(sprites, textureList[i], rect);
				kept.Add(sprite);
			}

			// Remove unused sprites
			for (int i = sprites.Count; i > 0; )
			{
				UIAtlas.Sprite sp = sprites[--i];
				if (!kept.Contains(sp)) sprites.RemoveAt(i);
			}

			// The material used by the atlas should use the new texture we created
			mAtlas.material.mainTexture = atlasTexture;

			Debug.Log(textureList.Count + " textures were packed into a " + atlasTexture.width + "x" + atlasTexture.height + " atlas, saved as " + path);
			Selection.activeObject = atlasTexture;
		}
	}
}