//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to view and edit UIFonts.
/// </summary>

[CustomEditor(typeof(UIFont))]
public class UIFontInspector : Editor
{
	enum View
	{
		Nothing,
		Atlas,
		Font,
	}

	enum FontType
	{
		Normal,
		Reference,
	}

	static View mView = View.Font;
	static bool mUseShader = false;
	
	UIFont mFont;
	FontType mType = FontType.Normal;
	UIFont mReplacement = null;

	public override bool HasPreviewGUI () { return mView != View.Nothing; }

	void OnSelectFont (MonoBehaviour obj)
	{
		// Undo doesn't work correctly in this case... so I won't bother.
		//NGUIEditorTools.RegisterUndo("Font Change");
		//NGUIEditorTools.RegisterUndo("Font Change", mFont);

		mFont.replacement = obj as UIFont;
		mReplacement = mFont.replacement;
		UnityEditor.EditorUtility.SetDirty(mFont);
		if (mReplacement == null) mType = FontType.Normal;
	}

	void OnSelectAtlas (MonoBehaviour obj)
	{
		if (mFont != null)
		{
			NGUIEditorTools.RegisterUndo("Font Atlas", mFont);
			mFont.atlas = obj as UIAtlas;
			MarkAsChanged();
		}
	}

	void MarkAsChanged ()
	{
		List<UILabel> labels = NGUIEditorTools.FindInScene<UILabel>();

		foreach (UILabel lbl in labels)
		{
			if (UIFont.CheckIfRelated(lbl.font, mFont))
			{
				lbl.font = null;
				lbl.font = mFont;
			}
		}
	}

	override public void OnInspectorGUI ()
	{
		mFont = target as UIFont;
		EditorGUIUtility.LookLikeControls(80f);

		NGUIEditorTools.DrawSeparator();

		if (mFont.replacement != null)
		{
			mType = FontType.Reference;
			mReplacement = mFont.replacement;
		}

		FontType after = (FontType)EditorGUILayout.EnumPopup("Font Type", mType);

		if (mType != after)
		{
			if (after == FontType.Normal)
			{
				OnSelectFont(null);
			}
			else
			{
				mType = FontType.Reference;
			}
		}

		if (mType == FontType.Reference)
		{
			ComponentSelector.Draw<UIFont>(mFont.replacement, OnSelectFont);

			NGUIEditorTools.DrawSeparator();
			GUILayout.Label("You can have one font simply point to\n" +
				"another one. This is useful if you want to be\n" +
				"able to quickly replace the contents of one\n" +
				"font with another one, for example for\n" +
				"swapping an SD font with an HD one, or\n" +
				"replacing an English font with a Chinese\n" +
				"one. All the labels referencing this font\n" +
				"will update their references to the new one.");

			if (mReplacement != mFont && mFont.replacement != mReplacement)
			{
				NGUIEditorTools.RegisterUndo("Font Change", mFont);
				mFont.replacement = mReplacement;
				UnityEditor.EditorUtility.SetDirty(mFont);
			}
			return;
		}

		NGUIEditorTools.DrawSeparator();
		ComponentSelector.Draw<UIAtlas>(mFont.atlas, OnSelectAtlas);

		if (mFont.atlas != null)
		{
			if (mFont.bmFont.LegacyCheck())
			{
				Debug.Log(mFont.name + " uses a legacy font data structure. Upgrading, please save.");
				EditorUtility.SetDirty(mFont);
			}

			if (mFont.bmFont.isValid)
			{
				NGUIEditorTools.AdvancedSpriteField(mFont.atlas, mFont.spriteName, SelectSprite, false);
			}
		}
		else
		{
			// No atlas specified -- set the material and texture rectangle directly
			Material mat = EditorGUILayout.ObjectField("Material", mFont.material, typeof(Material), false) as Material;

			if (mFont.material != mat)
			{
				NGUIEditorTools.RegisterUndo("Font Material", mFont);
				mFont.material = mat;
			}
		}

		bool resetWidthHeight = false;

		if (mFont.atlas != null || mFont.material != null)
		{
			TextAsset data = EditorGUILayout.ObjectField("Import Font", null, typeof(TextAsset), false) as TextAsset;

			if (data != null)
			{
				NGUIEditorTools.RegisterUndo("Import Font Data", mFont);
				BMFontReader.Load(mFont.bmFont, NGUITools.GetHierarchy(mFont.gameObject), data.bytes);
				mFont.MarkAsDirty();
				resetWidthHeight = true;
				Debug.Log("Imported " + mFont.bmFont.glyphCount + " characters");
			}
		}

		if (mFont.bmFont.isValid)
		{
			Color green = new Color(0.4f, 1f, 0f, 1f);
			Texture2D tex = mFont.texture;

			if (tex != null)
			{
				if (mFont.atlas == null)
				{
					// Pixels are easier to work with than UVs
					Rect pixels = NGUIMath.ConvertToPixels(mFont.uvRect, tex.width, tex.height, false);

					// Automatically set the width and height of the rectangle to be the original font texture's dimensions
					if (resetWidthHeight)
					{
						pixels.width = mFont.texWidth;
						pixels.height = mFont.texHeight;
					}

					// Font sprite rectangle
					GUI.backgroundColor = green;
					pixels = EditorGUILayout.RectField("Pixel Rect", pixels);
					GUI.backgroundColor = Color.white;

					// Create a button that can make the coordinates pixel-perfect on click
					GUILayout.BeginHorizontal();
					{
						GUILayout.Label("Correction", GUILayout.Width(75f));

						Rect corrected = NGUIMath.MakePixelPerfect(pixels);

						if (corrected == pixels)
						{
							GUI.color = Color.grey;
							GUILayout.Button("Make Pixel-Perfect");
							GUI.color = Color.white;
						}
						else if (GUILayout.Button("Make Pixel-Perfect"))
						{
							pixels = corrected;
							GUI.changed = true;
						}
					}
					GUILayout.EndHorizontal();

					// Convert the pixel coordinates back to UV coordinates
					Rect uvRect = NGUIMath.ConvertToTexCoords(pixels, tex.width, tex.height);

					if (mFont.uvRect != uvRect)
					{
						NGUIEditorTools.RegisterUndo("Font Pixel Rect", mFont);
						mFont.uvRect = uvRect;
					}
				}

				// Font spacing
				GUILayout.BeginHorizontal();
				{
					EditorGUIUtility.LookLikeControls(0f);
					GUILayout.Label("Spacing", GUILayout.Width(60f));
					GUILayout.Label("X", GUILayout.Width(12f));
					int x = EditorGUILayout.IntField(mFont.horizontalSpacing);
					GUILayout.Label("Y", GUILayout.Width(12f));
					int y = EditorGUILayout.IntField(mFont.verticalSpacing);
					EditorGUIUtility.LookLikeControls(80f);

					if (mFont.horizontalSpacing != x || mFont.verticalSpacing != y)
					{
						NGUIEditorTools.RegisterUndo("Font Spacing", mFont);
						mFont.horizontalSpacing = x;
						mFont.verticalSpacing = y;
					}
				}
				GUILayout.EndHorizontal();

				if (mFont.atlas == null)
				{
					mView = View.Font;
					mUseShader = false;

					float pixelSize = EditorGUILayout.FloatField("Pixel Size", mFont.pixelSize, GUILayout.Width(120f));

					if (pixelSize != mFont.pixelSize)
					{
						NGUIEditorTools.RegisterUndo("Font Change", mFont);
						mFont.pixelSize = pixelSize;
					}
				}
				else
				{
					GUILayout.BeginHorizontal();
					{
						mView = (View)EditorGUILayout.EnumPopup("Preview", mView);
						GUILayout.Label("Shader", GUILayout.Width(45f));
						mUseShader = EditorGUILayout.Toggle(mUseShader, GUILayout.Width(20f));
					}
					GUILayout.EndHorizontal();
				}
			}
		}
	}

	/// <summary>
	/// Draw the font preview window.
	/// </summary>

	public override void OnPreviewGUI (Rect rect, GUIStyle background)
	{
		mFont = target as UIFont;
		if (mFont == null) return;
		Texture2D tex = mFont.texture;

		if (mView != View.Nothing && tex != null)
		{
			Material m = (mUseShader ? mFont.material : null);

			if (mView == View.Font)
			{
				Rect outer = new Rect(mFont.uvRect);
				Rect uv = outer;

				outer = NGUIMath.ConvertToPixels(outer, tex.width, tex.height, true);

				NGUIEditorTools.DrawSprite(tex, rect, outer, outer, uv, Color.white, m);
			}
			else
			{
				Rect outer = new Rect(0f, 0f, 1f, 1f);
				Rect inner = new Rect(mFont.uvRect);
				Rect uv = outer;

				outer = NGUIMath.ConvertToPixels(outer, tex.width, tex.height, true);
				inner = NGUIMath.ConvertToPixels(inner, tex.width, tex.height, true);

				NGUIEditorTools.DrawSprite(tex, rect, outer, inner, uv, Color.white, m);
			}
		}
	}

	/// <summary>
	/// Sprite selection callback.
	/// </summary>

	void SelectSprite (string spriteName)
	{
		NGUIEditorTools.RegisterUndo("Font Sprite", mFont);
		mFont.spriteName = spriteName;
		Repaint();
	}
}
