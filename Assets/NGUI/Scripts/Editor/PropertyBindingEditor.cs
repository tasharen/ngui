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
		NGUIEditorTools.SetLabelWidth(80f);

		serializedObject.Update();

		GUILayout.Space(3f);
		NGUIEditorTools.DrawProperty(serializedObject, "source");
		GUILayout.Space(3f);
		NGUIEditorTools.DrawProperty(serializedObject, "target");

		GUILayout.Space(1f);
		NGUIEditorTools.DrawPaddedProperty(serializedObject, "direction");
		NGUIEditorTools.DrawPaddedProperty(serializedObject, "update");
		GUILayout.BeginHorizontal();
		NGUIEditorTools.DrawProperty(" ", serializedObject, "editMode", GUILayout.Width(100f));
		GUILayout.Label("Update in Edit Mode");
		GUILayout.EndHorizontal();

		serializedObject.ApplyModifiedProperties();
	}
}
