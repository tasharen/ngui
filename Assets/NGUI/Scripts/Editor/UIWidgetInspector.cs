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

		// Depth navigation
		if (type != PrefabType.Prefab)
		{
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Depth");

				int depth = mWidget.depth;
				if (GUILayout.Button("Back")) --depth;
				depth = EditorGUILayout.IntField(depth, GUILayout.Width(40f));
				if (GUILayout.Button("Forward")) ++depth;

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

		// Pivot point
		UIWidget.Pivot pivot = (UIWidget.Pivot)EditorGUILayout.EnumPopup("Pivot", mWidget.pivot);

		if (mWidget.pivot != pivot)
		{
			NGUIEditorTools.RegisterUndo("Pivot Change", mWidget);
			mWidget.pivot = pivot;
		}

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

		// Color tint
		Color color = EditorGUILayout.ColorField("Color Tint", mWidget.color);

		if (mWidget.color != color)
		{
			NGUIEditorTools.RegisterUndo("Color Change", mWidget);
			mWidget.color = color;
		}
	}

	/// <summary>
	/// Any and all derived functionality.
	/// </summary>

	protected virtual void OnInit() { }
	protected virtual bool OnDrawProperties () { return true; }
}