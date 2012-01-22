using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Creation Wizard
/// </summary>

public class UICreateTool : EditorWindow
{
	public enum CameraType
	{
		None,
		Simple2D,
		Advanced3D,
	}

	static public int layer = 0;
	static CameraType camType = CameraType.Simple2D;

	/// <summary>
	/// Helper function that returns the string name of the type.
	/// </summary>

	static public string GetName<T> () where T : Component
	{
		string s = typeof(T).ToString();
		if (s.StartsWith("UI")) s = s.Substring(2);
		else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
		return s;
	}

	/// <summary>
	/// Add a child object to the specified parent and attaches the specified script to it.
	/// </summary>

	static public T AddChild<T> (GameObject parent) where T : Component
	{
		GameObject go = new GameObject(GetName<T>());

		if (parent != null)
		{
			Transform t = go.transform;
			t.parent = parent.transform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
			go.layer = parent.layer;
		}
		return go.AddComponent<T>();
	}

	/// <summary>
	/// Convenience function that determines if the specified object is a prefab.
	/// </summary>

	static public bool IsPrefab (GameObject go)
	{
#if UNITY_3_4
		PrefabType type = EditorUtility.GetPrefabType(go);
#else
		PrefabType type = PrefabUtility.GetPrefabType(go);
#endif
		return (type != PrefabType.None);
	}

	/// <summary>
	/// Refresh the window on selection.
	/// </summary>

	void OnSelectionChange () { Repaint(); }

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
		EditorGUIUtility.LookLikeControls(80f);

		GUILayout.Label("Create a new UI with the following parameters:");
		GUITools.DrawSeparator();

		GUILayout.BeginHorizontal();
		layer = EditorGUILayout.LayerField("Layer", layer, GUILayout.Width(200f));
		GUILayout.Space(20f);
		GUILayout.Label("This is the layer your UI will reside on");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		camType = (CameraType)EditorGUILayout.EnumPopup("Camera", camType, GUILayout.Width(200f));
		GUILayout.Space(20f);
		GUILayout.Label("Should this UI have a camera?");
		GUILayout.EndHorizontal();

		GUITools.DrawSeparator();
		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("When ready,");
		bool create = GUILayout.Button("Create Your UI", GUILayout.Width(120f));
		GUILayout.EndHorizontal();

		if (create) CreateNewUI();
	}

	void CreateNewUI ()
	{
		Undo.RegisterSceneUndo("Create New UI");

		// Root for the UI
		GameObject root = null;

		if (camType == CameraType.Simple2D)
		{
			root = new GameObject("UI Root (2D)");
			root.AddComponent<UIOrthoRoot>();
		}
		else
		{
			root = new GameObject((camType == CameraType.Advanced3D) ? "UI Root (3D)" : "UI Root");
			root.transform.localScale = new Vector3(0.0025f, 0.0025f, 0.0025f);
		}

		// Assign the layer to be used by everything
		root.layer = layer;

		// Figure out the depth of the highest camera
		if (camType == CameraType.None)
		{
			// No camera requested -- simply add a panel
			UIPanel panel = AddChild<UIPanel>(root.gameObject);
			Selection.activeGameObject = panel.gameObject;
		}
		else
		{
			int mask = 1 << layer;
			float depth = -1f;
			bool clearColor = true;
			bool audioListener = true;
			Camera[] cameras = Resources.FindObjectsOfTypeAll(typeof(Camera)) as Camera[];

			foreach (Camera c in cameras)
			{
				if (IsPrefab(c.gameObject)) continue;
				if (c.name == "Preview Camera") continue;
				if (c.name == "SceneCamera") continue;

				// Choose the maximum depth
				depth = Mathf.Max(depth, c.depth);

				// Automatically exclude the specified layer mask from the camera if it can see more than that layer
				if (layer != 0 && c.cullingMask != mask) c.cullingMask = (c.cullingMask & (~mask));

				// Only consider this object if it's active
				if (c.enabled && c.gameObject.active) clearColor = false;

				// If this camera has an audio listener, we won't need to add one
				if (c.GetComponent<AudioListener>() != null) audioListener = false;
			}

			// Camera and UICamera for this UI
			Camera cam = AddChild<Camera>(root);
			cam.depth = depth + 1;
			cam.backgroundColor = Color.grey;
			cam.cullingMask = mask;

			if (camType == CameraType.Simple2D)
			{
				cam.orthographicSize = 1f;
				cam.orthographic = true;
				cam.nearClipPlane = -10f;
				cam.farClipPlane = 10f;
			}
			else
			{
				cam.nearClipPlane = 0.1f;
				cam.farClipPlane = 4f;
				cam.transform.localPosition = new Vector3(0f, 0f, -700f);
			}

			// We don't want to clear color if this is not the first camera
			if (cameras.Length > 0) cam.clearFlags = clearColor ? CameraClearFlags.Skybox : CameraClearFlags.Depth;

			// Add an audio listener if we need one
			if (audioListener) cam.gameObject.AddComponent<AudioListener>();

			// Add a UI Camera for event handling
			cam.gameObject.AddComponent<UICamera>();

			// Anchor is useful to have
			UIAnchor anchor = AddChild<UIAnchor>(cam.gameObject);
			anchor.uiCamera = cam;

			// Since the camera was brought back 700 units above, we should bring the anchor forward 700 to compensate
			if (camType == CameraType.Advanced3D) anchor.depthOffset = 700f;

			// And finally -- the first UI panel
			UIPanel panel = AddChild<UIPanel>(anchor.gameObject);
			Selection.activeGameObject = panel.gameObject;
		}
	}
}