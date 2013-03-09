//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UIWidgets.
/// </summary>

[CustomEditor(typeof(UIWidget))]
public class UIWidgetInspector : Editor
{
	protected UIWidget mWidget;
	static protected bool mUseShader = false;

	bool mInitialized = false;
	bool mDepthCheck = false;

	/// <summary>
	/// Register an Undo command with the Unity editor.
	/// </summary>

	void RegisterUndo()
	{
		NGUIEditorTools.RegisterUndo("Widget Change", mWidget);
	}

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		EditorGUIUtility.LookLikeControls(80f);
		mWidget = target as UIWidget;

		if (!mInitialized)
		{
			mInitialized = true;
			OnInit();
		}

		NGUIEditorTools.DrawSeparator();

		// Check to see if we can draw the widget's default properties to begin with
		if (OnDrawProperties())
		{
			// Draw all common properties next
			DrawCommonProperties();
		}
	}

	/// <summary>
	/// All widgets have depth, color and make pixel-perfect options
	/// </summary>

	protected void DrawCommonProperties ()
	{
#if UNITY_3_4
		PrefabType type = EditorUtility.GetPrefabType(mWidget.gameObject);
#else
		PrefabType type = PrefabUtility.GetPrefabType(mWidget.gameObject);
#endif

		NGUIEditorTools.DrawSeparator();

		// Pixel-correctness
		if (type != PrefabType.Prefab)
		{
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Correction");

				if (GUILayout.Button("Make Pixel-Perfect"))
				{
					NGUIEditorTools.RegisterUndo("Make Pixel-Perfect", mWidget.transform);
					mWidget.MakePixelPerfect();
				}
			}
			GUILayout.EndHorizontal();
		}

		// Pivot point -- old school drop-down style
		//UIWidget.Pivot pivot = (UIWidget.Pivot)EditorGUILayout.EnumPopup("Pivot", mWidget.pivot);

		//if (mWidget.pivot != pivot)
		//{
		//    NGUIEditorTools.RegisterUndo("Pivot Change", mWidget);
		//    mWidget.pivot = pivot;
		//}

		// Pivot point -- the new, more visual style
		GUILayout.BeginHorizontal();
		GUILayout.Label("Pivot", GUILayout.Width(76f));
		Toggle("◄", "ButtonLeft", UIWidget.Pivot.Left, true);
		Toggle("▬", "ButtonMid", UIWidget.Pivot.Center, true);
		Toggle("►", "ButtonRight", UIWidget.Pivot.Right, true);
		Toggle("▲", "ButtonLeft", UIWidget.Pivot.Top, false);
		Toggle("▌", "ButtonMid", UIWidget.Pivot.Center, false);
		Toggle("▼", "ButtonRight", UIWidget.Pivot.Bottom, false);
		GUILayout.EndHorizontal();

		// Depth navigation
		if (type != PrefabType.Prefab)
		{
			GUILayout.Space(2f);
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Depth");

				int depth = mWidget.depth;
				if (GUILayout.Button("Back", GUILayout.Width(60f))) --depth;
				depth = EditorGUILayout.IntField(depth);
				if (GUILayout.Button("Forward", GUILayout.Width(60f))) ++depth;

				if (mWidget.depth != depth)
				{
					NGUIEditorTools.RegisterUndo("Depth Change", mWidget);
					mWidget.depth = depth;
					mDepthCheck = true;
				}
			}
			GUILayout.EndHorizontal();

			UIPanel panel = mWidget.panel;

			if (panel != null)
			{
				int count = 0;

				for (int i = 0; i < panel.widgets.size; ++i)
				{
					UIWidget w = panel.widgets[i];
					if (w != null && w.depth == mWidget.depth && w.material == mWidget.material) ++count;
				}

				if (count > 1)
				{
					EditorGUILayout.HelpBox(count + " widgets are using the depth value of " + mWidget.depth +
						". It may not be clear what should be in front of what.", MessageType.Warning);
				}

				if (mDepthCheck)
				{
					if (panel.drawCalls.size > 1)
					{
						EditorGUILayout.HelpBox("The widgets underneath this panel are using more than one atlas. You may need to adjust transform position's Z value instead. When adjusting the Z, lower value means closer to the camera.", MessageType.Warning);
					}
				}
			}
		}

		NGUIEditorTools.DrawSeparator();

		// Color tint
		GUILayout.BeginHorizontal();
		Color color = EditorGUILayout.ColorField("Color Tint", mWidget.color);
		if (GUILayout.Button("Copy", GUILayout.Width(50f)))
			NGUISettings.color = color;
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		NGUISettings.color = EditorGUILayout.ColorField("Clipboard", NGUISettings.color);
		if (GUILayout.Button("Paste", GUILayout.Width(50f)))
			color = NGUISettings.color;
		GUILayout.EndHorizontal();

		if (mWidget.color != color)
		{
			NGUIEditorTools.RegisterUndo("Color Change", mWidget);
			mWidget.color = color;
		}
	}

	/// <summary>
	/// Draw a toggle button for the pivot point.
	/// </summary>

	void Toggle (string text, string style, UIWidget.Pivot pivot, bool isHorizontal)
	{
		bool isActive = false;

		switch (pivot)
		{
			case UIWidget.Pivot.Left:
			isActive = IsLeft(mWidget.pivot);
			break;

			case UIWidget.Pivot.Right:
			isActive = IsRight(mWidget.pivot);
			break;

			case UIWidget.Pivot.Top:
			isActive = IsTop(mWidget.pivot);
			break;

			case UIWidget.Pivot.Bottom:
			isActive = IsBottom(mWidget.pivot);
			break;

			case UIWidget.Pivot.Center:
			isActive = isHorizontal ? pivot == GetHorizontal(mWidget.pivot) : pivot == GetVertical(mWidget.pivot);
			break;
		}

		if (GUILayout.Toggle(isActive, text, style) != isActive)
			SetPivot(pivot, isHorizontal);
	}

	static bool IsLeft (UIWidget.Pivot pivot)
	{
		return pivot == UIWidget.Pivot.Left ||
			pivot == UIWidget.Pivot.TopLeft ||
			pivot == UIWidget.Pivot.BottomLeft;
	}

	static bool IsRight (UIWidget.Pivot pivot)
	{
		return pivot == UIWidget.Pivot.Right ||
			pivot == UIWidget.Pivot.TopRight ||
			pivot == UIWidget.Pivot.BottomRight;
	}

	static bool IsTop (UIWidget.Pivot pivot)
	{
		return pivot == UIWidget.Pivot.Top ||
			pivot == UIWidget.Pivot.TopLeft ||
			pivot == UIWidget.Pivot.TopRight;
	}

	static bool IsBottom (UIWidget.Pivot pivot)
	{
		return pivot == UIWidget.Pivot.Bottom ||
			pivot == UIWidget.Pivot.BottomLeft ||
			pivot == UIWidget.Pivot.BottomRight;
	}

	static UIWidget.Pivot GetHorizontal (UIWidget.Pivot pivot)
	{
		if (IsLeft(pivot)) return UIWidget.Pivot.Left;
		if (IsRight(pivot)) return UIWidget.Pivot.Right;
		return UIWidget.Pivot.Center;
	}

	static UIWidget.Pivot GetVertical (UIWidget.Pivot pivot)
	{
		if (IsTop(pivot)) return UIWidget.Pivot.Top;
		if (IsBottom(pivot)) return UIWidget.Pivot.Bottom;
		return UIWidget.Pivot.Center;
	}

	static UIWidget.Pivot Combine (UIWidget.Pivot horizontal, UIWidget.Pivot vertical)
	{
		if (horizontal == UIWidget.Pivot.Left)
		{
			if (vertical == UIWidget.Pivot.Top) return UIWidget.Pivot.TopLeft;
			if (vertical == UIWidget.Pivot.Bottom) return UIWidget.Pivot.BottomLeft;
			return UIWidget.Pivot.Left;
		}

		if (horizontal == UIWidget.Pivot.Right)
		{
			if (vertical == UIWidget.Pivot.Top) return UIWidget.Pivot.TopRight;
			if (vertical == UIWidget.Pivot.Bottom) return UIWidget.Pivot.BottomRight;
			return UIWidget.Pivot.Right;
		}
		return vertical;
	}

	void SetPivot (UIWidget.Pivot pivot, bool isHorizontal)
	{
		UIWidget.Pivot horizontal = GetHorizontal(mWidget.pivot);
		UIWidget.Pivot vertical = GetVertical(mWidget.pivot);

		pivot = isHorizontal ? Combine(pivot, vertical) : Combine(horizontal, pivot);

		if (mWidget.pivot != pivot)
		{
			NGUIEditorTools.RegisterUndo("Pivot change", mWidget);
			mWidget.pivot = pivot;
		}
	}

	/// <summary>
	/// Any and all derived functionality.
	/// </summary>

	protected virtual void OnInit() { }
	protected virtual bool OnDrawProperties () { return true; }
}
