using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UIWidgets.
/// </summary>

[CustomEditor(typeof(UIWidget))]
public class UIWidgetInspector : Editor
{
	public enum ViewOptions
	{
		Simple,
		Advanced,
	}

	protected UIWidget mWidget;
	protected bool mRegisteredUndo = false;
	protected static ViewOptions mView = ViewOptions.Simple;

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
		EditorGUIUtility.LookLikeControls(120f);
		mWidget = target as UIWidget;

		mRegisteredUndo = false;

		mView = (ViewOptions)EditorGUILayout.EnumPopup("View Style", mView);
		EditorGUILayout.Separator();

		Material mat = EditorGUILayout.ObjectField("Material", mWidget.material, typeof(Material), true) as Material;
		Color color = EditorGUILayout.ColorField("Color Tint", mWidget.color);

		int depth = mWidget.depth;
		bool autoDepth = mWidget.autoDepth;
		int group = mWidget.group;

		if (mView == ViewOptions.Advanced)
		{
			EditorGUILayout.Separator();
			GUILayout.BeginHorizontal();
			autoDepth = EditorGUILayout.Toggle("Automatic Depth", autoDepth);
			GUILayout.EndHorizontal();

			if (!autoDepth)
			{
				GUILayout.BeginHorizontal();
				{
					EditorGUILayout.PrefixLabel("Depth");
					if (GUILayout.Button("<")) { RegisterUndo(); --depth; }
					depth = EditorGUILayout.IntField(depth);
					if (GUILayout.Button(">")) { RegisterUndo(); ++depth; }
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Draw Group");
				if (GUILayout.Button("<")) { RegisterUndo(); --group; }
				group = EditorGUILayout.IntField(group);
				if (GUILayout.Button(">")) { RegisterUndo(); ++group; }
			}
			GUILayout.EndHorizontal();
		}

		// Draw all derived functionality
		if (GUI.changed) RegisterUndo();
		EditorGUILayout.Separator();
		OnCustomGUI();

		if (mRegisteredUndo)
		{
			mWidget.material = mat;
			mWidget.color = color;
			mWidget.autoDepth = autoDepth;

			if (!autoDepth) mWidget.depth = depth;

			// Changing the group should affect all children
			if (mWidget.group != group)
			{
				int prev = mWidget.group;
				UIWidget[] widgets = mWidget.gameObject.GetComponentsInChildren<UIWidget>();
				
				foreach (UIWidget w in widgets)
				{
					if (w.group == prev)
					{
						w.group = group;
					}
				}
			}
			mWidget.ScreenUpdate();
		}
	}

	/// <summary>
	/// Any and all derived functionality.
	/// </summary>

	protected virtual void OnCustomGUI() { }
}