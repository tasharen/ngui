//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

/// <summary>
/// Tool that makes it easy to drag prefabs into it to "cache" them for ease of use.
/// </summary>

public class UIPrefabTool : EditorWindow
{
	static public UIPrefabTool instance;

	class Item
	{
		public GameObject prefab;
		public string guid;
		public Texture tex;
		public bool dynamicTex = false;
	}

	enum Mode
	{
		CompactMode,
		IconMode,
		DetailedMode,
	}

	const int cellPadding = 4;

	int cellSize { get { return (mMode == Mode.CompactMode) ? 50 : 80; } }

	Mode mMode = Mode.IconMode;
	Vector2 mPos = Vector2.zero;
	bool mMouseIsInside = false;
	GUIContent mContent;
	GUIStyle mStyle;

	// List of all the added objects
	BetterList<Item> mItems = new BetterList<Item>();

	/// <summary>
	/// Get or set the dragged object.
	/// </summary>

	GameObject draggedObject
	{
		get
		{
			if (DragAndDrop.objectReferences == null) return null;
			if (DragAndDrop.objectReferences.Length == 1) return DragAndDrop.objectReferences[0] as GameObject;
			return null;
		}
		set
		{
			if (value != null)
			{
				DragAndDrop.PrepareStartDrag();
				DragAndDrop.objectReferences = new Object[1] { value };
				DragAndDrop.paths = new string[1] { "" };
				draggedObjectIsOurs = true;
			}
			else DragAndDrop.AcceptDrag();
		}
	}

	/// <summary>
	/// Whether the dragged object is coming from the outside (new object) or from the window (cloned object).
	/// </summary>

	bool draggedObjectIsOurs
	{
		get
		{
			object obj = DragAndDrop.GetGenericData("Prefab Tool");
			if (obj == null) return false;
			return (bool)obj;
		}
		set
		{
			DragAndDrop.SetGenericData("Prefab Tool", value);
		}
	}

	/// <summary>
	/// Initialize everything.
	/// </summary>

	void OnEnable ()
	{
		instance = this;

		Load();

		mContent = new GUIContent();
		mStyle = new GUIStyle();
		mStyle.alignment = TextAnchor.MiddleCenter;
		mStyle.padding = new RectOffset(2, 2, 2, 2);
		mStyle.clipping = TextClipping.Clip;
		mStyle.wordWrap = true;
		mStyle.stretchWidth = false;
		mStyle.stretchHeight = false;
		mStyle.normal.textColor = new Color(1f, 1f, 1f, 0.5f);
		mStyle.normal.background = null;
	}

	/// <summary>
	/// Clean up all textures.
	/// </summary>

	void OnDisable ()
	{
		instance = null;
		foreach (Item item in mItems) DestroyTexture(item);
		Save();
	}

	void OnSelectionChange () { Repaint(); }

	/// <summary>
	/// Reset all loaded prefabs, collecting default controls instead.
	/// </summary>

	public void Reset ()
	{
		foreach (Item item in mItems) DestroyTexture(item);
		mItems.Clear();
		string[] allAssets = AssetDatabase.GetAllAssetPaths();

		foreach (string s in allAssets)
		{
			if (s.EndsWith(".prefab") && s.Contains("Control -"))
				AddGUID(AssetDatabase.AssetPathToGUID(s), -1);
		}
	}

	/// <summary>
	/// Add a new item to the list.
	/// </summary>

	void AddItem (GameObject go, int index)
	{
		string guid = NGUIEditorTools.ObjectToGUID(go);

		if (string.IsNullOrEmpty(guid))
		{
			string path = EditorUtility.SaveFilePanelInProject("Save a prefab",
				"New Prefab.prefab", "prefab", "Save prefab as...", NGUISettings.currentPath);
			
			if (string.IsNullOrEmpty(path)) return;
			NGUISettings.currentPath = System.IO.Path.GetDirectoryName(path);

			go = PrefabUtility.CreatePrefab(path, go);
			if (go == null) return;

			guid = NGUIEditorTools.ObjectToGUID(go);
			if (string.IsNullOrEmpty(guid)) return;
		}

		Item ent = new Item();
		ent.prefab = go;
		ent.guid = guid;
		GeneratePreview(ent);
		if (index < mItems.size) mItems.Insert(index, ent);
		else mItems.Add(ent);
		Save();
	}

	/// <summary>
	/// Add a new item to the list.
	/// </summary>

	Item AddGUID (string guid, int index)
	{
		GameObject go = NGUIEditorTools.GUIDToObject<GameObject>(guid);

		if (go != null)
		{
			Item ent = new Item();
			ent.prefab = go;
			ent.guid = guid;
			GeneratePreview(ent);
			if (index < mItems.size) mItems.Insert(index, ent);
			else mItems.Add(ent);
			return ent;
		}
		return null;
	}

	/// <summary>
	/// Remove an existing item from the list.
	/// </summary>

	void RemoveItem (object obj)
	{
		if (this == null) return;
		int index = (int)obj;
		if (index < mItems.size && index > -1)
		{
			Item item = mItems[index];
			DestroyTexture(item);
			mItems.RemoveAt(index);
		}
		Save();
	}

	/// <summary>
	/// Find an item referencing the specified game object.
	/// </summary>

	Item FindItem (GameObject go)
	{
		for (int i = 0; i < mItems.size; ++i)
			if (mItems[i].prefab == go)
				return mItems[i];
		return null;
	}

	/// <summary>
	/// Save all the items to Editor Prefs.
	/// </summary>

	void Save ()
	{
		string data = "";

		if (mItems.size > 1)
		{
			string guid = mItems[0].guid;
			StringBuilder sb = new StringBuilder();
			sb.Append(guid);

			for (int i = 1; i < mItems.size; ++i)
			{
				guid = mItems[i].guid;

				if (string.IsNullOrEmpty(guid))
				{
					Debug.LogWarning("Unable to save " + mItems[i].prefab.name);
				}
				else
				{
					sb.Append('|');
					sb.Append(mItems[i].guid);
				}
			}
			data = sb.ToString();
		}
		NGUISettings.SetString("NGUI " + Application.dataPath, data);
	}

	/// <summary>
	/// Load all items from Editor Prefs.
	/// </summary>

	void Load ()
	{
		mMode = NGUISettings.GetEnum<Mode>("NGUI Prefab Mode", mMode);

		foreach (Item item in mItems) DestroyTexture(item);
		mItems.Clear();

		string data = NGUISettings.GetString("NGUI " + Application.dataPath, "");

		if (string.IsNullOrEmpty(data))
		{
			Reset();
		}
		else
		{
			if (string.IsNullOrEmpty(data)) return;
			string[] guids = data.Split('|');
			foreach (string s in guids) AddGUID(s, -1);
		}
	}

	/// <summary>
	/// Destroy the item's texture.
	/// </summary>

	void DestroyTexture (Item item)
	{
		if (item != null && item.dynamicTex && item.tex != null)
		{
			DestroyImmediate(item.tex);
			item.dynamicTex = false;
			item.tex = null;
		}
	}

	/// <summary>
	/// Update the visual mode based on the dragged object.
	/// </summary>

	void UpdateVisual ()
	{
		if (draggedObject == null) DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
		else if (draggedObjectIsOurs) DragAndDrop.visualMode = DragAndDropVisualMode.Move;
		else DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
	}

	/// <summary>
	/// Helper function that creates a new entry using the specified object's path.
	/// </summary>

	Item CreateItemByPath (string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			path = FileUtil.GetProjectRelativePath(path);
			string guid = AssetDatabase.AssetPathToGUID(path);

			if (!string.IsNullOrEmpty(guid))
			{
				GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
				Item ent = new Item();
				ent.prefab = go;
				ent.guid = guid;
				GeneratePreview(ent);
				return ent;
			}
			else Debug.Log("No GUID");
		}
		return null;
	}

	/// <summary>
	/// Generate an item preview for the specified item.
	/// </summary>

	void GeneratePreview (Item item)
	{
		if (item == null || item.prefab == null) return;

		if (item.tex != null && item.dynamicTex)
			DestroyImmediate(item.tex);

		item.tex = AssetPreview.GetAssetPreview(item.prefab);
		item.dynamicTex = false;
		if (item.tex != null) return;

		if (item.prefab.GetComponent<UIRect>() != null)
		{
			GameObject root = EditorUtility.CreateGameObjectWithHideFlags(
				"Preview Root", HideFlags.HideAndDontSave, typeof(UIPanel));

			GameObject camGO = EditorUtility.CreateGameObjectWithHideFlags(
				"Preview Camera", HideFlags.HideAndDontSave, typeof(Camera));

			root.transform.position = new Vector3(0f, 0f, 10000f);
			root.layer = item.prefab.layer;

			GameObject child = NGUITools.AddChild(root, item.prefab);

			Bounds b = NGUIMath.CalculateAbsoluteWidgetBounds(child.transform);
			Vector3 size = b.extents;
			camGO.transform.position = b.center;

			Camera cam = camGO.camera;
			cam.isOrthoGraphic = true;
			cam.cullingMask = (1 << root.layer);
			cam.nearClipPlane = -100f;
			cam.farClipPlane = 100f;
			cam.orthographicSize = Mathf.RoundToInt(Mathf.Max(size.x, size.y));
			cam.renderingPath = RenderingPath.Forward;
			cam.clearFlags = CameraClearFlags.Skybox;
			cam.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0f);

			Execute<UIWidget>("Start", root);
			Execute<UIPanel>("Start", root);
			Execute<UIWidget>("Update", root);
			Execute<UIPanel>("Update", root);
			Execute<UIPanel>("LateUpdate", root);

			RenderTexture rt = new RenderTexture(cellSize - 4, cellSize - 4, 1);
			rt.hideFlags = HideFlags.HideAndDontSave;
			cam.targetTexture = rt;
			cam.Render();

			DestroyImmediate(camGO);
			DestroyImmediate(root);

			item.tex = rt;
			item.dynamicTex = true;
		}
		//else item.tex = (Texture2D)AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(item.prefab));
	}

	/// <summary>
	/// Helper function that executes the functions in a proper order.
	/// </summary>

	static void Execute<T> (string funcName, GameObject root) where T : Component
	{
		T[] comps = root.GetComponents<T>();

		foreach (T comp in comps)
		{
			MethodInfo method = comp.GetType().GetMethod(funcName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null) method.Invoke(comp, null);
		}

		Transform t = root.transform;
		for (int i = 0, imax = t.childCount; i < imax; ++i)
			Execute<T>(funcName, t.GetChild(i).gameObject);
	}

	/// <summary>
	/// Helper function that retrieves the index of the cell under the mouse.
	/// </summary>

	int GetCellUnderMouse (int spacingX, int spacingY)
	{
		Vector2 pos = Event.current.mousePosition + mPos;

		int topPadding = 40; // Account for mode and search bars
		int x = cellPadding, y = cellPadding + topPadding;
		if (pos.y < y) return -1;

		float width = Screen.width - cellPadding + mPos.x;
		float height = Screen.height - cellPadding + mPos.y;
		int index = 0;

		for (; ; ++index)
		{
			Rect rect = new Rect(x, y, spacingX, spacingY);
			if (rect.Contains(pos)) break;

			x += spacingX;

			if (x + spacingX > width)
			{
				if (pos.x > x) return -1;
				y += spacingY;
				x = cellPadding;
				if (y + spacingY > height) break;
			}
		}
		return index;
	}

	bool mReset = false;

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
		Event currentEvent = Event.current;
		EventType type = currentEvent.type;

		int x = cellPadding, y = cellPadding;
		int width = Screen.width - cellPadding;
		int spacingX = cellSize + cellPadding;
		int spacingY = spacingX;
		if (mMode == Mode.DetailedMode) spacingY += 32;

		GameObject dragged = draggedObject;
		bool isDragging = (dragged != null);
		int indexUnderMouse = GetCellUnderMouse(spacingX, spacingY);
		Item selection = isDragging ? FindItem(dragged) : null;
		string searchFilter = NGUISettings.searchField;

		// Mode
		GUILayout.Space(4f);
		Mode modeAfter = (Mode)EditorGUILayout.EnumPopup(mMode);

		if (modeAfter != mMode)
		{
			mMode = modeAfter;
			mReset = true;
			NGUISettings.SetEnum("NGUI Prefab Mode", mMode);
		}

		if (mReset && type == EventType.Repaint)
		{
			mReset = false;
			foreach (Item item in mItems) GeneratePreview(item);
		}

		// Search field
		GUILayout.BeginHorizontal();
		{
			string after = EditorGUILayout.TextField("", searchFilter, "SearchTextField", GUILayout.Width(Screen.width - 20f));

			if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
			{
				after = "";
				GUIUtility.keyboardControl = 0;
			}

			if (searchFilter != after)
			{
				NGUISettings.searchField = after;
				searchFilter = after;
			}
		}
		GUILayout.EndHorizontal();

		if (type == EventType.MouseDown)
		{
			mMouseIsInside = true;
		}
		else if (type == EventType.MouseDrag)
		{
			mMouseIsInside = true;

			if (indexUnderMouse != -1)
			{
				// Drag operation begins
				if (draggedObjectIsOurs)
				{
					DragAndDrop.StartDrag("Prefab Tool");
				}
				currentEvent.Use();
			}
		}
		else if (type == EventType.MouseUp)
		{
			DragAndDrop.PrepareStartDrag();
			mMouseIsInside = false;
			Repaint();
		}
		else if (type == EventType.DragUpdated)
		{
			// Something dragged into the window
			mMouseIsInside = true;
			UpdateVisual();
			currentEvent.Use();
		}
		else if (type == EventType.DragPerform)
		{
			// We've dropped a new object into the window
			if (dragged != null)
			{
				if (selection != null)
				{
					DestroyTexture(selection);
					mItems.Remove(selection);
				}

				AddItem(dragged, indexUnderMouse);
				draggedObject = null;
			}
			mMouseIsInside = false;
			currentEvent.Use();
		}
		else if (type == EventType.DragExited || type == EventType.Ignore)
		{
			mMouseIsInside = false;
		}

		// If the mouse is not inside the window, clear the selection and dragged object
		if (!mMouseIsInside)
		{
			selection = null;
			dragged = null;
		}

		// Create a list of indices, inserting an entry of '-1' underneath the dragged object
		BetterList<int> indices = new BetterList<int>();

		for (int i = 0; i < mItems.size; )
		{
			if (dragged != null && indices.size == indexUnderMouse)
				indices.Add(-1);

			if (mItems[i] != selection)
			{
				if (string.IsNullOrEmpty(searchFilter) ||
					mItems[i].prefab.name.IndexOf(searchFilter, System.StringComparison.CurrentCultureIgnoreCase) != -1)
						indices.Add(i);
			}
			++i;
		}

		// There must always be '-1' (Add/Move slot) present
		if (!indices.Contains(-1)) indices.Add(-1);

		// We want to start dragging something from within the window
		if (type == EventType.MouseDown && indexUnderMouse > -1)
		{
			GUIUtility.keyboardControl = 0;

			if (currentEvent.button == 0 && indexUnderMouse < indices.size)
			{
				int index = indices[indexUnderMouse];

				if (index != -1 && index < mItems.size)
				{
					selection = mItems[index];
					draggedObject = selection.prefab;
					dragged = selection.prefab;
					currentEvent.Use();
				}
			}
		}
		//else if (type == EventType.MouseUp && currentEvent.button == 1 && indexUnderMouse > mItems.size)
		//{
		//    NGUIContextMenu.AddItem("Reset", false, RemoveItem, index);
		//    NGUIContextMenu.Show();
		//}

		// Draw the scroll view with prefabs
		mPos = GUILayout.BeginScrollView(mPos);
		{
			Color normal = new Color(1f, 1f, 1f, 0.5f);

			for (int i = 0; i < indices.size; ++i)
			{
				int index = indices[i];
				Item ent = (index != -1) ? mItems[index] : selection;

				if (ent == null || ent.prefab == null)
				{
					mItems.RemoveAt(index);
					continue;
				}

				Rect rect = new Rect(x, y, cellSize, cellSize);
				Rect inner = rect;
				inner.xMin += 2f;
				inner.xMax -= 2f;
				inner.yMin += 2f;
				inner.yMax -= 2f;
				rect.yMax -= 1f; // Button seems to be mis-shaped. It's height is larger than its width by a single pixel.

				GUI.backgroundColor = normal;

				if (!isDragging && (mMode == Mode.CompactMode || (ent == null || ent.tex != null)))
					mContent.tooltip = (ent != null) ? ent.prefab.name : "Click to add";
				else mContent.tooltip = "";

				if (ent == selection)
				{
					GUI.contentColor = normal;
					NGUIEditorTools.DrawTiledTexture(inner, NGUIEditorTools.backdropTexture);
				}

				GUI.contentColor = Color.white;

				if (GUI.Button(rect, mContent, "Button"))
				{
					if (ent == null || currentEvent.button == 0)
					{
						string path = EditorUtility.OpenFilePanel("Add a prefab", NGUISettings.currentPath, "prefab");

						if (!string.IsNullOrEmpty(path))
						{
							NGUISettings.currentPath = System.IO.Path.GetDirectoryName(path);
							Item newEnt = CreateItemByPath(path);

							if (newEnt != null)
							{
								mItems.Add(newEnt);
								Save();
							}
						}
					}
					else if (currentEvent.button == 1)
					{
						NGUIContextMenu.AddItem("Delete", false, RemoveItem, index);
						NGUIContextMenu.Show();
					}
				}

				string caption = (ent == null) ? "" : ent.prefab.name.Replace("Control - ", "");

				if (ent != null)
				{
					if (ent.tex != null)
					{
						GUI.DrawTexture(inner, ent.tex);
					}
					else if (mMode != Mode.DetailedMode)
					{
						GUI.Label(inner, caption, mStyle);
						caption = "";
					}
				}
				else GUI.Label(inner, "Add", mStyle);

				if (mMode == Mode.DetailedMode)
				{
					GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
					GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
					GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), caption, "ProgressBarBack");
					GUI.contentColor = Color.white;
					GUI.backgroundColor = Color.white;
				}

				x += spacingX;

				if (x + spacingX > width)
				{
					y += spacingY;
					x = cellPadding;
				}
			}
			GUILayout.Space(y);
		}
		GUILayout.EndScrollView();
	}
}
