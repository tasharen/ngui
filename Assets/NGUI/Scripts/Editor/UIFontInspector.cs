using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to view and edit UIFonts.
/// </summary>

[CustomEditor(typeof(UIFont))]
public class UIFontInspector : Editor
{
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
				GUI.backgroundColor = green;
				Rect uvRect = EditorGUILayout.RectField("UV Rect", font.uvRect);
				GUI.backgroundColor = Color.white;

				if (font.uvRect != uvRect)
				{
					Undo.RegisterUndo(font, "Font UV Rect");
					font.uvRect = uvRect;
				}

				Texture2D tex = mat.mainTexture as Texture2D;

				if (tex != null)
				{
					EditorGUILayout.Separator();
					Rect rect = GUITools.DrawAtlas(tex);
					GUITools.DrawOutline(rect, uvRect, green);
					EditorGUI.DropShadowLabel(new Rect(rect.xMin, rect.yMax, rect.width, 18f), "Font Size: " + font.size);
					GUILayout.Space(22f);
				}
			}
		}
	}
}