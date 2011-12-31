using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UITextures.
/// </summary>

[CustomEditor(typeof(UITexture))]
public class UITextureInspector : UIWidgetInspector
{
	override protected bool OnCustomStart ()
	{
		Material mat = EditorGUILayout.ObjectField("Material", mWidget.material, typeof(Material), false) as Material;

		if (mWidget.material != mat)
		{
			Undo.RegisterUndo(mWidget, "Material Selection");
			mWidget.material = mat;
			EditorUtility.SetDirty(mWidget.gameObject);
		}

		if (mWidget.material != null)
		{
			Color color = EditorGUILayout.ColorField("Color Tint", mWidget.color);

			if (mWidget.color != color)
			{
				Undo.RegisterUndo(mWidget, "Color Change");
				mWidget.color = color;
				EditorUtility.SetDirty(mWidget.gameObject);
			}

			GUILayout.BeginHorizontal();
			{
				bool center = EditorGUILayout.Toggle("Centered", mWidget.centered, GUILayout.Width(100f));

				if (center != mWidget.centered)
				{
					Undo.RegisterUndo(mWidget, "Center UITexture");
					mWidget.centered = center;
					EditorUtility.SetDirty(mWidget.gameObject);
				}

				if (GUILayout.Button("Make Pixel-Perfect"))
				{
					Undo.RegisterUndo(mWidget.transform, "Make Pixel-Perfect");
					mWidget.MakePixelPerfect();
					EditorUtility.SetDirty(mWidget.transform);
				}
			}
			GUILayout.EndHorizontal();
		}
		return false;
	}
}