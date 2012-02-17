using UnityEngine;
using UnityEditor;

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

	static View mView = View.Atlas;
	static bool mUseShader = false;
	UIFont mFont;

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

	override public void OnInspectorGUI ()
	{
		EditorGUIUtility.LookLikeControls(80f);
		NGUIEditorTools.DrawSeparator();

		mFont = target as UIFont;

		ComponentSelector.Draw<UIAtlas>(mFont.atlas, OnSelectAtlas);

		if (mFont.atlas != null)
		{
			if (mFont.bmFont.isValid)
			{
				string spriteName = UISlicedSpriteInspector.SpriteField(mFont.atlas, mFont.spriteName);

				if (mFont.spriteName != spriteName)
				{
					NGUIEditorTools.RegisterUndo("Font Sprite", mFont);
					mFont.spriteName = spriteName;
				}
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
				mFont.Refresh();
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

				EditorGUILayout.Separator();

				GUILayout.BeginHorizontal();
				{
					mView = (View)EditorGUILayout.EnumPopup("Show", mView);
					GUILayout.Label("Shader", GUILayout.Width(45f));

					if (mUseShader != EditorGUILayout.Toggle(mUseShader, GUILayout.Width(20f)))
					{
						mUseShader = !mUseShader;

						if (mUseShader && mView == View.Font)
						{
							// TODO: Remove this when Unity fixes the bug with DrawPreviewTexture not being affected by BeginGroup
							Debug.LogWarning("There is a bug in Unity that prevents the texture from getting clipped properly.\n" +
								"Until it's fixed by Unity, your texture may spill onto the rest of the Unity's GUI while using this mode.");
						}
					}
				}
				GUILayout.EndHorizontal();

				if (mView != View.Nothing)
				{
					// Draw the atlas
					EditorGUILayout.Separator();
					Material m = mUseShader ? mFont.material : null;
					Rect rect = (mView == View.Atlas) ? NGUIEditorTools.DrawAtlas(tex, m) : NGUIEditorTools.DrawSprite(tex, mFont.uvRect, m);
					NGUIEditorTools.DrawOutline(rect, mFont.uvRect, green);

					rect = GUILayoutUtility.GetRect(Screen.width, 18f);
					EditorGUI.DropShadowLabel(rect, "Font Size: " + mFont.size);
				}
			}
		}
	}
}