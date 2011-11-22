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

		// Check the hierarchy to ensure that this widget is not parented to another widget
		if (mHierarchyCheck) CheckHierarchy();

		// This flag gets set to 'true' if RegisterUndo() gets called
		mRegisteredUndo = false;

		// Atlas field comes first
		GUITools.DrawSeparator();
		UIAtlas atlas = EditorGUILayout.ObjectField("Atlas", mWidget.atlas, typeof(UIAtlas), true) as UIAtlas;

		UIAtlas.Sprite sprite = null;
		Color color = mWidget.color;
		bool center = mWidget.centered;
		int depth = mWidget.depth;

		if (atlas != null)
		{
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
					GUITools.DrawSeparator();
				}
			}

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
				}
			}
			GUILayout.EndHorizontal();

			// Draw all derived functionality
			if (GUI.changed) RegisterUndo();
			EditorGUILayout.Separator();
			OnCustomGUI();
		}

		// Update the widget's properties if something has changed
		if (mRegisteredUndo)
		{
			mWidget.atlas = atlas;
			mWidget.spriteName = (sprite != null) ? sprite.name : "";
			mWidget.color = color;
			mWidget.centered = center;
			mWidget.depth = depth;
			mWidget.Refresh();
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
			Debug.LogWarning("Parent of " + Tools.GetHierarchy(mWidget.gameObject) + " does not have a uniform absolute scale.\n" +
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

	protected virtual void OnCustomGUI() { }
}