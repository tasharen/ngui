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
	/// Add a child object to the specified parent and attaches the specified script to it.
	/// </summary>

	static public T AddChild<T> (GameObject parent) where T : Component
	{
		string s = typeof(T).ToString();
		if (s.StartsWith("UI")) s = s.Substring(2);
		else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);

		GameObject go = new GameObject(s);

		if (parent != null)
		{
			go.transform.parent = parent.transform;
			go.layer = parent.layer;
		}
		return go.AddComponent<T>();
	}

	/// <summary>
	/// Helper function that returns the selected root object.
	/// </summary>
	/// <returns></returns>

	static public GameObject SelectedRoot ()
	{
		GameObject go = Selection.activeGameObject;

		if (go != null)
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
		Undo.RegisterSceneUndo("Add " + typeof(T).ToString());

		// Create the widget and place it above other widgets
		T widget = AddChild<T>(go);
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

	[MenuItem("NGUI/Create New UI")]
	static void CreateNewUI ()
	{
		/*Undo.RegisterSceneUndo("Create New UI");

		// Figure out the depth of the highest camera
		float depth = -1f;
		bool clearColor = true;
		bool audioListener = true;
		int mask = 0;
		Camera[] cameras = Resources.FindObjectsOfTypeAll(typeof(Camera)) as Camera[];

		foreach (Camera c in cameras)
		{
			depth = Mathf.Max(depth, c.depth);

			if (c.enabled && c.gameObject.active)
			{
				mask |= c.cullingMask;
				clearColor = false;
				if (c.GetComponent<AudioListener>() != null) audioListener = false;
			}
		}

		// Find the first unused layer
		int layer = 0;

		if (mask != 0 && mask != ~0)
		{
			Debug.Log(mask);
			while ((mask & 1) == 1 && layer < 32)
			{
				++layer;
				mask >>= 1;
			}
			Debug.Log(mask + " " + layer);
		}

		// Orthographic root for the UI
		GameObject root = new GameObject("UI Root");
		root.AddComponent<UIOrthoRoot>();
		root.layer = layer;

		// Camera and UICamera for this UI
		Camera cam = AddChild<Camera>(root);
		cam.orthographicSize = 1f;
		cam.orthographic = true;
		cam.nearClipPlane = -10f;
		cam.farClipPlane = 10f;
		cam.depth = depth + 1;
		cam.backgroundColor = Color.grey;
		cam.cullingMask = (mask == ~0) ? 0 : (1 << layer);

		// We don't want to clear color if this is not the first camera
		if (cameras.Length > 0) cam.clearFlags = clearColor ? CameraClearFlags.Skybox : CameraClearFlags.Depth;

		// Add an audio listener if we need one
		if (audioListener) cam.gameObject.AddComponent<AudioListener>();

		// Add a UI Camera for event handling
		cam.gameObject.AddComponent<UICamera>();

		// Center-anchored point with a half-pixel offset for crisp UIs
		UIAnchor anchor = AddChild<UIAnchor>(root);
		anchor.uiCamera = cam;

		// And finally -- the first UI panel
		UIPanel panel = AddChild<UIPanel>(anchor.gameObject);

		if (cam.cullingMask == 0)
		{
			Debug.LogWarning("You already have cameras in the scene that draw every single layer.\n" +
				"Please clarify which layer the UI camera should use.", cam);
			Selection.activeGameObject = cam.gameObject;
		}
		else
		{
			Selection.activeGameObject = panel.gameObject;
		}*/
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