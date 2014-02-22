//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Inspector class used to edit the UIAtlas.
/// </summary>

[CustomEditor(typeof(UIAtlas))]
public class UIAtlasInspector : Editor
{
	static public UIAtlasInspector instance;

	enum AtlasType
	{
		Normal,
		Reference,
	}

	UIAtlas mAtlas;
	AtlasType mType = AtlasType.Normal;
	UIAtlas mReplacement = null;

	void OnEnable () { instance = this; }
	void OnDisable () { instance = null; }

	/// <summary>
	/// Convenience function -- mark all widgets using the sprite as changed.
	/// </summary>

	void MarkSpriteAsDirty ()
	{
		UISpriteData sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;
		if (sprite == null) return;

		UISprite[] sprites = NGUITools.FindActive<UISprite>();

		foreach (UISprite sp in sprites)
		{
			if (UIAtlas.CheckIfRelated(sp.atlas, mAtlas) && sp.spriteName == sprite.name)
			{
				UIAtlas atl = sp.atlas;
				sp.atlas = null;
				sp.atlas = atl;
				EditorUtility.SetDirty(sp);
			}
		}

		UILabel[] labels = NGUITools.FindActive<UILabel>();

		foreach (UILabel lbl in labels)
		{
			if (lbl.bitmapFont != null && UIAtlas.CheckIfRelated(lbl.bitmapFont.atlas, mAtlas) && lbl.bitmapFont.UsesSprite(sprite.name))
			{
				UIFont font = lbl.bitmapFont;
				lbl.bitmapFont = null;
				lbl.bitmapFont = font;
				EditorUtility.SetDirty(lbl);
			}
		}
	}

	/// <summary>
	/// Replacement atlas selection callback.
	/// </summary>

	void OnSelectAtlas (Object obj)
	{
		if (mReplacement != obj)
		{
			// Undo doesn't work correctly in this case... so I won't bother.
			//NGUIEditorTools.RegisterUndo("Atlas Change");
			//NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);

			mAtlas.replacement = obj as UIAtlas;
			mReplacement = mAtlas.replacement;
			NGUITools.SetDirty(mAtlas);
			if (mReplacement == null) mType = AtlasType.Normal;
		}
	}

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		NGUIEditorTools.SetLabelWidth(80f);
		mAtlas = target as UIAtlas;

		UISpriteData sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;

		GUILayout.Space(6f);

		if (mAtlas.replacement != null)
		{
			mType = AtlasType.Reference;
			mReplacement = mAtlas.replacement;
		}

		GUILayout.BeginHorizontal();
		AtlasType after = (AtlasType)EditorGUILayout.EnumPopup("Atlas Type", mType);
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();

		if (mType != after)
		{
			if (after == AtlasType.Normal)
			{
				mType = AtlasType.Normal;
				OnSelectAtlas(null);
			}
			else
			{
				mType = AtlasType.Reference;
			}
		}

		if (mType == AtlasType.Reference)
		{
			ComponentSelector.Draw<UIAtlas>(mAtlas.replacement, OnSelectAtlas, true);

			GUILayout.Space(6f);
			EditorGUILayout.HelpBox("You can have one atlas simply point to " +
				"another one. This is useful if you want to be " +
				"able to quickly replace the contents of one " +
				"atlas with another one, for example for " +
				"swapping an SD atlas with an HD one, or " +
				"replacing an English atlas with a Chinese " +
				"one. All the sprites referencing this atlas " +
				"will update their references to the new one.", MessageType.Info);

			if (mReplacement != mAtlas && mAtlas.replacement != mReplacement)
			{
				NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
				mAtlas.replacement = mReplacement;
				NGUITools.SetDirty(mAtlas);
			}
			return;
		}

		//GUILayout.Space(6f);
		Material mat = EditorGUILayout.ObjectField("Material", mAtlas.spriteMaterial, typeof(Material), false) as Material;

		if (mAtlas.spriteMaterial != mat)
		{
			NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
			mAtlas.spriteMaterial = mat;

			// Ensure that this atlas has valid import settings
			if (mAtlas.texture != null) NGUIEditorTools.ImportTexture(mAtlas.texture, false, false, !mAtlas.premultipliedAlpha);

			mAtlas.MarkAsChanged();
		}

		if (mat != null)
		{
			TextAsset ta = EditorGUILayout.ObjectField("TP Import", null, typeof(TextAsset), false) as TextAsset;

			if (ta != null)
			{
				// Ensure that this atlas has valid import settings
				if (mAtlas.texture != null) NGUIEditorTools.ImportTexture(mAtlas.texture, false, false, !mAtlas.premultipliedAlpha);

				NGUIEditorTools.RegisterUndo("Import Sprites", mAtlas);
				NGUIJson.LoadSpriteData(mAtlas, ta);
				if (sprite != null) sprite = mAtlas.GetSprite(sprite.name);
				mAtlas.MarkAsChanged();
			}

			float pixelSize = EditorGUILayout.FloatField("Pixel Size", mAtlas.pixelSize, GUILayout.Width(120f));

			if (pixelSize != mAtlas.pixelSize)
			{
				NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
				mAtlas.pixelSize = pixelSize;
			}
		}

		if (mAtlas.spriteMaterial != null)
		{
			Color blue = new Color(0f, 0.7f, 1f, 1f);
			Color green = new Color(0.4f, 1f, 0f, 1f);

			if (sprite == null && mAtlas.spriteList.Count > 0)
			{
				string spriteName = NGUISettings.selectedSprite;
				if (!string.IsNullOrEmpty(spriteName)) sprite = mAtlas.GetSprite(spriteName);
				if (sprite == null) sprite = mAtlas.spriteList[0];
			}

			if (sprite != null)
			{
				if (sprite == null) return;
					
				Texture2D tex = mAtlas.spriteMaterial.mainTexture as Texture2D;

				if (tex != null)
				{
					if (!NGUIEditorTools.DrawHeader("Sprite Details")) return;

					NGUIEditorTools.BeginContents();

					GUILayout.Space(3f);
					NGUIEditorTools.DrawAdvancedSpriteField(mAtlas, sprite.name, SelectSprite, true);
					GUILayout.Space(6f);

					GUI.changed = false;

					GUI.backgroundColor = green;
					NGUIEditorTools.IntVector sizeA = NGUIEditorTools.IntPair("Dimensions", "X", "Y", sprite.x, sprite.y);
					NGUIEditorTools.IntVector sizeB = NGUIEditorTools.IntPair(null, "Width", "Height", sprite.width, sprite.height);

					EditorGUILayout.Separator();
					GUI.backgroundColor = blue;
					NGUIEditorTools.IntVector borderA = NGUIEditorTools.IntPair("Border", "Left", "Right", sprite.borderLeft, sprite.borderRight);
					NGUIEditorTools.IntVector borderB = NGUIEditorTools.IntPair(null, "Bottom", "Top", sprite.borderBottom, sprite.borderTop);

					EditorGUILayout.Separator();
					GUI.backgroundColor = Color.white;
					NGUIEditorTools.IntVector padA = NGUIEditorTools.IntPair("Padding", "Left", "Right", sprite.paddingLeft, sprite.paddingRight);
					NGUIEditorTools.IntVector padB = NGUIEditorTools.IntPair(null, "Bottom", "Top", sprite.paddingBottom, sprite.paddingTop);

					if (GUI.changed)
					{
						NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
						
						sprite.x = sizeA.x;
						sprite.y = sizeA.y;
						sprite.width = sizeB.x;
						sprite.height = sizeB.y;

						sprite.paddingLeft = padA.x;
						sprite.paddingRight = padA.y;
						sprite.paddingBottom = padB.x;
						sprite.paddingTop = padB.y;

						sprite.borderLeft = borderA.x;
						sprite.borderRight = borderA.y;
						sprite.borderBottom = borderB.x;
						sprite.borderTop = borderB.y;

						MarkSpriteAsDirty();
					}
					NGUIEditorTools.EndContents();
				}

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				EditorGUILayout.BeginVertical();

				if (GUILayout.Button("Add Transparent Border"))
				{
					List<UIAtlasMaker.SpriteEntry> sprites = new List<UIAtlasMaker.SpriteEntry>();
					UIAtlasMaker.ExtractSprites(mAtlas, sprites);
					UIAtlasMaker.SpriteEntry se = null;

					for (int i = 0; i < sprites.Count; ++i)
					{
						if (sprites[i].name == sprite.name)
						{
							se = sprites[i];
							break;
						}
					}

					if (se != null)
					{
						++se.borderLeft;
						++se.borderRight;
						++se.borderTop;
						++se.borderBottom;

						int w1 = se.tex.width;
						int h1 = se.tex.height;

						int w2 = w1 + 2;
						int h2 = h1 + 2;

						Color32[] c1 = se.tex.GetPixels32();
						Color32[] c2 = new Color32[w2 * h2];

						for (int y2 = 0; y2 < h2; ++y2)
						{
							int y1 = NGUIMath.ClampIndex(y2 - 1, h1);

							for (int x2 = 0; x2 < w2; ++x2)
							{
								int x1 = NGUIMath.ClampIndex(x2 - 1, w1);
								int i2 = x2 + y2 * w2;
								c2[i2] = c1[x1 + y1 * w1];

								if (x2 == 0 || x2 + 1 == w2 || y2 == 0 || y2 + 1 == h2)
									c2[i2].a = 0;
							}
						}

						if (se.temporaryTexture) DestroyImmediate(se.tex);

						se.tex = new Texture2D(w2, h2);
						se.tex.name = sprite.name;
						se.tex.SetPixels32(c2);
						se.tex.Apply();
						se.temporaryTexture = true;

						UIAtlasMaker.UpdateAtlas(mAtlas, sprites);

						DestroyImmediate(se.tex);
						se.tex = null;
					}
				}

				if (GUILayout.Button("Add Clamped Border"))
				{
					List<UIAtlasMaker.SpriteEntry> sprites = new List<UIAtlasMaker.SpriteEntry>();
					UIAtlasMaker.ExtractSprites(mAtlas, sprites);
					UIAtlasMaker.SpriteEntry se = null;

					for (int i = 0; i < sprites.Count; ++i)
					{
						if (sprites[i].name == sprite.name)
						{
							se = sprites[i];
							break;
						}
					}

					if (se != null)
					{
						int w1 = se.tex.width - se.borderLeft - se.borderRight;
						int h1 = se.tex.height - se.borderBottom - se.borderTop;

						int w2 = se.tex.width + 2;
						int h2 = se.tex.height + 2;
						
						Color32[] c1 = se.tex.GetPixels32();
						Color32[] c2 = new Color32[w2 * h2];

						for (int y2 = 0; y2 < h2; ++y2)
						{
							int y1 = se.borderBottom + NGUIMath.ClampIndex(y2 - se.borderBottom - 1, h1);

							for (int x2 = 0; x2 < w2; ++x2)
							{
								int x1 = se.borderLeft + NGUIMath.ClampIndex(x2 - se.borderLeft - 1, w1);
								c2[x2 + y2 * w2] = c1[x1 + y1 * se.tex.width];
							}
						}

						if (se.temporaryTexture) DestroyImmediate(se.tex);

						++se.borderLeft;
						++se.borderRight;
						++se.borderTop;
						++se.borderBottom;

						se.tex = new Texture2D(w2, h2);
						se.tex.name = sprite.name;
						se.tex.SetPixels32(c2);
						se.tex.Apply();
						se.temporaryTexture = true;

						UIAtlasMaker.UpdateAtlas(mAtlas, sprites);

						DestroyImmediate(se.tex);
						se.tex = null;
					}
				}

				if (GUILayout.Button("Add Tiling Border"))
				{
					List<UIAtlasMaker.SpriteEntry> sprites = new List<UIAtlasMaker.SpriteEntry>();
					UIAtlasMaker.ExtractSprites(mAtlas, sprites);
					UIAtlasMaker.SpriteEntry se = null;

					for (int i = 0; i < sprites.Count; ++i)
					{
						if (sprites[i].name == sprite.name)
						{
							se = sprites[i];
							break;
						}
					}

					if (se != null)
					{
						int w1 = se.tex.width - se.borderLeft - se.borderRight;
						int h1 = se.tex.height - se.borderBottom - se.borderTop;

						int w2 = se.tex.width + 2;
						int h2 = se.tex.height + 2;

						Color32[] c1 = se.tex.GetPixels32();
						Color32[] c2 = new Color32[w2 * h2];

						for (int y2 = 0; y2 < h2; ++y2)
						{
							int y1 = se.borderBottom + NGUIMath.RepeatIndex(y2 - se.borderBottom - 1, h1);

							for (int x2 = 0; x2 < w2; ++x2)
							{
								int x1 = se.borderLeft + NGUIMath.RepeatIndex(x2 - se.borderLeft - 1, w1);
								c2[x2 + y2 * w2] = c1[x1 + y1 * se.tex.width];
							}
						}

						if (se.temporaryTexture) DestroyImmediate(se.tex);

						++se.borderLeft;
						++se.borderRight;
						++se.borderTop;
						++se.borderBottom;

						se.tex = new Texture2D(w2, h2);
						se.tex.name = sprite.name;
						se.tex.SetPixels32(c2);
						se.tex.Apply();
						se.temporaryTexture = true;

						UIAtlasMaker.UpdateAtlas(mAtlas, sprites);

						DestroyImmediate(se.tex);
						se.tex = null;
					}
				}

				EditorGUI.BeginDisabledGroup(!sprite.hasBorder);
				if (GUILayout.Button("Crop Border"))
				{
					List<UIAtlasMaker.SpriteEntry> sprites = new List<UIAtlasMaker.SpriteEntry>();
					UIAtlasMaker.ExtractSprites(mAtlas, sprites);
					UIAtlasMaker.SpriteEntry se = null;

					for (int i = 0; i < sprites.Count; ++i)
					{
						if (sprites[i].name == sprite.name)
						{
							se = sprites[i];
							break;
						}
					}

					if (se != null)
					{
						int w1 = se.tex.width;
						int h1 = se.tex.height;

						int w2 = w1 - se.borderLeft - se.borderRight;
						int h2 = h1 - se.borderTop - se.borderBottom;

						Color32[] c1 = se.tex.GetPixels32();
						Color32[] c2 = new Color32[w2 * h2];

						for (int y2 = 0; y2 < h2; ++y2)
						{
							int y1 = y2 + se.borderBottom;

							for (int x2 = 0; x2 < w2; ++x2)
							{
								int x1 = x2 + se.borderLeft;
								c2[x2 + y2 * w2] = c1[x1 + y1 * w1];
							}
						}

						se.borderLeft = 0;
						se.borderRight = 0;
						se.borderTop = 0;
						se.borderBottom = 0;

						if (se.temporaryTexture) DestroyImmediate(se.tex);

						se.tex = new Texture2D(w2, h2);
						se.tex.name = sprite.name;
						se.tex.SetPixels32(c2);
						se.tex.Apply();
						se.temporaryTexture = true;

						UIAtlasMaker.UpdateAtlas(mAtlas, sprites);

						DestroyImmediate(se.tex);
						se.tex = null;
					}
				}
				EditorGUI.EndDisabledGroup();

				if (GUILayout.Button("Extract Sprite"))
				{
					string path = EditorUtility.SaveFilePanelInProject("Save As", sprite.name + ".png", "png", "Extract sprite into which file?");

					if (!string.IsNullOrEmpty(path))
					{
						UIAtlasMaker.SpriteEntry se = UIAtlasMaker.ExtractSprite(mAtlas, sprite.name);

						if (se != null)
						{
							byte[] bytes = se.tex.EncodeToPNG();
							File.WriteAllBytes(path, bytes);
							AssetDatabase.ImportAsset(path);
							if (se.temporaryTexture) DestroyImmediate(se.tex);
						}
					}
				}

				EditorGUILayout.EndVertical();
				GUILayout.Space(28f);
				EditorGUILayout.EndHorizontal();

				if (NGUIEditorTools.previousSelection != null)
				{
					GUILayout.Space(3f);
					GUI.backgroundColor = Color.green;

					if (GUILayout.Button("<< Return to " + NGUIEditorTools.previousSelection.name))
					{
						NGUIEditorTools.SelectPrevious();
					}
					GUI.backgroundColor = Color.white;
				}
			}
		}
	}

	/// <summary>
	/// Sprite selection callback.
	/// </summary>

	void SelectSprite (string spriteName)
	{
		NGUISettings.selectedSprite = spriteName;
		Repaint();
	}

	/// <summary>
	/// All widgets have a preview.
	/// </summary>

	public override bool HasPreviewGUI () { return true; }

	/// <summary>
	/// Draw the sprite preview.
	/// </summary>

	public override void OnPreviewGUI (Rect rect, GUIStyle background)
	{
		UISpriteData sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;
		if (sprite == null) return;

		Texture2D tex = mAtlas.texture as Texture2D;
		if (tex != null) NGUIEditorTools.DrawSprite(tex, rect, sprite, Color.white);
	}
}
