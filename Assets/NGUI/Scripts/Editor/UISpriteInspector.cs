using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CustomEditor(typeof(UISprite))]
public class UISpriteInspector : UIWidgetInspector
{
	/// <summary>
	/// Any and all derived functionality.
	/// </summary>

	protected override void OnCustomGUI ()
	{
	}
}