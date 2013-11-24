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
#region Selection

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

	static public GameObject SelectedRoot () { return NGUIEditorTools.SelectedRoot(); }

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

#endregion
#region Create

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

	[MenuItem("NGUI/Create/")]
	static void AddBreaker123 () {}

	[MenuItem("NGUI/Create/Anchor")]
	static void AddAnchor2 () { Add<UIAnchor>(); }

	[MenuItem("NGUI/Create/Panel")]
	static void AddPanel ()
	{
		UIPanel panel = NGUISettings.AddPanel(SelectedRoot());
		Selection.activeGameObject = (panel == null) ? NGUIEditorTools.SelectedRoot(true) : panel.gameObject;
	}

	[MenuItem("NGUI/Create/Scroll View")]
	static void AddScrollView ()
	{
		UIPanel panel = NGUISettings.AddPanel(SelectedRoot());
		if (panel == null) panel = NGUIEditorTools.SelectedRoot(true).GetComponent<UIPanel>();
		panel.clipping = UIDrawCall.Clipping.SoftClip;
		Selection.activeGameObject = panel.gameObject;
	}

	[MenuItem("NGUI/Create/Grid")]
	static void AddGrid () { Add<UIGrid>(); }

	[MenuItem("NGUI/Create/Table")]
	static void AddTable () { Add<UITable>(); }

	static T Add<T> () where T : MonoBehaviour
	{
		T t = NGUITools.AddChild<T>(SelectedRoot());
		Selection.activeGameObject = t.gameObject;
		return t;
	}

#endregion
#region Attach

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

	static void AddIfMissing<T> () where T : Component
	{
		GameObject go = Selection.activeGameObject;
		if (go != null) go.AddMissingComponent<T>();
		else Debug.Log("You must select a game object first.");
	}

	static bool Exists<T> () where T : Component
	{
		GameObject go = Selection.activeGameObject;
		if (go != null) return go.GetComponent<T>() != null;
		return false;
	}

	[MenuItem("NGUI/Attach/Anchor")]
	static public void Add1 () { AddIfMissing<UIAnchor>(); }

	[MenuItem("NGUI/Attach/Anchor", true)]
	static public bool Add1a () { return !Exists<UIAnchor>(); }

	[MenuItem("NGUI/Attach/Stretch")]
	static public void Add2 () { AddIfMissing<UIStretch>(); }

	[MenuItem("NGUI/Attach/Stretch", true)]
	static public bool Add2a () { return !Exists<UIStretch>(); }

	[MenuItem("NGUI/Attach/")]
	static public void Add3s () {}

	[MenuItem("NGUI/Attach/Button Script")]
	static public void Add3 () { AddIfMissing<UIButton>(); }

	[MenuItem("NGUI/Attach/Toggle Script")]
	static public void Add4 () { AddIfMissing<UIToggle>(); }

	[MenuItem("NGUI/Attach/Slider Script")]
	static public void Add5 () { AddIfMissing<UISlider>(); }

	[MenuItem("NGUI/Attach/Scroll Bar Script")]
	static public void Add6 () { AddIfMissing<UIScrollBar>(); }

	[MenuItem("NGUI/Attach/Progress Bar Script")]
	static public void Add7 () { AddIfMissing<UIProgressBar>(); }

	[MenuItem("NGUI/Attach/Popup List Script")]
	static public void Add8 () { AddIfMissing<UIPopupList>(); }

	[MenuItem("NGUI/Attach/Input Field Script")]
	static public void Add9 () { AddIfMissing<UIInput>(); }
	
	[MenuItem("NGUI/Attach/Key Binding Script")]
	static public void Add10 () { AddIfMissing<UIKeyBinding>(); }

	[MenuItem("NGUI/Attach/Play Tween Script")]
	static public void Add11 () { AddIfMissing<UIPlayTween>(); }

	[MenuItem("NGUI/Attach/Play Animation Script")]
	static public void Add12 () { AddIfMissing<UIPlayAnimation>(); }

#endregion
#region Tweens

	[MenuItem("NGUI/Tween/Alpha")]
	static void Tween1 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenAlpha>(); }

	[MenuItem("NGUI/Tween/Alpha", true)]
	static bool Tween1a () { return (Selection.activeGameObject != null) && (Selection.activeGameObject.GetComponent<UIWidget>() != null); }

	[MenuItem("NGUI/Tween/Color")]
	static void Tween2 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenColor>(); }

	[MenuItem("NGUI/Tween/Color", true)]
	static bool Tween2a () { return (Selection.activeGameObject != null) && (Selection.activeGameObject.GetComponent<UIWidget>() != null); }

	[MenuItem("NGUI/Tween/Width")]
	static void Tween3 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenWidth>(); }

	[MenuItem("NGUI/Tween/Width", true)]
	static bool Tween3a () { return (Selection.activeGameObject != null) && (Selection.activeGameObject.GetComponent<UIWidget>() != null); }

	[MenuItem("NGUI/Tween/Height")]
	static void Tween4 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenHeight>(); }

	[MenuItem("NGUI/Tween/Height", true)]
	static bool Tween4a () { return (Selection.activeGameObject != null) && (Selection.activeGameObject.GetComponent<UIWidget>() != null); }

	[MenuItem("NGUI/Tween/Position")]
	static void Tween5 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenPosition>(); }

	[MenuItem("NGUI/Tween/Rotation")]
	static void Tween6 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenRotation>(); }

	[MenuItem("NGUI/Tween/Scale")]
	static void Tween7 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenScale>(); }

	[MenuItem("NGUI/Tween/Transform")]
	static void Tween8 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenTransform>(); }

	[MenuItem("NGUI/Tween/Volume")]
	static void Tween9 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenVolume>(); }

	[MenuItem("NGUI/Tween/Volume", true)]
	static bool Tween9a () { return (Selection.activeGameObject != null) && (Selection.activeGameObject.GetComponent<AudioSource>() != null); }

	[MenuItem("NGUI/Tween/Field of View")]
	static void Tween10 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenFOV>(); }

	[MenuItem("NGUI/Tween/Field of View", true)]
	static bool Tween10a () { return (Selection.activeGameObject != null) && (Selection.activeGameObject.GetComponent<Camera>() != null); }

	[MenuItem("NGUI/Tween/Orthographic Size")]
	static void Tween11 () { if (Selection.activeGameObject != null) Selection.activeGameObject.AddMissingComponent<TweenOrthoSize>(); }

	[MenuItem("NGUI/Tween/Orthographic Size", true)]
	static bool Tween11a () { return (Selection.activeGameObject != null) && (Selection.activeGameObject.GetComponent<Camera>() != null); }

#endregion
#region Open

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

#endregion
#region Handles

	[MenuItem("NGUI/Options/Handles/Turn On", true)]
	static public bool TurnHandlesOnCheck () { return !UIWidget.showHandlesWithMoveTool; }

	[MenuItem("NGUI/Options/Handles/Turn On")]
	static public void TurnHandlesOn () { UIWidget.showHandlesWithMoveTool = true; }

	[MenuItem("NGUI/Options/Handles/Turn Off", true)]
	static public bool TurnHandlesOffCheck () { return UIWidget.showHandlesWithMoveTool; }

	[MenuItem("NGUI/Options/Handles/Turn Off")]
	static public void TurnHandlesOff () { UIWidget.showHandlesWithMoveTool = false; }

	[MenuItem("NGUI/Options/Handles/Set to Blue", true)]
	static public bool SetToBlueCheck () { return UIWidget.showHandlesWithMoveTool && NGUISettings.colorMode != NGUISettings.ColorMode.Blue; }

	[MenuItem("NGUI/Options/Handles/Set to Blue")]
	static public void SetToBlue () { NGUISettings.colorMode = NGUISettings.ColorMode.Blue; }

	[MenuItem("NGUI/Options/Handles/Set to Orange", true)]
	static public bool SetToOrangeCheck () { return UIWidget.showHandlesWithMoveTool && NGUISettings.colorMode != NGUISettings.ColorMode.Orange; }

	[MenuItem("NGUI/Options/Handles/Set to Orange")]
	static public void SetToOrange () { NGUISettings.colorMode = NGUISettings.ColorMode.Orange; }

	[MenuItem("NGUI/Handles/Set to Green", true)]
	static public bool SetToGreenCheck () { return UIWidget.showHandlesWithMoveTool && NGUISettings.colorMode != NGUISettings.ColorMode.Green; }

	[MenuItem("NGUI/Options/Handles/Set to Green")]
	static public void SetToGreen () { NGUISettings.colorMode = NGUISettings.ColorMode.Green; }

#endregion

#region Snapping
	
	[MenuItem("NGUI/Options/Snapping/Turn On", true)]
	static public bool TurnSnapOnCheck () { return !NGUISnap.allow; }

	[MenuItem("NGUI/Options/Snapping/Turn On")]
	static public void TurnSnapOn () { NGUISnap.allow = true; }

	[MenuItem("NGUI/Options/Snapping/Turn Off", true)]
	static public bool TurnSnapOffCheck () { return NGUISnap.allow; }

	[MenuItem("NGUI/Options/Snapping/Turn Off")]
	static public void TurnSnapOff () { NGUISnap.allow = false; }

#endregion

	[MenuItem("NGUI/Normalize Depth Hierarchy &#0")]
	static public void Normalize () { NGUITools.NormalizeDepths(); }
	
	[MenuItem("NGUI/")]
	static void Breaker () { }

	[MenuItem("NGUI/Help")]
	static public void Help () { NGUIHelp.Show(); }
}
