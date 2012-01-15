using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

/// <summary>
/// This script adds the NGUI menu options to the Unity Editor.
/// </summary>

static public class NGUIMenu
{
	[MenuItem("NGUI/Add Label")]
	static void AddLabel ()
	{
		if (PrefabCheck())
		{
			AddWidget<UILabel>().SetToLastValues();
		}
	}

	[MenuItem("NGUI/Add Sprite")]
	static void AddSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UISprite>().SetToLastValues();
		}
	}

	[MenuItem("NGUI/Add Sliced Sprite")]
	static void AddSlicedSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UISlicedSprite>().SetToLastValues();
		}
	}
	
	[MenuItem("NGUI/Add Tiled Sprite")]
	static void AddTiledSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UITiledSprite>().SetToLastValues();
		}
	}

	[MenuItem("NGUI/Add Filled Sprite")]
	static void AddFilledSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UIFilledSprite>().SetToLastValues();
		}
	}

	[MenuItem("NGUI/Add Texture (no atlas)")]
	static void AddTexture ()
	{
		if (PrefabCheck())
		{
			AddWidget<UITexture>();
		}
	}

	[MenuItem("NGUI/Add Collider")]
	static void AddCollider ()
	{
		if (PrefabCheck())
		{
			GameObject go = Selection.activeGameObject;
			Undo.RegisterUndo(go, "Widget Collider");
			NGUITools.AddWidgetCollider(go);
		}
	}

	/// <summary>
	/// Add a menu option to display this wizard.
	/// </summary>

	[MenuItem("NGUI/Panel Tool #&p")]
	static void OpenWizard ()
	{
		EditorWindow.GetWindow<UIPanelTool>();
	}

	/// <summary>
	/// Add a new widget of specified type.
	/// </summary>

	static public T AddWidget<T> () where T : UIWidget
	{
		// Make this action undoable
		Undo.RegisterSceneUndo("Add " + typeof(T).ToString());

		// Create our new GameObject
		GameObject newGameObject = new GameObject();
		newGameObject.name = typeof(T).ToString();
		T widget = newGameObject.AddComponent<T>();

		// If there is a selected object in the scene then make the new object its child.
		if (Selection.activeTransform != null)
		{
			newGameObject.transform.parent = Selection.activeTransform;

			// Place the new GameObject at the same position as the parent.
			newGameObject.transform.localPosition = Vector3.zero;
			newGameObject.transform.localRotation = Quaternion.identity;
			newGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			newGameObject.layer = Selection.activeGameObject.layer;
		}

		// Select our newly created GameObject
		Selection.activeGameObject = newGameObject;
		return widget;
	}

	/// <summary>
	/// Helper function that checks to see if a prefab is currently selected.
	/// </summary>

	static bool PrefabCheck ()
	{
		if (Selection.activeTransform != null)
		{
			// Check if the selected object is a prefab instance and display a warning
#if UNITY_3_4
			PrefabType type = EditorUtility.GetPrefabType(Selection.activeGameObject);
#else
			PrefabType type = PrefabUtility.GetPrefabType(Selection.activeGameObject);
#endif

			if (type == PrefabType.PrefabInstance)
			{
				return EditorUtility.DisplayDialog("Losing prefab",
					"This action will lose the prefab connection. Are you sure you wish to continue?",
					"Continue", "Cancel");
			}
		}
		return true;
	}
}