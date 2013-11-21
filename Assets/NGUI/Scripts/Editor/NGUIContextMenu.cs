using UnityEngine;
using UnityEditor;

/// <summary>
/// This editor helper class makes it easy to create and show a context menu.
/// It ensures that it's possible to add multiple items with the same name.
/// </summary>

public static class NGUIContextMenu
{
	public delegate UIWidget AddFunc (GameObject go);

	static BetterList<string> mEntries = new BetterList<string>();
	static GenericMenu mMenu;

	/// <summary>
	/// Clear the context menu list.
	/// </summary>

	static public void Clear ()
	{
		mEntries.Clear();
		mMenu = null;
	}

	/// <summary>
	/// Add a new context menu entry.
	/// </summary>

	static public void AddItem (string item, bool isChecked, GenericMenu.MenuFunction2 callback, object param)
	{
		if (callback != null)
		{
			if (mMenu == null) mMenu = new GenericMenu();
			int count = 0;

			for (int i = 0; i < mEntries.size; ++i)
			{
				string str = mEntries[i];
				if (str == item) ++count;
			}
			mEntries.Add(item);

			if (count > 0) item += " [" + count + "]";
			mMenu.AddItem(new GUIContent(item), isChecked, callback, param);
		}
		else AddDisabledItem(item);
	}

	/// <summary>
	/// Wrapper function called by the menu that in turn calls the correct callback.
	/// </summary>

	static void AddChild (object obj)
	{
		AddFunc func = obj as AddFunc;
		UIWidget widget = func(Selection.activeGameObject);
		if (widget != null) Selection.activeGameObject = widget.gameObject;
	}

	/// <summary>
	/// Add a new context menu entry.
	/// </summary>

	static void AddChildWidget (string item, bool isChecked, AddFunc callback)
	{
		if (callback != null)
		{
			if (mMenu == null) mMenu = new GenericMenu();
			int count = 0;

			for (int i = 0; i < mEntries.size; ++i)
			{
				string str = mEntries[i];
				if (str == item) ++count;
			}
			mEntries.Add(item);

			if (count > 0) item += " [" + count + "]";
			mMenu.AddItem(new GUIContent(item), isChecked, AddChild, callback);
		}
		else AddDisabledItem(item);
	}

	/// <summary>
	/// Wrapper function called by the menu that in turn calls the correct callback.
	/// </summary>

	static void AddSibling (object obj)
	{
		AddFunc func = obj as AddFunc;
		UIWidget widget = func(Selection.activeTransform.parent.gameObject);
		if (widget != null) Selection.activeGameObject = widget.gameObject;
	}

	/// <summary>
	/// Add a new context menu entry.
	/// </summary>

	static void AddSiblingWidget (string item, bool isChecked, AddFunc callback)
	{
		if (callback != null)
		{
			if (mMenu == null) mMenu = new GenericMenu();
			int count = 0;

			for (int i = 0; i < mEntries.size; ++i)
			{
				string str = mEntries[i];
				if (str == item) ++count;
			}
			mEntries.Add(item);

			if (count > 0) item += " [" + count + "]";
			mMenu.AddItem(new GUIContent(item), isChecked, AddSibling, callback);
		}
		else AddDisabledItem(item);
	}

	/// <summary>
	/// Add commonly NGUI context menu options.
	/// </summary>

	static public void AddCommonItems (GameObject target)
	{
		if (target != null)
		{
			UIWidget widget = target.GetComponent<UIWidget>();

			if (widget != null)
			{
				AddItem("Widget/Make Pixel-Perfect", false, OnMakePixelPerfect, Selection.activeTransform);

				if (target.GetComponent<BoxCollider>() != null)
				{
					AddItem("Widget/Reset Collider Size", false, OnBoxCollider, target);
				}
				else
				{
					AddItem("Widget/Add Box Collider", false, OnBoxCollider, target);
				}

				NGUIContextMenu.AddSeparator("Widget/");
				AddItem("Widget/Delete", false, OnDelete, target);
			}

			if (Selection.activeTransform.parent == null || widget == null)
			{
				AddChildWidget("Add/Sprite", false, NGUISettings.AddSprite);
				AddChildWidget("Add/Label", false, NGUISettings.AddLabel);
				NGUIContextMenu.AddSeparator("Add/");
				AddChildWidget("Add/Invisible Widget", false, NGUISettings.AddWidget);
				AddChildWidget("Add/Simple Texture", false, NGUISettings.AddTexture);
				AddChildWidget("Add/Unity 2D Sprite", false, NGUISettings.Add2DSprite);
			}
			else
			{
				AddChildWidget("Add/Sprite/Child", false, NGUISettings.AddSprite);
				AddChildWidget("Add/Label/Child", false, NGUISettings.AddLabel);
				NGUIContextMenu.AddSeparator("Add/");
				AddChildWidget("Add/Invisible Widget/Child", false, NGUISettings.AddWidget);
				AddChildWidget("Add/Simple Texture/Child", false, NGUISettings.AddTexture);
				AddChildWidget("Add/Unity 2D Sprite/Child", false, NGUISettings.Add2DSprite);

				AddSiblingWidget("Add/Sprite/Sibling", false, NGUISettings.AddSprite);
				AddSiblingWidget("Add/Label/Sibling", false, NGUISettings.AddLabel);
				AddSiblingWidget("Add/Invisible Widget/Sibling", false, NGUISettings.AddWidget);
				AddSiblingWidget("Add/Simple Texture/Sibling", false, NGUISettings.AddTexture);
				AddSiblingWidget("Add/Unity 2D Sprite/Sibling", false, NGUISettings.Add2DSprite);

				NGUIContextMenu.AddSeparator("Add/");

				bool anchor = target.GetComponent<UIAnchor>() != null;
				bool stretch = target.GetComponent<UIStretch>() != null;

				if (!anchor || !stretch)
				{
					if (!anchor) AddItem("Add/Anchor Script", false, delegate(object obj) { target.AddComponent<UIAnchor>(); }, null);
					if (!stretch) AddItem("Add/Stretch Script", false, delegate(object obj) { target.AddComponent<UIStretch>(); }, null);
				}

				AddItem("Add/Button Script", false, delegate(object obj) { target.AddComponent<UIButton>(); }, null);
				AddItem("Add/Toggle Script", false, delegate(object obj) { target.AddComponent<UIToggle>(); }, null);
				AddItem("Add/Slider Script", false, delegate(object obj) { target.AddComponent<UISlider>(); }, null);
			}

			AddHelp(target, false);
			NGUIContextMenu.AddSeparator("");
		}
	}

	/// <summary>
	/// Add help options based on the components present on the specified game object.
	/// </summary>

	static public void AddHelp (GameObject go, bool addSeparator)
	{
		MonoBehaviour[] comps = Selection.activeGameObject.GetComponents<MonoBehaviour>();

		for (int i = 0; i < comps.Length; ++i)
		{
			System.Type type = comps[i].GetType();
			string url = NGUIHelp.GetHelpURL(type);
			
			if (url != null)
			{
				if (addSeparator)
				{
					addSeparator = false;
					AddSeparator("");
				}

				AddItem("Help/" + type, false, delegate(object obj) { Application.OpenURL(url); }, null);
			}
		}
	}

	static void OnHelp (object obj) { NGUIHelp.Show(obj); }
	static void OnMakePixelPerfect (object obj) { NGUITools.MakePixelPerfect(obj as Transform); }
	static void OnBoxCollider (object obj) { NGUITools.AddWidgetCollider(obj as GameObject); }
	static void OnDelete (object obj)
	{
		GameObject go = obj as GameObject;
		Selection.activeGameObject = go.transform.parent.gameObject;
#if UNITY_3_5
		NGUITools.Destroy(go);
#else
		Undo.DestroyObjectImmediate(go);
#endif
	}

	/// <summary>
	/// Add a new disabled context menu entry.
	/// </summary>

	static public void AddDisabledItem (string item)
	{
		if (mMenu == null) mMenu = new GenericMenu();
		mMenu.AddDisabledItem(new GUIContent(item));
	}

	/// <summary>
	/// Add a separator to the menu.
	/// </summary>

	static public void AddSeparator (string path)
	{
		if (mMenu == null) mMenu = new GenericMenu();
		mMenu.AddSeparator(path);
	}

	/// <summary>
	/// Show the context menu with all the added items.
	/// </summary>

	static public void Show ()
	{
		if (mMenu != null)
		{
			mMenu.ShowAsContext();
			mMenu = null;
			mEntries.Clear();
		}
	}
}
