//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(UICamera))]
public class UICameraEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		EditorGUIUtility.labelWidth = 140f;
		base.OnInspectorGUI();
	}
}
