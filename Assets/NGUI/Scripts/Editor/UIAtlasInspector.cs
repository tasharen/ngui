//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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
		UIAtlas.Sprite sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;
		if (sprite == null) return;

		UISprite[] sprites = NGUITools.FindActive<UISprite>();

		foreach (UISprite sp in sprites)
		{
			if (sp.spriteName == sprite.name)
			{
				sp.atlas = null;
				sp.atlas = mAtlas;
				EditorUtility.SetDirty(sp);
			}
		}

		UILabel[] labels = NGUITools.FindActive<UILabel>();

		foreach (UILabel lbl in labels)
		{
			if (lbl.font != null && UIAtlas.CheckIfRelated(lbl.font.atlas, mAtlas) && lbl.font.UsesSprite(sprite.name))
			{
				UIFont font = lbl.font;
				lbl.font = null;
				lbl.font = font;
				EditorUtility.SetDirty(lbl);
			}
		}
	}

	/// <summary>
	/// Replacement atlas selection callback.
	/// </summary>

	void OnSelectAtlas (MonoBehaviour obj)
	{
		if (mReplacement != obj)
		{
			// Undo doesn't work correctly in this case... so I won't bother.
			//NGUIEditorTools.RegisterUndo("Atlas Change");
			//NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);

			mAtlas.replacement = obj as UIAtlas;
			mReplacement = mAtlas.replacement;
			UnityEditor.EditorUtility.SetDirty(mAtlas);
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

		UIAtlas.Sprite sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;

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
			ComponentSelector.Draw<UIAtlas>(mAtlas.replacement, OnSelectAtlas);

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
				UnityEditor.EditorUtility.SetDirty(mAtlas);
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

			mAtlas.MarkAsDirty();
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
				mAtlas.MarkAsDirty();
			}

			GUILayout.BeginHorizontal();
			UIAtlas.Coordinates coords = (UIAtlas.Coordinates)EditorGUILayout.EnumPopup("Coordinates", mAtlas.coordinates);
			GUILayout.Space(18f);
			GUILayout.EndHorizontal();

			if (coords != mAtlas.coordinates)
			{
				NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
				mAtlas.coordinates = coords;
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
					NGUIEditorTools.AdvancedSpriteField(mAtlas, sprite.name, SelectSprite, true);
					GUILayout.Space(6f);

					Rect inner = sprite.inner;
					Rect outer = sprite.outer;

					if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
					{
						GUI.backgroundColor = green;
						outer = NGUIEditorTools.IntRect("Dimensions", sprite.outer);

						Vector4 border = new Vector4(
							sprite.inner.xMin - sprite.outer.xMin,
							sprite.inner.yMin - sprite.outer.yMin,
							sprite.outer.xMax - sprite.inner.xMax,
							sprite.outer.yMax - sprite.inner.yMax);

						GUI.backgroundColor = blue;
						border = NGUIEditorTools.IntPadding("Border", border);
						GUI.backgroundColor = Color.white;

						inner.xMin = sprite.outer.xMin + border.x;
						inner.yMin = sprite.outer.yMin + border.y;
						inner.xMax = sprite.outer.xMax - border.z;
						inner.yMax = sprite.outer.yMax - border.w;
					}
					else
					{
						// Draw the inner and outer rectangle dimensions
						GUI.backgroundColor = green;
						outer = EditorGUILayout.RectField("Outer Rect", sprite.outer);
						GUI.backgroundColor = blue;
						inner = EditorGUILayout.RectField("Inner Rect", sprite.inner);
						GUI.backgroundColor = Color.white;
					}

					if (outer.xMax < outer.xMin) outer.xMax = outer.xMin;
					if (outer.yMax < outer.yMin) outer.yMax = outer.yMin;

					if (outer != sprite.outer)
					{
						float x = outer.xMin - sprite.outer.xMin;
						float y = outer.yMin - sprite.outer.yMin;

						inner.x += x;
						inner.y += y;
					}

					// Sanity checks to ensure that the inner rect is always inside the outer
					inner.xMin = Mathf.Clamp(inner.xMin, outer.xMin, outer.xMax);
					inner.xMax = Mathf.Clamp(inner.xMax, outer.xMin, outer.xMax);
					inner.yMin = Mathf.Clamp(inner.yMin, outer.yMin, outer.yMax);
					inner.yMax = Mathf.Clamp(inner.yMax, outer.yMin, outer.yMax);
						
					bool changed = false;
						
					if (sprite.inner != inner || sprite.outer != outer)
					{
						NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
						sprite.inner = inner;
						sprite.outer = outer;
						MarkSpriteAsDirty();
						changed = true;
					}

					EditorGUILayout.Separator();

					if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
					{
						int left	= Mathf.RoundToInt(sprite.paddingLeft	* sprite.outer.width);
						int right	= Mathf.RoundToInt(sprite.paddingRight	* sprite.outer.width);
						int top		= Mathf.RoundToInt(sprite.paddingTop	* sprite.outer.height);
						int bottom	= Mathf.RoundToInt(sprite.paddingBottom	* sprite.outer.height);

						NGUIEditorTools.IntVector a = NGUIEditorTools.IntPair("Padding", "Left", "Top", left, top);
						NGUIEditorTools.IntVector b = NGUIEditorTools.IntPair(null, "Right", "Bottom", right, bottom);

						if (changed || a.x != left || a.y != top || b.x != right || b.y != bottom)
						{
							NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
							sprite.paddingLeft		= a.x / sprite.outer.width;
							sprite.paddingTop		= a.y / sprite.outer.height;
							sprite.paddingRight		= b.x / sprite.outer.width;
							sprite.paddingBottom	= b.y / sprite.outer.height;
							MarkSpriteAsDirty();
						}
					}
					else
					{
						// Create a button that can make the coordinates pixel-perfect on click
						GUILayout.BeginHorizontal();
						{
							GUILayout.Label("Correction", GUILayout.Width(75f));

							Rect corrected0 = outer;
							Rect corrected1 = inner;

							if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
							{
								corrected0 = NGUIMath.MakePixelPerfect(corrected0);
								corrected1 = NGUIMath.MakePixelPerfect(corrected1);
							}
							else
							{
								corrected0 = NGUIMath.MakePixelPerfect(corrected0, tex.width, tex.height);
								corrected1 = NGUIMath.MakePixelPerfect(corrected1, tex.width, tex.height);
							}

							if (corrected0 == sprite.outer && corrected1 == sprite.inner)
							{
								GUI.color = Color.grey;
								GUILayout.Button("Make Pixel-Perfect");
								GUI.color = Color.white;
							}
							else if (GUILayout.Button("Make Pixel-Perfect"))
							{
								outer = corrected0;
								inner = corrected1;
								GUI.changed = true;
							}
						}
						GUILayout.EndHorizontal();
					}
					NGUIEditorTools.EndContents();
				}

				if (NGUIEditorTools.previousSelection != null)
				{
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
		UIAtlas.Sprite sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;
		if (sprite == null) return;

		Texture2D tex = mAtlas.texture as Texture2D;
		if (tex == null) return;

		Rect outer = new Rect(sprite.outer);
		Rect inner = new Rect(sprite.inner);
		Rect uv = outer;

		if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
		{
			uv = NGUIMath.ConvertToTexCoords(outer, tex.width, tex.height);
		}
		else
		{
			outer = NGUIMath.ConvertToPixels(outer, tex.width, tex.height, true);
			inner = NGUIMath.ConvertToPixels(inner, tex.width, tex.height, true);
		}
		NGUIEditorTools.DrawSprite(tex, rect, outer, inner, uv, Color.white);
	}
}
