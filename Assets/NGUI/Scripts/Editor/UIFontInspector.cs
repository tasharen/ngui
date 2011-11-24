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

		if (GUI.changed)
		{
			Undo.RegisterUndo(font, "Font Data");
			font.data = data;
		}

		if (data != null)
		{
			Material mat = font.material;
			mat = EditorGUILayout.ObjectField("Material", mat, typeof(Material), false) as Material;

			if (mat != null)
			{
				Texture2D tex = mat.mainTexture as Texture2D;

				if (tex != null)
				{
					EditorGUILayout.Separator();
					Rect rect = GUITools.DrawAtlas(tex);
					EditorGUI.DropShadowLabel(new Rect(rect.xMin, rect.yMax, rect.width, 18f), "Font Size: " + font.size);
					GUILayout.Space(22f);
				}
			}

			if (GUI.changed)
			{
				Undo.RegisterUndo(font, "Font Material");
				font.material = mat;
			}
		}
	}
}