//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Font maker lets you create font prefabs with a single click of a button.
/// </summary>

public class UIFontMaker : EditorWindow
{
	/// <summary>
	/// Update all labels associated with this font.
	/// </summary>

	void MarkAsChanged ()
	{
		if (NGUISettings.font != null)
		{
			List<UILabel> labels = NGUIEditorTools.FindInScene<UILabel>();

			foreach (UILabel lbl in labels)
			{
				if (lbl.font == NGUISettings.font)
				{
					lbl.font = null;
					lbl.font = NGUISettings.font;
				}
			}
		}
	}

	/// <summary>
	/// Font selection callback.
	/// </summary>

	void OnSelectFont (MonoBehaviour obj)
	{
		NGUISettings.font = obj as UIFont;
		Repaint();
	}

	/// <summary>
	/// Atlas selection callback.
	/// </summary>

	void OnSelectAtlas (MonoBehaviour obj)
	{
		NGUISettings.atlas = obj as UIAtlas;
		Repaint();
	}

	/// <summary>
	/// Refresh the window on selection.
	/// </summary>

	void OnSelectionChange () { Repaint(); }

	/// <summary>
	/// Draw the UI for this tool.
	/// </summary>

	void OnGUI ()
	{
		string prefabPath = "";
		string matPath = "";

		if (NGUISettings.font != null && NGUISettings.font.name == NGUISettings.fontName)
		{
			prefabPath = AssetDatabase.GetAssetPath(NGUISettings.font.gameObject.GetInstanceID());
			if (NGUISettings.font.material != null) matPath = AssetDatabase.GetAssetPath(NGUISettings.font.material.GetInstanceID());
		}

		// Assume default values if needed
		if (string.IsNullOrEmpty(NGUISettings.fontName)) NGUISettings.fontName = "New Font";
		if (string.IsNullOrEmpty(prefabPath)) prefabPath = NGUIEditorTools.GetSelectionFolder() + NGUISettings.fontName + ".prefab";
		if (string.IsNullOrEmpty(matPath)) matPath = NGUIEditorTools.GetSelectionFolder() + NGUISettings.fontName + ".mat";

		EditorGUIUtility.LookLikeControls(80f);

		NGUIEditorTools.DrawHeader("Input");

		NGUISettings.fontData = EditorGUILayout.ObjectField("Font Data", NGUISettings.fontData, typeof(TextAsset), false) as TextAsset;
		NGUISettings.fontTexture = EditorGUILayout.ObjectField("Texture", NGUISettings.fontTexture, typeof(Texture2D), false) as Texture2D;

		// Draw the atlas selection only if we have the font data and texture specified, just to make it easier
		if (NGUISettings.fontData != null && NGUISettings.fontTexture != null)
		{
			NGUIEditorTools.DrawHeader("Output");

			GUILayout.BeginHorizontal();
			GUILayout.Label("Font Name", GUILayout.Width(76f));
			GUI.backgroundColor = Color.white;
			NGUISettings.fontName = GUILayout.TextField(NGUISettings.fontName);
			GUILayout.EndHorizontal();

			ComponentSelector.Draw<UIFont>("...or select", NGUISettings.font, OnSelectFont);
			ComponentSelector.Draw<UIAtlas>(NGUISettings.atlas, OnSelectAtlas);
		}
		NGUIEditorTools.DrawSeparator();

		// Helpful info
		if (NGUISettings.fontData == null)
		{
			GUILayout.Label(
				"The font creation mostly takes place outside\n" +
				"of Unity. You can use BMFont on Windows\n" +
				"or your choice of Glyph Designer or the\n" +
				"less expensive bmGlyph on the Mac.\n\n" +
				"Either of those tools will create a TXT for\n" +
				"you that you will drag & drop into the\n" +
				"field above.");
		}
		else if (NGUISettings.fontTexture == null)
		{
			GUILayout.Label(
				"When exporting your font, you should get\n" +
				"two files: the TXT, and the texture. Only\n" +
				"one texture can be used per font.");
		}
		else if (NGUISettings.atlas == null)
		{
			GUILayout.Label(
				"You can create a font that doesn't use a\n" +
				"texture atlas. This will mean that the text\n" +
				"labels using this font will generate an extra\n" +
				"draw call, and will need to be sorted by\n" +
				"adjusting the Z instead of the Depth.\n\n" +
				"If you do specify an atlas, the font's texture\n" +
				"will be added to it automatically.");

			NGUIEditorTools.DrawSeparator();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUI.backgroundColor = Color.red;
			bool create = GUILayout.Button("Create a Font without an Atlas", GUILayout.Width(200f));
			GUI.backgroundColor = Color.white;
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			if (create)
			{
				GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

				if (go == null || EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to replace the contents of the " +
					NGUISettings.fontName + " font with the currently selected values? This action can't be undone.", "Yes", "No"))
				{
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

					mat.mainTexture = NGUISettings.fontTexture;

					if (go == null || go.GetComponent<UIFont>() == null)
					{
						// Create a new prefab for the atlas
#if UNITY_3_4
						Object prefab = EditorUtility.CreateEmptyPrefab(prefabPath);
#else
						Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
#endif
						// Create a new game object for the font
						go = new GameObject(NGUISettings.fontName);
						NGUISettings.font = go.AddComponent<UIFont>();
						NGUISettings.font.material = mat;
						BMFontReader.Load(NGUISettings.font.bmFont, NGUITools.GetHierarchy(NGUISettings.font.gameObject), NGUISettings.fontData.bytes);

						// Update the prefab
#if UNITY_3_4
						EditorUtility.ReplacePrefab(go, prefab);
#else
						PrefabUtility.ReplacePrefab(go, prefab);
#endif
						DestroyImmediate(go);
						AssetDatabase.Refresh();

						// Select the atlas
						go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
					}

					NGUISettings.font = go.GetComponent<UIFont>();
					MarkAsChanged();
				}
			}
		}
		else
		{
			GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

			bool create = false;

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (go != null)
			{
				if (go.GetComponent<UIFont>() != null)
				{
					GUI.backgroundColor = Color.red;
					create = GUILayout.Button("Replace the Font", GUILayout.Width(140f));
				}
				else
				{
					GUI.backgroundColor = Color.grey;
					GUILayout.Button("Rename Your Font", GUILayout.Width(140f));
				}
			}
			else
			{
				GUI.backgroundColor = Color.green;
				create = GUILayout.Button("Create the Font", GUILayout.Width(140f));
			}
			GUI.backgroundColor = Color.white;
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			if (create)
			{
				if (go == null || EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to replace the contents of the " +
					NGUISettings.fontName + " font with the currently selected values? This action can't be undone.", "Yes", "No"))
				{
					UIAtlasMaker.AddOrUpdate(NGUISettings.atlas, NGUISettings.fontTexture);

					if (go == null || go.GetComponent<UIFont>() == null)
					{
						// Create a new prefab for the atlas
#if UNITY_3_4
						Object prefab = EditorUtility.CreateEmptyPrefab(prefabPath);
#else
						Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
#endif
						// Create a new game object for the font
						go = new GameObject(NGUISettings.fontName);
						NGUISettings.font = go.AddComponent<UIFont>();
						NGUISettings.font.atlas = NGUISettings.atlas;
						NGUISettings.font.spriteName = NGUISettings.fontTexture.name;
						BMFontReader.Load(NGUISettings.font.bmFont, NGUITools.GetHierarchy(NGUISettings.font.gameObject), NGUISettings.fontData.bytes);

						// Update the prefab
#if UNITY_3_4
						EditorUtility.ReplacePrefab(go, prefab);
#else
						PrefabUtility.ReplacePrefab(go, prefab);
#endif
						DestroyImmediate(go);
						AssetDatabase.Refresh();

						// Select the atlas
						go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
					}
					else if (NGUISettings.fontData != null)
					{
						BMFontReader.Load(NGUISettings.font.bmFont, NGUITools.GetHierarchy(NGUISettings.font.gameObject), NGUISettings.fontData.bytes);
						EditorUtility.SetDirty(NGUISettings.font);
						NGUISettings.font.MarkAsDirty();
					}

					NGUISettings.font = go.GetComponent<UIFont>();
					NGUISettings.font.spriteName = NGUISettings.fontTexture.name;
					NGUISettings.font.atlas = NGUISettings.atlas;
					MarkAsChanged();
				}
			}
		}
	}
}