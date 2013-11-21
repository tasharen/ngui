//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

/// <summary>
/// This script adds the NGUI menu options to the Unity Editor.
/// </summary>

static public class NGUIMenu
{
	[MenuItem("CONTEXT/UIWidget/Help")]
	static void ShowHelpMenu (UnityEditor.MenuCommand command) { ShowHelp(command.context); }

	[MenuItem("CONTEXT/UIButton/Help")]
	static void ShowButtonHelp (UnityEditor.MenuCommand command) { ShowHelp(typeof(UIButton)); }

	[MenuItem("CONTEXT/UIToggle/Help")]
	static void ShowToggleHelp (UnityEditor.MenuCommand command) { ShowHelp(typeof(UIToggle)); }

	[MenuItem("CONTEXT/UIRoot/Help")]
	static void ShowRootHelp (UnityEditor.MenuCommand command) { ShowHelp(typeof(UIRoot)); }

	/// <summary>
	/// Show help for the specific topic.
	/// </summary>

	static public void ShowHelp (object obj)
	{
		if (obj is GameObject)
		{
			GameObject go = obj as GameObject;
			UIWidget widget = go.GetComponent<UIWidget>();

			if (widget != null)
			{
				ShowHelp(widget.GetType());
				return;
			}
		}
		ShowHelp(obj.GetType());
	}

	/// <summary>
	/// Show help for the specific topic.
	/// </summary>

	static public void ShowHelp (Type type)
	{
		if (type == typeof(UIWidget))
		{
			Application.OpenURL("http://www.tasharen.com/forum/index.php?topic=6702");
		}
		else if (type == typeof(UITexture))
		{
			Application.OpenURL("http://www.tasharen.com/forum/index.php?topic=6703");
		}
		else if (type == typeof(UISprite))
		{
			Application.OpenURL("http://www.tasharen.com/forum/index.php?topic=6704");
		}
		else if (type == typeof(UIPanel))
		{
			Application.OpenURL("http://www.tasharen.com/forum/index.php?topic=6705");
		}
		else if (type == typeof(UILabel))
		{
			Application.OpenURL("http://www.tasharen.com/forum/index.php?topic=6706");
		}
		else if (type == typeof(UIButton))
		{
			Application.OpenURL("http://www.tasharen.com/forum/index.php?topic=6708");
		}
		else if (type == typeof(UIToggle))
		{
			Application.OpenURL("http://www.tasharen.com/forum/index.php?topic=6709");
		}
		else if (type == typeof(UIRoot))
		{
			Application.OpenURL("http://www.tasharen.com/forum/index.php?topic=6710");
		}
		else
		{
			// TODO: Navigate to a more context-appropriate URL
			Application.OpenURL("http://www.tasharen.com/forum/index.php?board=1.0");
			Debug.Log(type);
		}
	}

	[MenuItem("NGUI/Selection/Bring To Front &#=")]
	static public void BringForward2 ()
	{
		int val = 0;
		for (int i = 0; i < Selection.gameObjects.Length; ++i)
			val |= NGUITools.AdjustDepth(Selection.gameObjects[i], 1000);

		if ((val & 1) != 0)
		{
			NGUITools.NormalizePanelDepths();
			if (UIPanelTool.instance != null)
				UIPanelTool.instance.Repaint();
		}
		if ((val & 2) != 0) NGUITools.NormalizeWidgetDepths();
	}

	[MenuItem("NGUI/Selection/Bring To Front &#=", true)]
	static public bool BringForward2Validation () { return (Selection.activeGameObject != null); }

	[MenuItem("NGUI/Selection/Push To Back &#-")]
	static public void PushBack2 ()
	{
		int val = 0;
		for (int i = 0; i < Selection.gameObjects.Length; ++i)
			val |= NGUITools.AdjustDepth(Selection.gameObjects[i], -1000);

		if ((val & 1) != 0)
		{
			NGUITools.NormalizePanelDepths();
			if (UIPanelTool.instance != null)
				UIPanelTool.instance.Repaint();
		}
		if ((val & 2) != 0) NGUITools.NormalizeWidgetDepths();
	}

	[MenuItem("NGUI/Selection/Push To Back &#-", true)]
	static public bool PushBack2Validation () { return (Selection.activeGameObject != null); }

	[MenuItem("NGUI/Selection/Adjust Depth By +1 %=")]
	static public void BringForward ()
	{
		int val = 0;
		for (int i = 0; i < Selection.gameObjects.Length; ++i)
			val |= NGUITools.AdjustDepth(Selection.gameObjects[i], 1);
		if (((val & 1) != 0) && UIPanelTool.instance != null)
			UIPanelTool.instance.Repaint();
	}

	[MenuItem("NGUI/Selection/Adjust Depth By +1 %=", true)]
	static public bool BringForwardValidation () { return (Selection.activeGameObject != null); }

	[MenuItem("NGUI/Selection/Adjust Depth By -1 %-")]
	static public void PushBack ()
	{
		int val = 0;
		for (int i = 0; i < Selection.gameObjects.Length; ++i)
			val |= NGUITools.AdjustDepth(Selection.gameObjects[i], -1);
		if (((val & 1) != 0) && UIPanelTool.instance != null)
			UIPanelTool.instance.Repaint();
	}

	[MenuItem("NGUI/Selection/Adjust Depth By -1 %-", true)]
	static public bool PushBackValidation () { return (Selection.activeGameObject != null); }

	/// <summary>
	/// Same as SelectedRoot(), but with a log message if nothing was found.
	/// </summary>

	static public GameObject SelectedRoot ()
	{
		GameObject go = NGUIEditorTools.SelectedRoot();

		if (go == null)
		{
			Debug.Log("No UI found. You can create a new one easily by using the UI creation wizard.\nOpening it for your convenience.");
			CreateUIWizard();
		}
		return go;
	}

	[MenuItem("NGUI/Create/Sprite &#s")]
	static public void AddSprite ()
	{
		GameObject go = NGUIEditorTools.SelectedRoot(true);

		if (go != null)
		{
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			Undo.RegisterSceneUndo("Add a Sprite");
#endif
			Selection.activeGameObject = NGUISettings.AddSprite(go).gameObject;
		}
		else
		{
			Debug.Log("You must select a game object first.");
		}
	}

	[MenuItem("NGUI/Create/Label &#l")]
	static public void AddLabel ()
	{
		GameObject go = NGUIEditorTools.SelectedRoot(true);

		if (go != null)
		{
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			Undo.RegisterSceneUndo("Add a Label");
#endif
			Selection.activeGameObject = NGUISettings.AddLabel(go).gameObject;
		}
		else
		{
			Debug.Log("You must select a game object first.");
		}
	}

	[MenuItem("NGUI/Create/Texture &#t")]
	static public void AddTexture ()
	{
		GameObject go = NGUIEditorTools.SelectedRoot(true);

		if (go != null)
		{
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			Undo.RegisterSceneUndo("Add a Texture");
#endif
			Selection.activeGameObject = NGUISettings.AddTexture(go).gameObject;
		}
		else
		{
			Debug.Log("You must select a game object first.");
		}
	}

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
	[MenuItem("NGUI/Create/Unity 2D Sprite &#r")]
	static public void AddSprite2D ()
	{
		GameObject go = NGUIEditorTools.SelectedRoot(true);
		if (go != null) Selection.activeGameObject = NGUISettings.Add2DSprite(go).gameObject;
		else Debug.Log("You must select a game object first.");
	}
#endif

	[MenuItem("NGUI/Create/Widget &#w")]
	static public void AddWidget ()
	{
		GameObject go = NGUIEditorTools.SelectedRoot(true);

		if (go != null)
		{
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			Undo.RegisterSceneUndo("Add a Widget");
#endif
			Selection.activeGameObject = NGUISettings.AddWidget(go).gameObject;
		}
		else
		{
			Debug.Log("You must select a game object first.");
		}
	}

	[MenuItem("NGUI/Create/Panel")]
	static public void AddPanel ()
	{
		GameObject go = SelectedRoot();

		if (NGUIEditorTools.WillLosePrefab(go))
		{
			NGUIEditorTools.RegisterUndo("Add a child UI Panel", go);

			GameObject child = new GameObject(NGUITools.GetTypeName<UIPanel>());
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

	[MenuItem("NGUI/Attach/Collider &#c")]
	static public void AddCollider ()
	{
		GameObject go = Selection.activeGameObject;

		if (NGUIEditorTools.WillLosePrefab(go))
		{
			if (go != null)
			{
				NGUIEditorTools.RegisterUndo("Add Widget Collider", go);
				NGUITools.AddWidgetCollider(go);
			}
			else
			{
				Debug.Log("You must select a game object first, such as your button.");
			}
		}
	}

	[MenuItem("NGUI/Attach/Anchor &#h")]
	static public void AddAnchor ()
	{
		GameObject go = Selection.activeGameObject;

		if (go != null)
		{
			NGUIEditorTools.RegisterUndo("Add an Anchor", go);
			if (go.GetComponent<UIAnchor>() == null) go.AddComponent<UIAnchor>();
		}
		else
		{
			Debug.Log("You must select a game object first.");
		}
	}

	[MenuItem("NGUI/Open/Atlas Maker")]
	[MenuItem("Assets/NGUI/Open Atlas Maker", false, 0)]
	static public void OpenAtlasMaker ()
	{
		EditorWindow.GetWindow<UIAtlasMaker>(false, "Atlas Maker", true);
	}

	[MenuItem("NGUI/Open/Font Maker")]
	[MenuItem("Assets/NGUI/Open Bitmap Font Maker", false, 0)]
	static public void OpenFontMaker ()
	{
		EditorWindow.GetWindow<UIFontMaker>(false, "Font Maker", true);
	}

	[MenuItem("NGUI/Open/Widget Wizard")]
	static public void CreateWidgetWizard ()
	{
		EditorWindow.GetWindow<UICreateWidgetWizard>(false, "Widget Tool", true);
	}

	[MenuItem("NGUI/Open/UI Wizard")]
	[MenuItem("Assets/NGUI/Open UI Wizard", false, 0)]
	static public void CreateUIWizard ()
	{
		EditorWindow.GetWindow<UICreateNewUIWizard>(false, "UI Tool", true);
	}

	[MenuItem("NGUI/Open/Panel Tool")]
	static public void OpenPanelWizard ()
	{
		EditorWindow.GetWindow<UIPanelTool>(false, "Panel Tool", true);
	}

	[MenuItem("NGUI/Open/Camera Tool")]
	static public void OpenCameraWizard ()
	{
		EditorWindow.GetWindow<UICameraTool>(false, "Camera Tool", true);
	}

	[MenuItem("NGUI/Handles/Turn On", true)]
	static public bool TurnHandlesOnCheck () { return !UIWidget.showHandlesWithMoveTool; }

	[MenuItem("NGUI/Handles/Turn On")]
	static public void TurnHandlesOn () { UIWidget.showHandlesWithMoveTool = true; }

	[MenuItem("NGUI/Handles/Turn Off", true)]
	static public bool TurnHandlesOffCheck () { return UIWidget.showHandlesWithMoveTool; }

	[MenuItem("NGUI/Handles/Turn Off")]
	static public void TurnHandlesOff () { UIWidget.showHandlesWithMoveTool = false; }

	[MenuItem("NGUI/Handles/Set to Blue", true)]
	static public bool SetToBlueCheck () { return UIWidget.showHandlesWithMoveTool && NGUISettings.colorMode != NGUISettings.ColorMode.Blue; }

	[MenuItem("NGUI/Handles/Set to Blue")]
	static public void SetToBlue () { NGUISettings.colorMode = NGUISettings.ColorMode.Blue; }

	[MenuItem("NGUI/Handles/Set to Orange", true)]
	static public bool SetToOrangeCheck () { return UIWidget.showHandlesWithMoveTool && NGUISettings.colorMode != NGUISettings.ColorMode.Orange; }

	[MenuItem("NGUI/Handles/Set to Orange")]
	static public void SetToOrange () { NGUISettings.colorMode = NGUISettings.ColorMode.Orange; }

	[MenuItem("NGUI/Handles/Set to Green", true)]
	static public bool SetToGreenCheck () { return UIWidget.showHandlesWithMoveTool && NGUISettings.colorMode != NGUISettings.ColorMode.Green; }

	[MenuItem("NGUI/Handles/Set to Green")]
	static public void SetToGreen () { NGUISettings.colorMode = NGUISettings.ColorMode.Green; }

	[MenuItem("NGUI/Selection/Make Pixel Perfect &#p")]
	static void PixelPerfectSelection ()
	{
		foreach (Transform t in Selection.transforms)
			NGUITools.MakePixelPerfect(t);
	}

	[MenuItem("NGUI/Selection/Make Pixel Perfect &#p", true)]
	static bool PixelPerfectSelectionValidation ()
	{
		return (Selection.activeTransform != null);
	}

	[MenuItem("NGUI/Normalize Depth Hierarchy &#0")]
	static public void Normalize () { NGUITools.NormalizeDepths(); }
}
