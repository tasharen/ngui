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
			AddWidget<UILabel>();
		}
	}

	[MenuItem("NGUI/Add Sprite")]
	static void AddSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UISprite>();
		}
	}

	[MenuItem("NGUI/Add Sliced Sprite")]
	static void AddSlicedSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UISlicedSprite>();
		}
	}
	
	[MenuItem("NGUI/Add Tiled Sprite")]
	static void AddTiledSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UITiledSprite>();
		}
	}

	[MenuItem("NGUI/Add Collider")]
	static void AddCollider ()
	{
		if (PrefabCheck())
		{
			GameObject go = Selection.activeGameObject;
			Collider col = go.GetComponent<Collider>();
			BoxCollider box = col as BoxCollider;

			Undo.RegisterUndo(go, "Add a collider");
			
			if (box == null)
			{
				if (col != null)
				{
					if (Application.isPlaying) GameObject.Destroy(col);
					else GameObject.DestroyImmediate(col);
				}
				box = go.AddComponent<BoxCollider>();
			}

			UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>() as UIWidget[];

			Matrix4x4 mat = go.transform.worldToLocalMatrix;
			Bounds b = new Bounds(Vector3.zero, Vector3.zero);

			foreach (UIWidget w in widgets)
			{
				if (w.centered)
				{
					b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0f))));
					b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(-0.5f,  0.5f, 0f))));
					b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3( 0.5f, -0.5f, 0f))));
					b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3( 0.5f,  0.5f, 0f))));
				}
				else
				{
					b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(0f, 0f, 0f))));
					b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(0f, 1f, 0f))));
					b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(1f, 0f, 0f))));
					b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(1f, 1f, 0f))));
				}
			}

			box.isTrigger = true;
			box.center = b.center;
			box.size = b.size;
		}
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
			PrefabType type = EditorUtility.GetPrefabType(Selection.activeGameObject);

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