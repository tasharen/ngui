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

		// This flag gets set to 'true' if RegisterUndo() gets called
		mRegisteredUndo = false;

		// View style comes first -- simple or advanced
		mView = (ViewOptions)EditorGUILayout.EnumPopup("View Style", mView);
		EditorGUILayout.Separator();

		// Atlas field comes next
		UIAtlas atlas = EditorGUILayout.ObjectField("Atlas", mWidget.atlas, typeof(UIAtlas), true) as UIAtlas;
		UIAtlas.Sprite sprite = null;
		
		// If we have an atlas we should draw a list of sprites contained by the atlas
		if (atlas != null)
		{
			string[] sprites = atlas.GetListOfSprites();

			if (sprites != null && sprites.Length > 0)
			{
				int index = 0;
				string spriteName = (mWidget.spriteName != null) ? mWidget.spriteName : sprites[0];

				// We need to find the sprite in order to have it selected
				if (!string.IsNullOrEmpty(spriteName))
				{
					for (int i = 0; i < sprites.Length; ++i)
					{
						if (string.Equals(sprites[i], spriteName, System.StringComparison.OrdinalIgnoreCase))
						{
							index = i;
							break;
						}
					}
				}

				// Draw the sprite selection popup
				index = EditorGUILayout.Popup("Sprite", index, sprites);
				sprite = atlas.GetSprite(sprites[index]);
			}
		}

		// All widgets have a color tint
		Color color = EditorGUILayout.ColorField("Color Tint", mWidget.color);

		bool autoDepth = mWidget.autoDepth;
		int depth = mWidget.depth;
		int group = mWidget.group;

		// Advanced options exposes depth and draw group properties
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

		// Update the widget's properties if something has changed
		if (mRegisteredUndo)
		{
			mWidget.atlas = atlas;
			mWidget.spriteName = (sprite != null) ? sprite.name : "";
			mWidget.color = color;
			mWidget.autoDepth = autoDepth;

			if (!autoDepth) mWidget.depth = depth;

			// Changing the group should affect all children
			if (mWidget.group != group)
			{
				int prev = mWidget.group;
				UIWidget[] widgets = mWidget.gameObject.GetComponentsInChildren<UIWidget>();
				foreach (UIWidget w in widgets) if (w.group == prev) w.group = group;
			}
			mWidget.ScreenUpdate();
		}
	}

	/// <summary>
	/// Any and all derived functionality.
	/// </summary>

	protected virtual void OnCustomGUI() { }
}