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

static public class NGUIHelp
{
	[MenuItem("CONTEXT/UIWidget/Help")]
	static void ShowHelp1 (UnityEditor.MenuCommand command) { Show(command.context); }

	[MenuItem("CONTEXT/UIButton/Help")]
	static void ShowHelp2 (UnityEditor.MenuCommand command) { Show(typeof(UIButton)); }

	[MenuItem("CONTEXT/UIToggle/Help")]
	static void ShowHelp3 (UnityEditor.MenuCommand command) { Show(typeof(UIToggle)); }

	[MenuItem("CONTEXT/UIRoot/Help")]
	static void ShowHelp4 (UnityEditor.MenuCommand command) { Show(typeof(UIRoot)); }

	[MenuItem("CONTEXT/UICamera/Help")]
	static void ShowHelp5 (UnityEditor.MenuCommand command) { Show(typeof(UICamera)); }

	[MenuItem("CONTEXT/UIAnchor/Help")]
	static void ShowHelp6 (UnityEditor.MenuCommand command) { Show(typeof(UIAnchor)); }

	[MenuItem("CONTEXT/UIStretch/Help")]
	static void ShowHelp7 (UnityEditor.MenuCommand command) { Show(typeof(UIStretch)); }

	[MenuItem("CONTEXT/UISlider/Help")]
	static void ShowHelp8 (UnityEditor.MenuCommand command) { Show(typeof(UISlider)); }

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
	[MenuItem("CONTEXT/UI2DSprite/Help")]
	static void ShowHelp9 (UnityEditor.MenuCommand command) { Show(typeof(UI2DSprite)); }
#endif

	[MenuItem("CONTEXT/UIScrollBar/Help")]
	static void ShowHelp10 (UnityEditor.MenuCommand command) { Show(typeof(UIScrollBar)); }

	[MenuItem("CONTEXT/UIProgressBar/Help")]
	static void ShowHelp11 (UnityEditor.MenuCommand command) { Show(typeof(UIProgressBar)); }

	[MenuItem("CONTEXT/UIPopupList/Help")]
	static void ShowHelp12 (UnityEditor.MenuCommand command) { Show(typeof(UIPopupList)); }

	[MenuItem("CONTEXT/UIInputField/Help")]
	static void ShowHelp13 (UnityEditor.MenuCommand command) { Show(typeof(UIInput)); }

	[MenuItem("CONTEXT/UIInputField/Help")]
	static void ShowHelp14 (UnityEditor.MenuCommand command) { Show(typeof(UIInput)); }

	[MenuItem("CONTEXT/UIGrid/Help")]
	static void ShowHelp15 (UnityEditor.MenuCommand command) { Show(typeof(UIGrid)); }

	[MenuItem("CONTEXT/UITable/Help")]
	static void ShowHelp16 (UnityEditor.MenuCommand command) { Show(typeof(UITable)); }

	[MenuItem("CONTEXT/UIPlayTween/Help")]
	static void ShowHelp17 (UnityEditor.MenuCommand command) { Show(typeof(UIPlayTween)); }

	[MenuItem("CONTEXT/UIPlayAnimation/Help")]
	static void ShowHelp18 (UnityEditor.MenuCommand command) { Show(typeof(UIPlayAnimation)); }

	[MenuItem("CONTEXT/UIPlaySound/Help")]
	static void ShowHelp19 (UnityEditor.MenuCommand command) { Show(typeof(UIPlaySound)); }

	[MenuItem("CONTEXT/UIDraggablePanel/Help")]
	static void ShowHelp20 (UnityEditor.MenuCommand command) { Show(typeof(UIDraggablePanel)); }

	[MenuItem("CONTEXT/UIDragPanelContents/Help")]
	static void ShowHelp21 (UnityEditor.MenuCommand command) { Show(typeof(UIDragPanelContents)); }

	[MenuItem("CONTEXT/UICenterOnChild/Help")]
	static void ShowHelp22 (UnityEditor.MenuCommand command) { Show(typeof(UICenterOnChild)); }

	[MenuItem("CONTEXT/UICenterOnClick/Help")]
	static void ShowHelp23 (UnityEditor.MenuCommand command) { Show(typeof(UICenterOnClick)); }

	/// <summary>
	/// Get the URL pointing to the documentation for the specified component.
	/// </summary>

	static public string GetHelpURL (Type type)
	{
		if (type == typeof(UIWidget))		return "http://www.tasharen.com/forum/index.php?topic=6702";
		if (type == typeof(UITexture))		return "http://www.tasharen.com/forum/index.php?topic=6703";
		if (type == typeof(UISprite))		return "http://www.tasharen.com/forum/index.php?topic=6704";
		if (type == typeof(UIPanel))		return "http://www.tasharen.com/forum/index.php?topic=6705";
		if (type == typeof(UILabel))		return "http://www.tasharen.com/forum/index.php?topic=6706";
		if (type == typeof(UIButton))		return "http://www.tasharen.com/forum/index.php?topic=6708";
		if (type == typeof(UIToggle))		return "http://www.tasharen.com/forum/index.php?topic=6709";
		if (type == typeof(UIRoot))			return "http://www.tasharen.com/forum/index.php?topic=6710";
		if (type == typeof(UICamera))		return "http://www.tasharen.com/forum/index.php?topic=6711";
		if (type == typeof(UIAnchor))		return "http://www.tasharen.com/forum/index.php?topic=6712";
		if (type == typeof(UIStretch))		return "http://www.tasharen.com/forum/index.php?topic=6713";
		if (type == typeof(UISlider))		return "http://www.tasharen.com/forum/index.php?topic=6715";
#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
		if (type == typeof(UI2DSprite))		return "http://www.tasharen.com/forum/index.php?topic=6729";
#endif
		if (type == typeof(UIScrollBar))	return "http://www.tasharen.com/forum/index.php?topic=6733";
		if (type == typeof(UIProgressBar))	return "http://www.tasharen.com/forum/index.php?topic=6738";
		if (type == typeof(UIPopupList))	return "http://www.tasharen.com/forum/index.php?topic=6751";
		return null;
	}

	/// <summary>
	/// Show help for the specific topic.
	/// </summary>

	static public void Show (Type type)
	{
		string url = GetHelpURL(type);
		if (url == null) url = "http://www.tasharen.com/ngui/doc.php?topic=" + type;
		Application.OpenURL(url);
	}

	/// <summary>
	/// Show help for the specific topic.
	/// </summary>

	static public void Show (object obj)
	{
		if (obj is GameObject)
		{
			GameObject go = obj as GameObject;
			UIWidget widget = go.GetComponent<UIWidget>();

			if (widget != null)
			{
				Show(widget.GetType());
				return;
			}
		}
		Show(obj.GetType());
	}
}
