//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Font maker lets you create font prefabs with a single click of a button.
/// </summary>

public class UIFontMaker : EditorWindow
{
	enum FontType
	{
		//GeneratedBitmap,	// Bitmap font, created from a dynamic font using FreeType
		ImportedBitmap,		// Imported bitmap font, created using BMFont or another external tool
		Dynamic,			// Dynamic font, used as-is
	}

	enum Create
	{
		None,
		Bitmap,		// Bitmap font, created from a dynamic font using FreeType
		Import,		// Imported bitmap font
		Dynamic,	// Dynamic font, used as-is
	}

	FontType mType = FontType.ImportedBitmap;

	/// <summary>
	/// Update all labels associated with this font.
	/// </summary>

	void MarkAsChanged ()
	{
		if (NGUISettings.ambigiousFont != null)
		{
			List<UILabel> labels = NGUIEditorTools.FindAll<UILabel>();

			foreach (UILabel lbl in labels)
			{
				if (lbl.ambigiousFont == NGUISettings.ambigiousFont)
				{
					lbl.ambigiousFont = null;
					lbl.ambigiousFont = NGUISettings.ambigiousFont;
				}
			}
		}
	}

	/// <summary>
	/// Atlas selection callback.
	/// </summary>

	void OnSelectAtlas (Object obj)
	{
		NGUISettings.atlas = obj as UIAtlas;
		Repaint();
	}

	/// <summary>
	/// Refresh the window on selection.
	/// </summary>

	void OnSelectionChange () { Repaint(); }
	void OnUnityFont (Object obj) { NGUISettings.ambigiousFont = obj; }

	/// <summary>
	/// Draw the UI for this tool.
	/// </summary>

	void OnGUI ()
	{
		Object fnt = NGUISettings.ambigiousFont;
		UIFont bf = (fnt as UIFont);

		NGUIEditorTools.SetLabelWidth(80f);
		GUILayout.Space(3f);

		NGUIEditorTools.DrawHeader("Input", true);
		NGUIEditorTools.BeginContents();

		GUILayout.BeginHorizontal();
		mType = (FontType)EditorGUILayout.EnumPopup("Type", mType, GUILayout.MinWidth(200f));
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();
		Create create = Create.None;

		if (mType == FontType.ImportedBitmap)
		{
			NGUISettings.fontData = EditorGUILayout.ObjectField("Font Data", NGUISettings.fontData, typeof(TextAsset), false) as TextAsset;
			NGUISettings.fontTexture = EditorGUILayout.ObjectField("Texture", NGUISettings.fontTexture, typeof(Texture2D), false, GUILayout.Width(140f)) as Texture2D;
			NGUIEditorTools.EndContents();

			// Draw the atlas selection only if we have the font data and texture specified, just to make it easier
			EditorGUI.BeginDisabledGroup(NGUISettings.fontData == null || NGUISettings.fontTexture == null);
			{
				NGUIEditorTools.DrawHeader("Output", true);
				NGUIEditorTools.BeginContents();
				ComponentSelector.Draw<UIAtlas>(NGUISettings.atlas, OnSelectAtlas, false);
				NGUIEditorTools.EndContents();
			}
			EditorGUI.EndDisabledGroup();

			if (NGUISettings.fontData == null)
			{
				EditorGUILayout.HelpBox("To create a font from a previously exported FNT file, you need to use BMFont on " +
					"Windows or your choice of Glyph Designer or the less expensive bmGlyph on the Mac.\n\n" +
					"Either of these tools will create a FNT file for you that you will drag & drop into the field above.", MessageType.Info);
			}
			else if (NGUISettings.fontTexture == null)
			{
				EditorGUILayout.HelpBox("When exporting your font, you should get two files: the FNT, and the texture. Only one texture can be used per font.", MessageType.Info);
			}
			else if (NGUISettings.atlas == null)
			{
				EditorGUILayout.HelpBox("You can create a font that doesn't use a texture atlas. This will mean that the text " +
					"labels using this font will generate an extra draw call.\n\nIf you do specify an atlas, the font's texture will be added to it automatically.", MessageType.Info);
			}

			EditorGUI.BeginDisabledGroup(NGUISettings.fontData == null || NGUISettings.fontTexture == null);
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				if (GUILayout.Button("Create the Font")) create = Create.Import;
				GUILayout.Space(20f);
				GUILayout.EndHorizontal();
			}
			EditorGUI.EndDisabledGroup();
		}
		else
		{
			GUILayout.BeginHorizontal();
			if (NGUIEditorTools.DrawPrefixButton("Source"))
				ComponentSelector.Show<Font>(OnUnityFont, new string[] { ".ttf", ".otf" });
			Font ttf = NGUISettings.ambigiousFont as Font;
			NGUISettings.ambigiousFont = EditorGUILayout.ObjectField(ttf, typeof(Font), false) as Font;
			ttf = NGUISettings.ambigiousFont as Font;
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			NGUISettings.fontSize = EditorGUILayout.IntField("Size", NGUISettings.fontSize, GUILayout.Width(120f));
			NGUISettings.fontStyle = (FontStyle)EditorGUILayout.EnumPopup(NGUISettings.fontStyle);
			GUILayout.Space(18f);
			GUILayout.EndHorizontal();
			NGUIEditorTools.EndContents();

			if (mType == FontType.Dynamic)
			{
				EditorGUI.BeginDisabledGroup(ttf == null);
				GUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				if (GUILayout.Button("Create the Font")) create = Create.Dynamic;
				GUILayout.Space(20f);
				GUILayout.EndHorizontal();
				EditorGUI.EndDisabledGroup();
#if UNITY_3_5
				EditorGUILayout.HelpBox("Dynamic fonts require Unity 4.0 or higher.", MessageType.Error);
#else
				// Helpful info
				if (ttf == null)
				{
					EditorGUILayout.HelpBox("You don't have to create a UIFont to use dynamic fonts. You can just reference the Unity Font directly on the label.", MessageType.Info);
				}
				EditorGUILayout.HelpBox("Please note that dynamic fonts can't be made a part of an atlas, and using dynamic fonts will result in at least one extra draw call.", MessageType.Warning);
#endif
			}
			else
			{
				// Draw the atlas selection only if we have the font data and texture specified, just to make it easier
				EditorGUI.BeginDisabledGroup(ttf == null);
				{
					NGUIEditorTools.DrawHeader("Output", true);
					NGUIEditorTools.BeginContents();
					ComponentSelector.Draw<UIAtlas>(NGUISettings.atlas, OnSelectAtlas, false);
					NGUIEditorTools.EndContents();

					if (ttf == null)
					{
						EditorGUILayout.HelpBox("You can create a bitmap font by specifying a dynamic font to use as the source.", MessageType.Info);
					}
					else if (NGUISettings.atlas == null)
					{
						EditorGUILayout.HelpBox("You can create a font that doesn't use a texture atlas. This will mean that the text " +
							"labels using this font will generate an extra draw call.\n\nIf you do specify an atlas, the font's texture will be added to it automatically.", MessageType.Info);
					}

					GUILayout.BeginHorizontal();
					GUILayout.Space(20f);
					if (GUILayout.Button("Create the Font")) create = Create.Bitmap;
					GUILayout.Space(20f);
					GUILayout.EndHorizontal();
				}
				EditorGUI.EndDisabledGroup();
			}
		}

		if (create == Create.None) return;

		// Open the "Save As" file dialog
		string prefabPath = EditorUtility.SaveFilePanelInProject("Save As", "New Font.prefab", "prefab", "Save font as...");
		if (string.IsNullOrEmpty(prefabPath)) return;

		// Try to load the material
		Material mat = null;
				
		if (create != Create.Dynamic)
		{
			//if (create == Create.Bitmap)
			//{
			//	// Create the bitmap font
			//	BMFont bmf = FreeType.CreateFont(NGUISettings.dynamicFont, NGUISettings.fontSize, NGUISettings.fontStyle);
			//	Debug.Log(bmf != null);
			//	return;
			//}

			if (NGUISettings.atlas != null)
			{
				// Add the font's texture to the atlas
				UIAtlasMaker.AddOrUpdate(NGUISettings.atlas, NGUISettings.fontTexture);
			}
			else
			{
				// Create a material for the font
				string matPath = prefabPath.Replace(".prefab", ".mat");
				mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;

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
				mat.mainTexture = NGUISettings.fontTexture;
			}
		}

		// Load the font's prefab
		GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

		// Font doesn't exist yet
		if (go == null || go.GetComponent<UIFont>() == null)
		{
			// Create a new prefab for the atlas
			Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);

			string fontName = prefabPath.Replace(".prefab", "");
			fontName = fontName.Substring(prefabPath.LastIndexOfAny(new char[] { '/', '\\' }) + 1);

			// Create a new game object for the font
			go = new GameObject(fontName);
			bf = go.AddComponent<UIFont>();
			CreateFont(bf, create, mat);

			// Update the prefab
			PrefabUtility.ReplacePrefab(go, prefab);
			DestroyImmediate(go);
			AssetDatabase.Refresh();

			// Select the atlas
			go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
			bf = go.GetComponent<UIFont>();
			NGUISettings.ambigiousFont = bf;
		}
		else
		{
			bf = go.GetComponent<UIFont>();
			CreateFont(bf, create, mat);
			NGUISettings.ambigiousFont = bf;
		}
		MarkAsChanged();

		Selection.activeGameObject = go;
	}

	static void CreateFont (UIFont font, Create create, Material mat)
	{
		if (create == Create.Dynamic)
		{
			// New dynamic font
			font.atlas = null;
			font.dynamicFont = NGUISettings.dynamicFont;
			font.dynamicFontStyle = NGUISettings.fontStyle;
			font.defaultSize = NGUISettings.fontSize;
		}
		else if (create == Create.Import)
		{
			// New bitmap font
			font.dynamicFont = null;
			BMFontReader.Load(font.bmFont, NGUITools.GetHierarchy(font.gameObject), NGUISettings.fontData.bytes);

			if (NGUISettings.atlas == null)
			{
				font.atlas = null;
				font.material = mat;
			}
			else
			{
				font.spriteName = NGUISettings.fontTexture.name;
				font.atlas = NGUISettings.atlas;
			}

			NGUISettings.fontSize = font.defaultSize;
		}
	}
}
