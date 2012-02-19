//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UIFilledSprites.
/// </summary>

[CustomEditor(typeof(UIFilledSprite))]
public class UIFilledSpriteInspector : UISpriteInspector
{
	override protected bool OnDrawProperties()
	{
		UIFilledSprite sprite = mWidget as UIFilledSprite;

		if (!base.OnDrawProperties()) return false;

		UIFilledSprite.FillDirection fillDirection = (UIFilledSprite.FillDirection)EditorGUILayout.EnumPopup("Fill Dir", sprite.fillDirection);

		if (sprite.fillDirection != fillDirection)
		{
			NGUIEditorTools.RegisterUndo("Sprite Change", mSprite);
			sprite.fillDirection = fillDirection;
		}

		float fillAmount = EditorGUILayout.FloatField("Fill Amount", sprite.fillAmount);

		if (sprite.fillAmount != fillAmount)
		{
			NGUIEditorTools.RegisterUndo("Sprite Change", mSprite);
			sprite.fillAmount = fillAmount;
		}
		return true;
	}
}