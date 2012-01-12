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
			Undo.RegisterUndo(mSprite, "Sprite Change");
			sprite.fillDirection = fillDirection;
			EditorUtility.SetDirty(mSprite.gameObject);
		}

		float fillAmount = EditorGUILayout.FloatField("Fill Amount", sprite.fillAmount);

		if (sprite.fillAmount != fillAmount)
		{
			Undo.RegisterUndo(mSprite, "Sprite Change");
			sprite.fillAmount = fillAmount;
			EditorUtility.SetDirty(mSprite.gameObject);
		}
		return true;
	}
}