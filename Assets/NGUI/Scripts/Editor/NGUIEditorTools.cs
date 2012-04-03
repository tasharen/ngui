//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tools for the editor
/// </summary>

public class NGUIEditorTools
{
	static Texture2D mWhiteTex;
	static Texture2D mBackdropTex;
	static Texture2D mContrastTex;
	static Texture2D mGradientTex;

	/// <summary>
	/// Returns a blank usable 1x1 white texture.
	/// </summary>

	static public Texture2D blankTexture
	{
		get
		{
			if (mWhiteTex == null) mWhiteTex = CreateDummyTex();
			return mWhiteTex;
		}
	}

	/// <summary>
	/// Returns a usable texture that looks like a dark checker board.
	/// </summary>

	static public Texture2D backdropTexture
	{
		get
		{
			if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(
				new Color(0.1f, 0.1f, 0.1f, 0.5f),
				new Color(0.2f, 0.2f, 0.2f, 0.5f));
			return mBackdropTex;
		}
	}

	/// <summary>
	/// Returns a usable texture that looks like a high-contrast checker board.
	/// </summary>

	static public Texture2D contrastTexture
	{
		get
		{
			if (mContrastTex == null) mContrastTex = CreateCheckerTex(
				new Color(0f, 0.0f, 0f, 0.5f),
				new Color(1f, 1f, 1f, 0.5f));
			return mContrastTex;
		}
	}

	/// <summary>
	/// Gradient texture is used for title bars / headers.
	/// </summary>

	static public Texture2D gradientTexture
	{
		get
		{
			if (mGradientTex == null) mGradientTex = CreateGradientTex();
			return mGradientTex;
		}
	}

	/// <summary>
	/// Create a white dummy texture.
	/// </summary>

	static Texture2D CreateDummyTex ()
	{
		Texture2D tex = new Texture2D(1, 1);
		tex.name = "[Generated] Dummy Texture";
		tex.SetPixel(0, 0, Color.white);
		tex.Apply();
		tex.filterMode = FilterMode.Point;
		return tex;
	}

	/// <summary>
	/// Create a checker-background texture
	/// </summary>

	static Texture2D CreateCheckerTex (Color c0, Color c1)
	{
		Texture2D tex = new Texture2D(16, 16);
		tex.name = "[Generated] Checker Texture";

		for (int y = 0; y < 8;  ++y) for (int x = 0; x < 8;  ++x) tex.SetPixel(x, y, c1);
		for (int y = 8; y < 16; ++y) for (int x = 0; x < 8;  ++x) tex.SetPixel(x, y, c0);
		for (int y = 0; y < 8;  ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
		for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

		tex.Apply();
		tex.filterMode = FilterMode.Point;
		return tex;
	}

	/// <summary>
	/// Create a gradient texture
	/// </summary>

	static Texture2D CreateGradientTex ()
	{
		Texture2D tex = new Texture2D(1, 16);
		tex.name = "[Generated] Gradient Texture";

		Color c0 = new Color(1f, 1f, 1f, 0f);
		Color c1 = new Color(1f, 1f, 1f, 0.4f);

		for (int i = 0; i < 16; ++i)
		{
			float f = Mathf.Abs((i / 15f) * 2f - 1f);
			f *= f;
			tex.SetPixel(0, i, Color.Lerp(c0, c1, f));
		}

		tex.Apply();
		tex.filterMode = FilterMode.Bilinear;
		return tex;
	}

	/// <summary>
	/// Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
	/// </summary>

	static public void DrawTiledTexture (Rect rect, Texture tex)
	{
		GUI.BeginGroup(rect);
		{
			int width  = Mathf.RoundToInt(rect.width);
			int height = Mathf.RoundToInt(rect.height);

			for (int y = 0; y < height; y += tex.height)
			{
				for (int x = 0; x < width; x += tex.width)
				{
					GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
				}
			}
		}
		GUI.EndGroup();
	}

	/// <summary>
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = contrastTexture;
			GUI.color = Color.white;
			DrawTiledTexture(new Rect(rect.xMin, rect.yMax, 1f, -rect.height), tex);
			DrawTiledTexture(new Rect(rect.xMax, rect.yMax, 1f, -rect.height), tex);
			DrawTiledTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			DrawTiledTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
		}
	}

	/// <summary>
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			GUI.color = color;
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
			GUI.color = Color.white;
		}
	}

	/// <summary>
	/// Draw a selection outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect relative, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			// Calculate where the outer rectangle would be
			float x = rect.xMin + rect.width * relative.xMin;
			float y = rect.yMax - rect.height * relative.yMin;
			float width = rect.width * relative.width;
			float height = -rect.height * relative.height;
			relative = new Rect(x, y, width, height);

			// Draw the selection
			DrawOutline(relative, color);
		}
	}

	/// <summary>
	/// Draw a selection outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect relative)
	{
		if (Event.current.type == EventType.Repaint)
		{
			// Calculate where the outer rectangle would be
			float x = rect.xMin + rect.width * relative.xMin;
			float y = rect.yMax - rect.height * relative.yMin;
			float width = rect.width * relative.width;
			float height = -rect.height * relative.height;
			relative = new Rect(x, y, width, height);

			// Draw the selection
			DrawOutline(relative);
		}
	}

	/// <summary>
	/// Draw a 9-sliced outline.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect outer, Rect inner)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Color green = new Color(0.4f, 1f, 0f, 1f);

			DrawOutline(rect, new Rect(outer.x, inner.y, outer.width, inner.height));
			DrawOutline(rect, new Rect(inner.x, outer.y, inner.width, outer.height));
			DrawOutline(rect, outer, green);
		}
	}

	/// <summary>
	/// Draw a checkered background for the specified texture.
	/// </summary>

	static Rect DrawBackground (Texture2D tex, float ratio)
	{
		Rect rect = GUILayoutUtility.GetRect(0f, 0f);
		rect.width = Screen.width - rect.xMin;
		rect.height = rect.width * ratio;
		GUILayout.Space(rect.height);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D blank = blankTexture;
			Texture2D check = backdropTexture;

			// Lines above and below the texture rectangle
			GUI.color = new Color(0f, 0f, 0f, 0.2f);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin - 1, rect.width, 1f), blank);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), blank);
			GUI.color = Color.white;

			// Checker background
			DrawTiledTexture(rect, check);
		}
		return rect;
	}

	/// <summary>
	/// Draw a texture atlas, complete with a background texture and an outline.
	/// </summary>

	static public Rect DrawAtlas (Texture2D tex, Material mat)
	{
		Rect rect = DrawBackground(tex, (float)tex.height / tex.width);

		if (Event.current.type == EventType.Repaint)
		{
			if (mat == null)
			{
				GUI.DrawTexture(rect, tex);
			}
			else
			{
				UnityEditor.EditorGUI.DrawPreviewTexture(rect, tex, mat);
			}
		}
		return rect;
	}

	/// <summary>
	/// Draw an enlarged sprite within the specified texture atlas.
	/// </summary>

	static public Rect DrawSprite (Texture2D tex, Rect sprite, Material mat)
	{
		float paddingX = 4f / tex.width;
		float paddingY = 4f / tex.height;
		float ratio = (sprite.height + paddingY) / (sprite.width + paddingX);

		ratio *= (float)tex.height / tex.width;

		// Draw the checkered background
		Rect rect = DrawBackground(tex, ratio);

		// We only want to draw into this rectangle
		GUI.BeginGroup(rect);
		{
			if (Event.current.type == EventType.Repaint)
			{
				// We need to calculate where to begin and how to stretch the texture
				// for it to appear properly scaled in the rectangle
				float scaleX = rect.width / (sprite.width + paddingX);
				float scaleY = rect.height / (sprite.height + paddingY);
				float ox = scaleX * (sprite.x - paddingX * 0.5f);
				float oy = scaleY * (1f - (sprite.yMax + paddingY * 0.5f));

				Rect drawRect = new Rect(-ox, -oy, scaleX, scaleY);

				if (mat == null)
				{
					GUI.DrawTexture(drawRect, tex);
				}
				else
				{
					// NOTE: DrawPreviewTexture doesn't seem to support BeginGroup-based clipping
					// when a custom material is specified. It seems to be a bug in Unity.
					// Passing 'null' for the material or omitting the parameter clips as expected.
					UnityEditor.EditorGUI.DrawPreviewTexture(drawRect, tex, mat);
					//UnityEditor.EditorGUI.DrawPreviewTexture(drawRect, tex);
					//GUI.DrawTexture(drawRect, tex);
				}
				rect = new Rect(drawRect.x + rect.x, drawRect.y + rect.y, drawRect.width, drawRect.height);
			}
		}
		GUI.EndGroup();
		return rect;
	}

	/// <summary>
	/// Draw a visible separator in addition to adding some padding.
	/// </summary>

	static public void DrawSeparator ()
	{
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
	}

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public Rect DrawHeader (string text)
	{
		GUILayout.Space(28f);
		Rect rect = GUILayoutUtility.GetLastRect();
		rect.yMin += 5f;
		rect.yMax -= 4f;
		rect.width = Screen.width;

		if (Event.current.type == EventType.Repaint)
		{
			GUI.color = Color.black;
			GUI.DrawTexture(new Rect(0f, rect.yMin, Screen.width, rect.yMax - rect.yMin), gradientTexture);
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin, Screen.width, 1f), blankTexture);
			GUI.DrawTexture(new Rect(0f, rect.yMax - 1, Screen.width, 1f), blankTexture);
			GUI.color = Color.white;
			GUI.Label(new Rect(rect.x + 4f, rect.y, rect.width - 4, rect.height), text, EditorStyles.boldLabel);
		}
		return rect;
	}

	/// <summary>
	/// Draw a simple box outline for the entire line.
	/// </summary>

	static public void HighlightLine (Color c)
	{
		Rect rect = GUILayoutUtility.GetRect(Screen.width - 16f, 22f);
		GUILayout.Space(-23f);
		c.a *= 0.3f;
		GUI.color = c;
		GUI.DrawTexture(rect, gradientTexture);
		c.r *= 0.5f;
		c.g *= 0.5f;
		c.b *= 0.5f;
		GUI.color = c;
		GUI.DrawTexture(new Rect(rect.x, rect.y + 1f, rect.width, 1f), blankTexture);
		GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - 1f, rect.width, 1f), blankTexture);
		GUI.color = Color.white;
	}

	/// <summary>
	/// Convenience function that displays a list of sprites and returns the selected value.
	/// </summary>

	static public string DrawList (string field, string[] list, string selection, params GUILayoutOption[] options)
	{
		if (list != null && list.Length > 0)
		{
			int index = 0;
			if (string.IsNullOrEmpty(selection)) selection = list[0];

			// We need to find the sprite in order to have it selected
			if (!string.IsNullOrEmpty(selection))
			{
				for (int i = 0; i < list.Length; ++i)
				{
					if (selection.Equals(list[i], System.StringComparison.OrdinalIgnoreCase))
					{
						index = i;
						break;
					}
				}
			}

			// Draw the sprite selection popup
			index = EditorGUILayout.Popup(field, index, list, options);
			return list[index];
		}
		return null;
	}

	/// <summary>
	/// Helper function that returns the selected root object.
	/// </summary>

	static public GameObject SelectedRoot ()
	{
		GameObject go = Selection.activeGameObject;

		// Only use active objects
		if (go != null && !go.active) go = null;

		// Try to find a panel
		UIPanel p = (go != null) ? NGUITools.FindInParents<UIPanel>(go) : null;

		// No selection? Try to find the root automatically
		if (p == null)
		{
			UIPanel[] panels = GameObject.FindSceneObjectsOfType(typeof(UIPanel)) as UIPanel[];
			if (panels.Length > 0) go = panels[0].gameObject;
		}

		// Now find the first uniformly scaled object
		if (go != null)
		{
			Transform t = go.transform;

			// Find the first uniformly scaled object
			while (!Mathf.Approximately(t.localScale.x, t.localScale.y) ||
				   !Mathf.Approximately(t.localScale.x, t.localScale.z))
			{
				t = t.parent;
				if (t == null) return (p != null) ? p.gameObject : null;
				else go = t.gameObject;
			}
		}
		return go;
	}

	/// <summary>
	/// Helper function that checks to see if this action would break the prefab connection.
	/// </summary>

	static public bool WillLosePrefab (GameObject root)
	{
		if (root == null) return false;

		if (root.transform != null)
		{
			// Check if the selected object is a prefab instance and display a warning
#if UNITY_3_4
			PrefabType type = EditorUtility.GetPrefabType(root);
#else
			PrefabType type = PrefabUtility.GetPrefabType(root);
#endif
			if (type == PrefabType.PrefabInstance)
			{
				return EditorUtility.DisplayDialog("Losing prefab",
					"This action will lose the prefab connection. Are you sure you wish to continue?",
					"Continue", "Cancel");
			}
		}
		return true;
	}

	/// <summary>
	/// Change the import settings of the specified texture asset, making it readable.
	/// </summary>

	static bool MakeTextureReadable (string path, bool force)
	{
		if (string.IsNullOrEmpty(path)) return false;
		TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
		if (ti == null) return false;

		TextureImporterSettings settings = new TextureImporterSettings();
		ti.ReadTextureSettings(settings);

		if (force ||
			settings.mipmapEnabled ||
			!settings.readable ||
			settings.maxTextureSize < 4096 ||
			settings.filterMode != FilterMode.Point ||
			settings.wrapMode != TextureWrapMode.Clamp ||
			settings.npotScale != TextureImporterNPOTScale.None)
		{
			settings.mipmapEnabled = false;
			settings.readable = true;
			settings.maxTextureSize = 4096;
			settings.textureFormat = TextureImporterFormat.RGBA32;
			settings.filterMode = FilterMode.Point;
			settings.wrapMode = TextureWrapMode.Clamp;
			settings.npotScale = TextureImporterNPOTScale.None;
			
			ti.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		}
		return true;
	}

	/// <summary>
	/// Change the import settings of the specified texture asset, making it suitable to be used as a texture atlas.
	/// </summary>

	static bool MakeTextureAnAtlas (string path, bool force)
	{
		if (string.IsNullOrEmpty(path)) return false;
		TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
		if (ti == null) return false;

		TextureImporterSettings settings = new TextureImporterSettings();
		ti.ReadTextureSettings(settings);

		if (force ||
			settings.readable ||
			settings.maxTextureSize < 4096 ||
			settings.wrapMode != TextureWrapMode.Clamp ||
			settings.npotScale != TextureImporterNPOTScale.ToNearest)
		{
			settings.mipmapEnabled = true;
			settings.readable = false;
			settings.maxTextureSize = 4096;
			settings.textureFormat = TextureImporterFormat.RGBA32;
			settings.filterMode = FilterMode.Trilinear;
			settings.aniso = 4;
			settings.wrapMode = TextureWrapMode.Clamp;
			settings.npotScale = TextureImporterNPOTScale.ToNearest;
			
			ti.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		}
		return true;
	}

	/// <summary>
	/// Fix the import settings for the specified texture, re-importing it if necessary.
	/// </summary>

	static public Texture2D ImportTexture (string path, bool forInput, bool force)
	{
		if (!string.IsNullOrEmpty(path))
		{
			if (forInput) { if (!MakeTextureReadable(path, force)) return null; }
			else if (!MakeTextureAnAtlas(path, force)) return null;
			return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
		}
		return null;
	}

	/// <summary>
	/// Fix the import settings for the specified texture, re-importing it if necessary.
	/// </summary>

	static public Texture2D ImportTexture (Texture tex, bool forInput, bool force)
	{
		if (tex != null)
		{
			string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
			return ImportTexture(path, forInput, force);
		}
		return null;
	}

	/// <summary>
	/// Figures out the saveable filename for the texture of the specified atlas.
	/// </summary>

	static public string GetSaveableTexturePath (UIAtlas atlas)
	{
		// Path where the texture atlas will be saved
		string path = "";

		// If the atlas already has a texture, overwrite its texture
		if (atlas.texture != null)
		{
			path = AssetDatabase.GetAssetPath(atlas.texture.GetInstanceID());

			if (!string.IsNullOrEmpty(path))
			{
				int dot = path.LastIndexOf('.');
				return path.Substring(0, dot) + ".png";
			}
		}

		// No texture to use -- figure out a name using the atlas
		path = AssetDatabase.GetAssetPath(atlas.GetInstanceID());
		path = string.IsNullOrEmpty(path) ? "Assets/" + atlas.name + ".png" : path.Replace(".prefab", ".png");
		return path;
	}

	/// <summary>
	/// Helper function that returns the folder where the current selection resides.
	/// </summary>

	static public string GetSelectionFolder ()
	{
		if (Selection.activeObject != null)
		{
			string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());

			if (!string.IsNullOrEmpty(path))
			{
				int dot = path.LastIndexOf('.');
				int slash = Mathf.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
				if (slash > 0) return (dot > slash) ? path.Substring(0, slash + 1) : path + "/";
			}
		}
		return "Assets/";
	}

	/// <summary>
	/// Struct type for the integer vector field below.
	/// </summary>

	public struct IntVector
	{
		public int x;
		public int y;
	}

	/// <summary>
	/// Integer vector field.
	/// </summary>

	static public IntVector IntPair (string prefix, string leftCaption, string rightCaption, int x, int y)
	{
		GUILayout.BeginHorizontal();

		if (string.IsNullOrEmpty(prefix))
		{
			GUILayout.Space(82f);
		}
		else
		{
			GUILayout.Label(prefix, GUILayout.Width(74f));
		}

		EditorGUIUtility.LookLikeControls(48f);

		IntVector retVal;
		retVal.x = EditorGUILayout.IntField(leftCaption, x, GUILayout.MinWidth(30f));
		retVal.y = EditorGUILayout.IntField(rightCaption, y, GUILayout.MinWidth(30f));

		EditorGUIUtility.LookLikeControls(80f);

		GUILayout.EndHorizontal();
		return retVal;
	}

	/// <summary>
	/// Integer rectangle field.
	/// </summary>

	static public Rect IntRect (string prefix, Rect rect)
	{
		int left	= Mathf.RoundToInt(rect.xMin);
		int top		= Mathf.RoundToInt(rect.yMin);
		int width	= Mathf.RoundToInt(rect.width);
		int height	= Mathf.RoundToInt(rect.height);

		NGUIEditorTools.IntVector a = NGUIEditorTools.IntPair(prefix, "Left", "Top", left, top);
		NGUIEditorTools.IntVector b = NGUIEditorTools.IntPair(null, "Width", "Height", width, height);

		return new Rect(a.x, a.y, b.x, b.y);
	}

	/// <summary>
	/// Integer vector field.
	/// </summary>

	static public Vector4 IntPadding (string prefix, Vector4 v)
	{
		int left	= Mathf.RoundToInt(v.x);
		int top		= Mathf.RoundToInt(v.y);
		int right	= Mathf.RoundToInt(v.z);
		int bottom	= Mathf.RoundToInt(v.w);

		NGUIEditorTools.IntVector a = NGUIEditorTools.IntPair(prefix, "Left", "Top", left, top);
		NGUIEditorTools.IntVector b = NGUIEditorTools.IntPair(null, "Right", "Bottom", right, bottom);

		return new Vector4(a.x, a.y, b.x, b.y);
	}

	/// <summary>
	/// Create an undo point for the specified objects.
	/// This action also marks the object as dirty so prefabs work correctly in 3.5.0 (work-around for a bug in Unity).
	/// </summary>

	static public void RegisterUndo (string name, params Object[] objects)
	{
		if (objects != null && objects.Length > 0)
		{
			foreach (Object obj in objects)
			{
				if (obj == null) continue;
				Undo.RegisterUndo(obj, name);
				EditorUtility.SetDirty(obj);
			}
		}
		else
		{
			Undo.RegisterSceneUndo(name);
		}
	}

	/// <summary>
	/// Find all scene components, active or inactive.
	/// </summary>

	static public List<T> FindInScene<T> () where T : Component
	{
		T[] comps = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];

		List<T> list = new List<T>();

		foreach (T comp in comps)
		{
			if (comp.gameObject.hideFlags == 0)
			{
				string path = AssetDatabase.GetAssetPath(comp.gameObject);
				if (string.IsNullOrEmpty(path)) list.Add(comp);
			}
		}
		return list;
	}
}