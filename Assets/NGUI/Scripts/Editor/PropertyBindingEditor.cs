//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(PropertyBinding))]
public class PropertyBindingEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(3f);
		NGUIEditorTools.SetLabelWidth(80f);
		NGUIEditorTools.DrawPaddedProperty(serializedObject, "update");
		GUILayout.Space(3f);
		NGUIEditorTools.DrawProperty(serializedObject, "source");
		GUILayout.Space(3f);
		NGUIEditorTools.DrawProperty(serializedObject, "target");
	}
}
