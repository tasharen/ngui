//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using Entry = PropertyReferenceDrawer.Entry;

/// <summary>
/// Draws a single event delegate. Contributed by Adam Byrd.
/// </summary>

[CustomPropertyDrawer(typeof(EventDelegate))]
public class EventDelegateDrawer : PropertyDrawer
{
	const int lineHeight = 16;

	public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
	{
		SerializedProperty targetProp = prop.FindPropertyRelative("mTarget");
		return (targetProp.objectReferenceValue != null) ? 3 * lineHeight : 2 * lineHeight;
	}

	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
	{
		Undo.RecordObject(prop.serializedObject.targetObject, "Delegate Selection");

		SerializedProperty targetProp = prop.FindPropertyRelative("mTarget");
		SerializedProperty methodProp = prop.FindPropertyRelative("mMethodName");

		MonoBehaviour target = targetProp.objectReferenceValue as MonoBehaviour;
		string methodName = methodProp.stringValue;

		EditorGUI.indentLevel = prop.depth;
		EditorGUI.LabelField(position, label);

		Rect line = position;
		line.yMin = position.yMin + lineHeight;
		line.yMax = line.yMin + lineHeight;

		EditorGUI.indentLevel = targetProp.depth;
		target = EditorGUI.ObjectField(line, "Notify", target, typeof(MonoBehaviour), true) as MonoBehaviour;

		if (target != null && target.gameObject != null)
		{
			GameObject go = target.gameObject;
			List<Entry> list = EventDelegateEditor.GetMethods(go);

			int index = 0;
			int choice = 0;

			EventDelegate del = new EventDelegate();
			del.target = target;
			del.methodName = methodName;
			string[] names = PropertyReferenceDrawer.GetNames(list, del.ToString(), out index);

			line.yMin += lineHeight;
			line.yMax += lineHeight;
			choice = EditorGUI.Popup(line, "Method", index, names);

			if (choice > 0)
			{
				if (choice != index)
				{
					Entry entry = list[choice - 1];
					target = entry.target as MonoBehaviour;
					methodName = entry.name;
				}
			}
		}

		targetProp.objectReferenceValue = target;
		methodProp.stringValue = methodName;
	}
}
#endif
