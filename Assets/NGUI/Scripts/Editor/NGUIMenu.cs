using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

/// <summary>
/// This script adds the NGUI menu options to the Unity Editor.
/// </summary>

static public class NGUIMenu
{
	/// <summary>
	/// Helper function that returns the selected root object.
	/// </summary>
	/// <returns></returns>

	static public GameObject SelectedRoot ()
	{
		GameObject go = Selection.activeGameObject;

		// No selection? Try to find the root automatically
		if (go == null)
		{
			UIPanel[] panels = Resources.FindObjectsOfTypeAll(typeof(UIPanel)) as UIPanel[];

			foreach (UIPanel p in panels)
			{
				if (UICreateTool.IsPrefab(p.gameObject)) continue;
				go = p.gameObject;
				break;
			}
		}

		if (go == null)
		{
			// Still nothing? This means we don't have a UI in the scene. Let's create one.
			Debug.Log("No UI found. You can create a new one easily by using the UI creation wizard.\nOpening it for your convenience.");
			CreateNewUI();
		}
		else
		{
			Transform t = go.transform;

			// Find the first uniformly scaled object
			while (!Mathf.Approximately(t.localScale.x, t.localScale.y) ||
				   !Mathf.Approximately(t.localScale.x, t.localScale.z))
			{
				t = t.parent;

				if (t == null)
				{
					Debug.LogWarning("You must select a uniformly scaled object first.");
					return null;
				}
				else go = t.gameObject;
			}
		}
		return go;
	}

	/// <summary>
	/// Helper function that checks to see if a prefab is currently selected.
	/// </summary>

	static bool PrefabCheck ()
	{
		GameObject root = SelectedRoot();
		if (root == null) return false;

		if (root.transform != null)
		{
			// Check if the selected object is a prefab instance and display a warning
#if UNITY_3_4
			PrefabType type = EditorUtility.GetPrefabType(root);
#else
			PrefabType type = PrefabUtility.GetPrefabType(root);
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

	/// <summary>
	/// Add a new widget of specified type.
	/// </summary>

	static public T AddWidget<T> () where T : UIWidget
	{
		GameObject go = SelectedRoot();

		int depth = 1;
		UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>();
		foreach (UIWidget w in widgets) depth = Mathf.Max(depth, w.depth);

		// Make this action undoable
		Undo.RegisterSceneUndo("Add a child " + typeof(T).ToString());

		// Create the widget and place it above other widgets
		T widget = UICreateTool.AddChild<T>(go);
		widget.depth = depth + 1;

		// Clear the local transform
		Transform t = widget.transform;
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		t.localScale = new Vector3(100f, 100f, 1f);
		widget.gameObject.layer = go.layer;

		// Select our newly created GameObject
		Selection.activeGameObject = widget.gameObject;
		return widget;
	}

	[MenuItem("NGUI/Attach a Collider")]
	static void AddCollider ()
	{
		if (PrefabCheck())
		{
			GameObject go = Selection.activeGameObject;
			Undo.RegisterUndo(go, "Widget Collider");
			NGUITools.AddWidgetCollider(go);
		}
	}

	[MenuItem("NGUI/Add a Panel")]
	static void AddPanel ()
	{
		if (PrefabCheck())
		{
			GameObject go = Selection.activeGameObject;
			Undo.RegisterUndo(go, "Add a child UI Panel");

			GameObject child = new GameObject(UICreateTool.GetName<UIPanel>());
			child.layer = go.layer;

			Transform ct = child.transform;
			ct.parent = go.transform;
			ct.localPosition = Vector3.zero;
			ct.localRotation = Quaternion.identity;
			ct.localScale = Vector3.one;

			child.AddComponent<UIPanel>();
			Selection.activeGameObject = child;
		}
	}

	[MenuItem("NGUI/Add a Label")]
	static void AddLabel ()
	{
		if (PrefabCheck())
		{
			AddWidget<UILabel>().SetToLastValues();
		}
	}

	[MenuItem("NGUI/Add a Sprite")]
	static void AddSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UISprite>().SetToLastValues();
		}
	}

	[MenuItem("NGUI/Add a Sliced Sprite")]
	static void AddSlicedSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UISlicedSprite>().SetToLastValues();
		}
	}

	[MenuItem("NGUI/Add a Tiled Sprite")]
	static void AddTiledSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UITiledSprite>().SetToLastValues();
		}
	}

	[MenuItem("NGUI/Add a Filled Sprite")]
	static void AddFilledSprite ()
	{
		if (PrefabCheck())
		{
			AddWidget<UIFilledSprite>().SetToLastValues();
		}
	}

	[MenuItem("NGUI/Add a Texture (no atlas)")]
	static void AddTexture ()
	{
		if (PrefabCheck())
		{
			AddWidget<UITexture>();
		}
	}

	[MenuItem("NGUI/Create a New UI")]
	static void CreateNewUI ()
	{
		EditorWindow.GetWindow<UICreateTool>();
	}

	[MenuItem("NGUI/Panel Tool #&p")]
	static void OpenPanelWizard ()
	{
		EditorWindow.GetWindow<UIPanelTool>();
	}

	[MenuItem("NGUI/Camera Tool #&c")]
	static void OpenCameraWizard ()
	{
		EditorWindow.GetWindow<UICameraTool>();
	}
}