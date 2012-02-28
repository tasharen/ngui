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
	bool mHierarchyCheck = true;
	protected bool mAllowPreview = true;

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

		// Check the hierarchy to ensure that this widget is not parented to another widget
		if (mHierarchyCheck) CheckHierarchy();

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
				}
			}
			GUILayout.EndHorizontal();
		}

		Color color = EditorGUILayout.ColorField("Color Tint", mWidget.color);

		if (mWidget.color != color)
		{
			NGUIEditorTools.RegisterUndo("Color Change", mWidget);
			mWidget.color = color;
		}

		// Depth navigation
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

		UIWidget.Pivot pivot = (UIWidget.Pivot)EditorGUILayout.EnumPopup("Pivot", mWidget.pivot);

		if (mWidget.pivot != pivot)
		{
			NGUIEditorTools.RegisterUndo("Pivot Change", mWidget);
			mWidget.pivot = pivot;
		}

		if (mAllowPreview && mWidget.mainTexture != null)
		{
			GUILayout.BeginHorizontal();
			{
				UISettings.texturePreview = EditorGUILayout.Toggle("Preview", UISettings.texturePreview, GUILayout.Width(100f));

				/*if (UISettings.texturePreview)
				{
					if (mUseShader != EditorGUILayout.Toggle("Use Shader", mUseShader))
					{
						mUseShader = !mUseShader;

						if (mUseShader)
						{
							// TODO: Remove this when Unity fixes the bug with DrawPreviewTexture not being affected by BeginGroup
							Debug.LogWarning("There is a bug in Unity that prevents the texture from getting clipped properly.\n" +
								"Until it's fixed by Unity, your texture may spill onto the rest of the Unity's GUI while using this mode.");
						}
					}
				}*/
			}
			GUILayout.EndHorizontal();

			// Draw the texture last
			if (UISettings.texturePreview) OnDrawTexture();
		}
	}

	/// <summary>
	/// Check the hierarchy to ensure that this widget is not parented to another widget.
	/// </summary>
 
	void CheckHierarchy()
	{
		mHierarchyCheck = false;
		if (Application.isPlaying) return;
		Transform trans = mWidget.transform.parent;
		if (trans == null) return;
		Vector3 scale = trans.lossyScale;

		if (Mathf.Abs(scale.x - scale.y) > 0.001f || Mathf.Abs(scale.y - scale.x) > 0.001f)
		{
			UIAnchor anch = trans.GetComponent<UIAnchor>();

			if (anch == null || !anch.stretchToFill)
			{
				Debug.LogWarning("Parent of " + NGUITools.GetHierarchy(mWidget.gameObject) + " does not have a uniform absolute scale.\n" +
					"Consider re-parenting to a uniformly-scaled game object instead.");

				// If the warning above gets triggered, it means that the widget's parent does not have a uniform scale.
				// This may lead to strangeness when scaling or rotating the widget. Consider this hierarchy:

				// Widget #1
				//  |
				//  +- Widget #2

				// You can change it to this, solving the problem:

				// GameObject (scale 1, 1, 1)
				//  |
				//  +- Widget #1
				//  |
				//  +- Widget #2
			}
		}
	}

	/// <summary>
	/// Any and all derived functionality.
	/// </summary>

	protected virtual void OnInit() { }
	protected virtual bool OnDrawProperties () { return true; }
	protected virtual void OnDrawTexture () { }
}