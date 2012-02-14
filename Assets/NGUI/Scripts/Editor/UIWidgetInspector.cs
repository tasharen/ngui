using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UIWidgets.
/// </summary>

[CustomEditor(typeof(UIWidget))]
public class UIWidgetInspector : Editor
{
	protected UIWidget mWidget;
	protected bool mRegisteredUndo = false;
	static protected bool mShowTexture = true;
	static protected bool mUseShader = false;

	bool mInitialized = false;
	bool mHierarchyCheck = true;

	/// <summary>
	/// Register an Undo command with the Unity editor.
	/// </summary>

	protected void RegisterUndo()
	{
		if (!mRegisteredUndo)
		{
			mRegisteredUndo = true;
			Undo.RegisterUndo(mWidget, "Widget Change");
		}
	}

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		EditorGUIUtility.LookLikeControls(80f);
		mWidget = target as UIWidget;

#if UNITY_3_4
		PrefabType type = EditorUtility.GetPrefabType(mWidget.gameObject);
#else
		PrefabType type = PrefabUtility.GetPrefabType(mWidget.gameObject);
#endif

		if (type == PrefabType.Prefab)
		{
			GUILayout.Label("Drag this widget into the scene to modify it.");
		}
		else
		{
			if (!mInitialized)
			{
				mInitialized = true;
				OnInit();
			}

			NGUIEditorTools.DrawSeparator();

			// Check the hierarchy to ensure that this widget is not parented to another widget
			if (mHierarchyCheck) CheckHierarchy();

			// This flag gets set to 'true' if RegisterUndo() gets called
			mRegisteredUndo = false;

			// Check to see if we can draw the widget's default properties to begin with
			if (OnDrawProperties())
			{
				// Draw all common properties next
				DrawCommonProperties();
			}

			// Update the widget's properties if something has changed
			if (mRegisteredUndo) mWidget.MarkAsChanged();
		}
	}

	/// <summary>
	/// All widgets have depth, color and make pixel-perfect options
	/// </summary>

	protected void DrawCommonProperties ()
	{
		NGUIEditorTools.DrawSeparator();

		// Depth navigation
		GUILayout.BeginHorizontal();
		{
			EditorGUILayout.PrefixLabel("Depth");

			int depth = mWidget.depth;
			if (GUILayout.Button("Back")) --depth;
			depth = EditorGUILayout.IntField(depth, GUILayout.Width(40f));
			if (GUILayout.Button("Forward")) ++depth;

			if (mWidget.depth != depth)
			{
				Undo.RegisterUndo(mWidget, "Depth Change");
				mWidget.depth = depth;
				EditorUtility.SetDirty(mWidget.gameObject);
			}

			// NOTE: Experimental code that swaps depth layers and updates the colliders on depth change.
			/*if (GUILayout.Button("Back"))
			{
				Undo.RegisterSceneUndo("Depth Change");
				mWidget.panel.SwapDepth(mWidget.depth, mWidget.depth - 1);
			}

			int depth = EditorGUILayout.IntField(mWidget.depth, GUILayout.Width(40f));

			if (mWidget.depth != depth)
			{
				Undo.RegisterSceneUndo("Depth Change");
				mWidget.depth = depth;
				EditorUtility.SetDirty(mWidget.gameObject);
			}

			if (GUILayout.Button("Forward"))
			{
				Undo.RegisterSceneUndo("Depth Change");
				mWidget.panel.SwapDepth(mWidget.depth, mWidget.depth + 1);
			}*/
		}
		GUILayout.EndHorizontal();

		Color color = EditorGUILayout.ColorField("Color Tint", mWidget.color);

		if (mWidget.color != color)
		{
			Undo.RegisterUndo(mWidget, "Color Change");
			mWidget.color = color;
			EditorUtility.SetDirty(mWidget.gameObject);
		}

		GUILayout.BeginHorizontal();
		{
			EditorGUILayout.PrefixLabel("Correction");

			if (GUILayout.Button("Make Pixel-Perfect"))
			{
				Undo.RegisterUndo(mWidget.transform, "Make Pixel-Perfect");
				mWidget.MakePixelPerfect();
				EditorUtility.SetDirty(mWidget.transform);
			}
		}
		GUILayout.EndHorizontal();

		UIWidget.Pivot pivot = (UIWidget.Pivot)EditorGUILayout.EnumPopup("Pivot", mWidget.pivot);

		if (mWidget.pivot != pivot)
		{
			Undo.RegisterUndo(mWidget, "Pivot Change");
			mWidget.pivot = pivot;
			EditorUtility.SetDirty(mWidget.gameObject);
		}

		if (mWidget.mainTexture != null)
		{
			GUILayout.BeginHorizontal();
			{
				mShowTexture = EditorGUILayout.Toggle("Preview", mShowTexture, GUILayout.Width(100f));

				if (mShowTexture)
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
				}
			}
			GUILayout.EndHorizontal();

			// Draw the texture last
			if (mShowTexture) OnDrawTexture();
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