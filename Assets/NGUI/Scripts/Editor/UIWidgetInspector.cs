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
	protected bool mUseShader = false;

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
		GUITools.DrawSeparator();

		// Check the hierarchy to ensure that this widget is not parented to another widget
		if (mHierarchyCheck) CheckHierarchy();

		// This flag gets set to 'true' if RegisterUndo() gets called
		mRegisteredUndo = false;

		Color color = mWidget.color;
		bool center = mWidget.centered;
		int depth = mWidget.depth;

		// Check to see if we can draw the widget's default properties to begin with
		if (OnCustomStart())
		{
			// Depth navigation
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Depth");
				if (GUILayout.Button("Back")) { RegisterUndo(); --depth; }
				depth = EditorGUILayout.IntField(depth, GUILayout.Width(40f));
				if (GUILayout.Button("Forward")) { RegisterUndo(); ++depth; }
			}
			GUILayout.EndHorizontal();

			color = EditorGUILayout.ColorField("Color Tint", color);

			GUILayout.BeginHorizontal();
			{
				center = EditorGUILayout.Toggle("Centered", center, GUILayout.Width(100f));

				if (GUILayout.Button("Make Pixel-Perfect"))
				{
					Undo.RegisterUndo(mWidget.transform, "Make Pixel-Perfect");
					mWidget.MakePixelPerfect();
					EditorUtility.SetDirty(mWidget.transform);
				}
			}
			GUILayout.EndHorizontal();

			// Draw all derived functionality
			if (GUI.changed) RegisterUndo();

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

			// Custom functionality
			OnCustomEnd();
		}

		// Update the widget's properties if something has changed
		if (mRegisteredUndo)
		{
			mWidget.color = color;
			mWidget.centered = center;
			mWidget.depth = depth;
			OnCustomSave();
			mWidget.MarkAsChanged();
		}
	}

	/// <summary>
	/// Check the hierarchy to ensure that this widget is not parented to another widget.
	/// </summary>
 
	void CheckHierarchy()
	{
		mHierarchyCheck = false;
		Transform trans = mWidget.transform.parent;
		if (trans == null) return;
		Vector3 scale = trans.lossyScale;

		if (Mathf.Abs(scale.x - scale.y) > 0.001f || Mathf.Abs(scale.y - scale.x) > 0.001f)
		{
			Debug.LogWarning("Parent of " + NGUITools.GetHierarchy(mWidget.gameObject) + " does not have a uniform absolute scale.\n" +
				"Consider re-parenting to a uniformly-scaled game object instead.");

			// If the warning above gets triggered, it means that the widget's parent does not have a uniform scale.
			// This may lead to strangeness when scaling or rotating the widget. Consider this hierarchy:
			
			// Widget #1
			//  |
			//  +- Widget #2

			// You can change it to this, solving the problem:

			// GameObject
			//  |
			//  +- Widget #1
			//  |
			//  +- Widget #2
		}
	}

	/// <summary>
	/// Any and all derived functionality.
	/// </summary>

	protected virtual bool OnCustomStart () { return true; }
	protected virtual void OnCustomEnd () { }
	protected virtual void OnCustomSave () { }
}