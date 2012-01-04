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
			Collider col = go.GetComponent<Collider>();
			BoxCollider box = col as BoxCollider;

			if (box == null)
			{
				Undo.RegisterUndo(go, "Add a collider");

				if (col != null)
				{
					if (Application.isPlaying) GameObject.Destroy(col);
					else GameObject.DestroyImmediate(col);
				}
				box = go.AddComponent<BoxCollider>();
			}
			else
			{
				Undo.RegisterUndo(box, "Resize the collider");
			}

			UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>() as UIWidget[];

			Matrix4x4 mat = go.transform.worldToLocalMatrix;
			Bounds b = new Bounds(Vector3.zero, Vector3.zero);

			foreach (UIWidget w in widgets)
			{
				Vector2 size = w.visibleSize;
				Vector2 offset = w.pivotOffset;
				float x = (offset.x + 0.5f) * size.x;
				float y = (offset.y - 0.5f) * size.y;
				size *= 0.5f;

				b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(x - size.x, y - size.y, 0f))));
				b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(x - size.x, y + size.y, 0f))));
				b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(x + size.x, y - size.y, 0f))));
				b.Encapsulate(mat.MultiplyPoint(w.transform.TransformPoint(new Vector3(x + size.x, y + size.y, 0f))));
			}

			box.isTrigger = true;
			box.center = b.center;
			box.size = b.size; // Need 3D colliders? Try this: new Vector3(b.size.x, b.size.y, Mathf.Max(1f, b.size.z));
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