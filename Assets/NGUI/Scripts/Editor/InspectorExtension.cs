using UnityEngine;
using UnityEditor;
using System.Collections;

public class InspectorExtension<T> : Editor where T : class, InspectorExtendable
{
	
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		T theTarget = target as T;
		
		NGUIEditorTools.DrawSeparator();
		theTarget.OnInspectorGUI();
		
	}
}

