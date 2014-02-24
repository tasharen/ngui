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
		Bitmap,
		Dynamic,
		//Import,
	}

	FontType mType = FontType.Bitmap;

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
		NGUIEditorTools.DrawHeader("Input", true);
		NGUIEditorTools.BeginContents();

		GUILayout.BeginHorizontal();
		mType = (FontType)EditorGUILayout.EnumPopup("Type", mType, GUILayout.MinWidth(200f));
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();
		int create = 0;

		if (mType == FontType.Dynamic)
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

			EditorGUI.BeginDisabledGroup(ttf == null);
			if (GUILayout.Button("Create")) create = 1;
			EditorGUI.EndDisabledGroup();

#if UNITY_3_5
			EditorGUILayout.HelpBox("Dynamic fonts require Unity 4.0 or higher.", MessageType.Error);
#else
			// Helpful info
			if (ttf == null)
			{
				EditorGUILayout.HelpBox("Dynamic font creation happens right in Unity. Simply specify the TrueType font to be used as source.", MessageType.Info);
			}
			EditorGUILayout.HelpBox("Please note that dynamic fonts can't be made a part of an atlas, and using dynamic fonts will result in at least one extra draw call.", MessageType.Warning);
#endif
		}
		else
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

			// Helpful info
			if (NGUISettings.fontData == null)
			{
				EditorGUILayout.HelpBox("The bitmap font creation mostly takes place outside of Unity. You can use BMFont on " +
					"Windows or your choice of Glyph Designer or the less expensive bmGlyph on the Mac.\n\n" +
					"Either of these tools will create a FNT file for you that you will drag & drop into the field above.", MessageType.Info);
			}
			else if (NGUISettings.fontTexture == null)
			{
				EditorGUILayout.HelpBox("When exporting your font, you should get two files: the TXT, and the texture. Only one texture can be used per font.", MessageType.Info);
			}
			else if (NGUISettings.atlas == null)
			{
				EditorGUILayout.HelpBox("You can create a font that doesn't use a texture atlas. This will mean that the text " +
					"labels using this font will generate an extra draw call, and will need to be sorted by " +
					"adjusting the Z instead of the Depth.\n\nIf you do specify an atlas, the font's texture will be added to it automatically.", MessageType.Info);

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Create a Font without an Atlas", GUILayout.Width(200f))) create = 2;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Create the Font", GUILayout.Width(140f))) create = 3;

				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
		}

		if (create != 0)
		{
			string prefabPath = EditorUtility.SaveFilePanelInProject("Save As", "New Font.prefab", "prefab", "Save font as...");

			if (!string.IsNullOrEmpty(prefabPath))
			{
				GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

				// Try to load the material
				Material mat = null;
				
				// Non-atlased font
				if (create == 2)
				{
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
				else if (create != 1)
				{
					UIAtlasMaker.AddOrUpdate(NGUISettings.atlas, NGUISettings.fontTexture);
				}

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
		}
	}

	static void CreateFont (UIFont font, int create, Material mat)
	{
		if (create == 1)
		{
			// New dynamic font
			font.atlas = null;
			font.dynamicFont = NGUISettings.dynamicFont;
			font.dynamicFontStyle = NGUISettings.fontStyle;
			font.defaultSize = NGUISettings.fontSize;
		}
		else
		{
			// New bitmap font
			font.dynamicFont = null;
			BMFontReader.Load(font.bmFont, NGUITools.GetHierarchy(font.gameObject), NGUISettings.fontData.bytes);

			if (create == 2)
			{
				font.atlas = null;
				font.material = mat;
			}
			else if (create == 3)
			{
				font.spriteName = NGUISettings.fontTexture.name;
				font.atlas = NGUISettings.atlas;
			}
		}
	}
}
