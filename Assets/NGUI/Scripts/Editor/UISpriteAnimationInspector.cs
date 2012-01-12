using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UISpriteAnimations.
/// </summary>

[CustomEditor(typeof(UISpriteAnimation))]
public class UISpriteAnimationInspector : Editor
{
	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		GUITools.DrawSeparator();
		EditorGUIUtility.LookLikeControls(80f);
		UISpriteAnimation anim = target as UISpriteAnimation;

		int fps = EditorGUILayout.IntField("Framerate", anim.framesPerSecond);
		fps = Mathf.Clamp(fps, 1, 60);

		if (anim.framesPerSecond != fps)
		{
			Undo.RegisterUndo(anim, "Sprite Animation Change");
			anim.framesPerSecond = fps;
			EditorUtility.SetDirty(anim);
		}

		string namePrefix = EditorGUILayout.TextField("Name Prefix", anim.namePrefix);

		if (anim.namePrefix != namePrefix)
		{
			Undo.RegisterUndo(anim, "Sprite Animation Change");
			anim.namePrefix = namePrefix;
			EditorUtility.SetDirty(anim);
		}
	}
}