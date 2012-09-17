//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UISlicedSprites.
/// </summary>

[CustomEditor(typeof(UISlicedSprite))]
public class UISlicedSpriteInspector : UISpriteInspector
{
	/// <summary>
	/// Draw the atlas and sprite selection fields.
	/// </summary>

	override protected bool OnDrawProperties ()
	{
		if (base.OnDrawProperties())
		{
			UISlicedSprite sp = mSprite as UISlicedSprite;
			bool fill = EditorGUILayout.Toggle("Fill Center", sp.fillCenter);

			if (sp.fillCenter != fill)
			{
				NGUIEditorTools.RegisterUndo("Sprite Change", sp);
				sp.fillCenter = fill;
				EditorUtility.SetDirty(sp.gameObject);
			}
			return true;
		}
		return false;
	}
}