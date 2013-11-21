//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIScrollBar))]
public class UIScrollBarInspector : UIWidgetContainerEditor
{
	public override void OnInspectorGUI ()
	{
		NGUIEditorTools.SetLabelWidth(80f);
		UIScrollBar sb = target as UIScrollBar;

		serializedObject.Update();

		GUILayout.Space(3f);

		float val = EditorGUILayout.Slider("Value", sb.value, 0f, 1f);
		float size = EditorGUILayout.Slider("Size", sb.barSize, 0f, 1f);
		float alpha = EditorGUILayout.Slider("Alpha", sb.alpha, 0f, 1f);

		if (NGUIEditorTools.DrawHeader("Appearance"))
		{
			NGUIEditorTools.BeginContents();
			NGUIEditorTools.DrawProperty("Foreground", serializedObject, "mFG");
			NGUIEditorTools.DrawProperty("Background", serializedObject, "mBG");

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Direction", serializedObject, "mDir");
			GUILayout.Space(18f);
			GUILayout.EndHorizontal();

			NGUIEditorTools.DrawProperty("Inverted", serializedObject, "mInverted");
			NGUIEditorTools.EndContents();
		}

		if (sb.value != val ||
			sb.barSize != size ||
			sb.alpha != alpha)
		{
			NGUIEditorTools.RegisterUndo("Scroll Bar Change", sb);
			sb.value = val;
			sb.barSize = size;
			sb.alpha = alpha;
			UnityEditor.EditorUtility.SetDirty(sb);
		}
		
		NGUIEditorTools.DrawEvents("On Value Change", sb, sb.onChange);
		serializedObject.ApplyModifiedProperties();
	}
}
