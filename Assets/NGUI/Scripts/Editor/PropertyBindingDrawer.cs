//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

/// <summary>
/// Generic property binding drawer.
/// </summary>

[CustomPropertyDrawer(typeof(PropertyBinding))]
public class PropertyBindingDrawer : PropertyDrawer
{
	public class Entry
	{
		public Component target;
		public string name;
	}

	/// <summary>
	/// Collect a list of usable properties and fields.
	/// </summary>

	static List<Entry> GetProperties (GameObject target, bool read, bool write)
	{
		Component[] comps = target.GetComponents<Component>();

		List<Entry> list = new List<Entry>();

		for (int i = 0, imax = comps.Length; i < imax; ++i)
		{
			Component comp = comps[i];
			if (comp == null) continue;

			Type type = comp.GetType();
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
			FieldInfo[] fields = type.GetFields(flags);
			PropertyInfo[] props = type.GetProperties(flags);

			for (int b = 0; b < fields.Length; ++b)
			{
				FieldInfo field = fields[b];
				Entry ent = new Entry();
				ent.target = comp;
				ent.name = field.Name;
				list.Add(ent);
			}

			for (int b = 0; b < props.Length; ++b)
			{
				PropertyInfo prop = props[b];
				if (read && !prop.CanRead) continue;
				if (write && !prop.CanWrite) continue;
				Entry ent = new Entry();
				ent.target = comp;
				ent.name = prop.Name;
				list.Add(ent);
			}
		}
		return list;
	}

	/// <summary>
	/// Convert the specified list of delegate entries into a string array.
	/// </summary>

	static public string[] GetNames (List<Entry> list, string choice, out int index)
	{
		index = 0;
		string[] names = new string[list.Count + 1];
		names[0] = string.IsNullOrEmpty(choice) ? "<Choose>" : choice;

		for (int i = 0; i < list.Count; )
		{
			Entry ent = list[i];
			string type = ent.target.GetType().ToString();
			int period = type.LastIndexOf('.');
			if (period > 0) type = type.Substring(period + 1);

			string del = type + "." + ent.name;
			names[++i] = del;

			if (index == 0 && string.Equals(del, choice))
				index = i;
		}
		return names;
	}

	/// <summary>
	/// The property is either going to be 16 or 34 pixels tall, depending on whether the target has been set or not.
	/// </summary>

	public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
	{
		SerializedProperty target = prop.FindPropertyRelative("mTarget");
		Component comp = target.objectReferenceValue as Component;
		return (comp != null) ? 34f : 16f;
	}

	/// <summary>
	/// Draw the actual property.
	/// </summary>

	public override void OnGUI (Rect rect, SerializedProperty prop, GUIContent label)
	{
		SerializedProperty target = prop.FindPropertyRelative("mTarget");
		SerializedProperty field = prop.FindPropertyRelative("mName");

		rect.height = 16f;
		GUI.changed = false;
		EditorGUI.PropertyField(rect, target, label);
		if (GUI.changed) field.stringValue = "";

		Component comp = target.objectReferenceValue as Component;

		if (comp != null)
		{
			EditorGUI.BeginDisabledGroup(target.hasMultipleDifferentValues);
			rect.y += 18f;
			int index = 0;

			// Get all the properties on the target game object
			List<Entry> list = GetProperties(comp.gameObject, true, true);

			// We want the field to look like "Component.property" rather than just "property"
			string current = PropertyBinding.ToString(target.objectReferenceValue as Component, field.stringValue);

			// Convert all the properties to names
			string[] names = PropertyBindingDrawer.GetNames(list, current, out index);

			// Draw a selection list
			GUI.changed = false;
			int choice = EditorGUI.Popup(rect, " ", index, names);

			// Update the target object and property name
			if (GUI.changed && choice > 0)
			{
				Entry ent = list[choice - 1];
				target.objectReferenceValue = ent.target;
				field.stringValue = ent.name;
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}
