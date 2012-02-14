using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Font maker lets you create font prefabs with a single click of a button.
/// </summary>

public class UIFontMaker : EditorWindow
{
	UIFont mFont;
	UIAtlas mAtlas;
	TextAsset mAsset;
	Texture2D mTex;
	string mFontName = "";

	/// <summary>
	/// Save the font information to player prefs.
	/// </summary>

	void Save ()
	{
		PlayerPrefs.SetInt("NGUI Font Asset", (mAsset != null) ? mAsset.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Font Texture", (mTex != null) ? mTex.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Font", (mFont != null) ? mFont.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Atlas", (mAtlas != null) ? mAtlas.GetInstanceID() : -1);
		PlayerPrefs.SetString("NGUI Font Name", mFontName);
	}

	/// <summary>
	/// Update all labels associated with this font.
	/// </summary>

	void MarkAsChanged ()
	{
		if (mFont != null)
		{
			UILabel[] labels = Resources.FindObjectsOfTypeAll(typeof(UILabel)) as UILabel[];

			foreach (UILabel lbl in labels)
			{
				if (lbl.font == mFont)
				{
					lbl.font = null;
					lbl.font = mFont;
				}
			}
		}
	}

	/// <summary>
	/// Font selection callback.
	/// </summary>

	void OnSelectFont (MonoBehaviour obj)
	{
		UIFont a = obj as UIFont;

		if (mFont != a)
		{
			mFont = a;
			mFontName = (mFont != null) ? mFont.name : "New Font";
			Save();
			Repaint();
		}
	}

	/// <summary>
	/// Atlas selection callback.
	/// </summary>

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

	/// <summary>
	/// Refresh the window on selection.
	/// </summary>

	void OnSelectionChange () { Repaint(); }

	/// <summary>
	/// Draw the UI for this tool.
	/// </summary>

	void OnGUI ()
	{
		if (mAsset == null)
		{
			int assetID = PlayerPrefs.GetInt("NGUI Font Asset", -1);
			if (assetID != -1) mAsset = EditorUtility.InstanceIDToObject(assetID) as TextAsset;
		}

		if (mTex == null)
		{
			int assetID = PlayerPrefs.GetInt("NGUI Font Texture", -1);
			if (assetID != -1) mTex = EditorUtility.InstanceIDToObject(assetID) as Texture2D;
		}

		if (mFont == null)
		{
			mFontName = PlayerPrefs.GetString("NGUI Font Name");
			int fontID = PlayerPrefs.GetInt("NGUI Font", -1);
			if (fontID != -1) mFont = EditorUtility.InstanceIDToObject(fontID) as UIFont;
		}

		if (mAtlas == null)
		{
			int atlasID = PlayerPrefs.GetInt("NGUI Atlas", -1);
			if (atlasID != -1) mAtlas = EditorUtility.InstanceIDToObject(atlasID) as UIAtlas;
		}

		string prefabPath = "";
		string matPath = "";

		if (mFont != null && mFont.name == mFontName)
		{
			prefabPath = AssetDatabase.GetAssetPath(mFont.gameObject.GetInstanceID());
			if (mFont.material != null) matPath = AssetDatabase.GetAssetPath(mFont.material.GetInstanceID());
		}

		// Assume default values if needed
		if (string.IsNullOrEmpty(mFontName)) mFontName = "New Font";
		if (string.IsNullOrEmpty(prefabPath)) prefabPath = NGUIEditorTools.GetSelectionFolder() + mFontName + ".prefab";
		if (string.IsNullOrEmpty(matPath)) matPath = NGUIEditorTools.GetSelectionFolder() + mFontName + ".mat";

		EditorGUIUtility.LookLikeControls(80f);

		NGUIEditorTools.DrawHeader("Input");

		TextAsset asset = EditorGUILayout.ObjectField("Font Data", mAsset, typeof(TextAsset), false) as TextAsset;
		if (mAsset != asset) { mAsset = asset; Save(); }

		Texture2D tex = EditorGUILayout.ObjectField("Texture", mTex, typeof(Texture2D), false) as Texture2D;
		if (mTex != tex) { mTex = tex; Save(); }

		// Draw the atlas selection only if we have the font data and texture specified, just to make it easier
		if (mAsset != null && mTex != null)
		{
			NGUIEditorTools.DrawHeader("Output");

			GUILayout.BeginHorizontal();
			GUILayout.Label("Font Name", GUILayout.Width(76f));
			GUI.backgroundColor = Color.white;
			string fontName = GUILayout.TextField(mFontName);
			GUILayout.EndHorizontal();

			if (mFontName != fontName)
			{
				mFontName = fontName;
				Save();
			}

			ComponentSelector.Draw<UIFont>("...or select", mFont, OnSelectFont);
			ComponentSelector.Draw<UIAtlas>(mAtlas, OnSelectAtlas);
		}
		NGUIEditorTools.DrawSeparator();

		// Helpful info
		if (mAsset == null)
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
		else if (mTex == null)
		{
			GUILayout.Label(
				"When exporting your font, you should get\n" +
				"two files: the TXT, and the texture. Only\n" +
				"one texture can be used per font.");
		}
		else if (mAtlas == null)
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
					mFontName + " font with the currently selected values? This action can't be undone.", "Yes", "No"))
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

					mat.mainTexture = mTex;

					if (go == null || go.GetComponent<UIFont>() == null)
					{
						// Create a new prefab for the atlas
#if UNITY_3_4
						Object prefab = EditorUtility.CreateEmptyPrefab(prefabPath);
#else
						Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
#endif
						// Create a new game object for the font
						go = new GameObject(mFontName);
						mFont = go.AddComponent<UIFont>();
						mFont.material = mat;
						BMFontReader.Load(mFont.bmFont, NGUITools.GetHierarchy(mFont.gameObject), mAsset.bytes);

						// Update the prefab
						EditorUtility.ReplacePrefab(go, prefab);
						DestroyImmediate(go);
						AssetDatabase.Refresh();

						// Select the atlas
						go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
					}

					mFont = go.GetComponent<UIFont>();
					Save();
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
					mFontName + " font with the currently selected values? This action can't be undone.", "Yes", "No"))
				{
					UIAtlasMaker.AddOrUpdate(mAtlas, mTex);

					if (go == null || go.GetComponent<UIFont>() == null)
					{
						// Create a new prefab for the atlas
#if UNITY_3_4
						Object prefab = EditorUtility.CreateEmptyPrefab(prefabPath);
#else
						Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
#endif
						// Create a new game object for the font
						go = new GameObject(mFontName);
						mFont = go.AddComponent<UIFont>();
						mFont.atlas = mAtlas;
						mFont.spriteName = mTex.name;
						BMFontReader.Load(mFont.bmFont, NGUITools.GetHierarchy(mFont.gameObject), mAsset.bytes);

						// Update the prefab
						EditorUtility.ReplacePrefab(go, prefab);
						DestroyImmediate(go);
						AssetDatabase.Refresh();

						// Select the atlas
						go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
					}

					mFont = go.GetComponent<UIFont>();
					mFont.spriteName = mTex.name;
					mFont.atlas = mAtlas;
					Save();
					MarkAsChanged();
				}
			}
		}
	}
}