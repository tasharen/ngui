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
		Atlas,
		Font,
	}

	static View mView = View.Atlas;

	override public void OnInspectorGUI ()
	{
		EditorGUIUtility.LookLikeControls(80f);
		GUITools.DrawSeparator();

		UIFont font = target as UIFont;
		TextAsset data = EditorGUILayout.ObjectField("Font Data", font.data, typeof(TextAsset), false) as TextAsset;

		if (font.data != data)
		{
			Undo.RegisterUndo(font, "Font Data");
			font.data = data;
		}

		if (data != null)
		{
			Material mat = EditorGUILayout.ObjectField("Material", font.material, typeof(Material), false) as Material;

			if (font.material != mat)
			{
				Undo.RegisterUndo(font, "Font Material");
				font.material = mat;
			}

			if (mat != null)
			{
				Color green = new Color(0.4f, 1f, 0f, 1f);

				Texture2D tex = mat.mainTexture as Texture2D;

				if (tex != null)
				{
					// Pixels are easier to work with than UVs
					Rect pixels = NGUITools.ConvertToPixels(font.uvRect, tex.width, tex.height, false);

					GUI.backgroundColor = green;
					pixels = EditorGUILayout.RectField("Pixel Rect", pixels);
					GUI.backgroundColor = Color.white;

					EditorGUILayout.Separator();

					// Create a button that can make the coordinates pixel-perfect on click
					GUILayout.BeginHorizontal();
					{
						GUILayout.Label("Correction", GUILayout.Width(75f));

						Rect corrected = NGUITools.MakePixelPerfect(pixels);

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

					mView = (View)EditorGUILayout.EnumPopup("Show", mView);

					// Convert the pixel coordinates back to UV coordinates
					Rect uvRect = NGUITools.ConvertToTexCoords(pixels, tex.width, tex.height);

					if (font.uvRect != uvRect)
					{
						Undo.RegisterUndo(font, "Font Pixel Rect");
						font.uvRect = uvRect;
					}

					// Draw the atlas
					EditorGUILayout.Separator();
					Rect rect = (mView == View.Atlas) ? GUITools.DrawAtlas(tex) : GUITools.DrawSprite(tex, uvRect);
					GUITools.DrawOutline(rect, uvRect, green);

					rect = GUILayoutUtility.GetRect(Screen.width, 18f);
					EditorGUI.DropShadowLabel(rect, "Font Size: " + font.size);
				}
			}
		}
	}
}