using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit the UIAtlas.
/// </summary>

[CustomEditor(typeof(UIAtlas))]
public class UIAtlasInspector : Editor
{
	UIAtlas mAtlas;
	int mIndex = 0;
	bool mRegisteredUndo = false;
	bool mConfirmDelete = false;

	/// <summary>
	/// Register an Undo command with the Unity editor.
	/// </summary>

	protected void RegisterUndo ()
	{
		if (!mRegisteredUndo)
		{
			mRegisteredUndo = true;
			Undo.RegisterUndo(mAtlas, "Atlas Change");
		}
	}

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		mRegisteredUndo = false;
		EditorGUIUtility.LookLikeControls(80f);
		mAtlas = target as UIAtlas;

		Material mat = mAtlas.material;
		UIAtlas.Coordinates coords = mAtlas.coordinates;
		
		if (!mConfirmDelete)
		{
			GUITools.DrawSeparator();
			mat = EditorGUILayout.ObjectField("Material", mat, typeof(Material), true) as Material;
			coords = (UIAtlas.Coordinates)EditorGUILayout.EnumPopup("Coordinates", coords);
		}

		if (GUI.changed)
		{
			// Atlas material has changed
			RegisterUndo();
			mAtlas.material = mat;
			mAtlas.coordinates = coords;
			mConfirmDelete = false;
		}
		else if (mat != null)
		{
			Color blue = new Color(0f, 0.7f, 1f, 1f);
			Color green = new Color(0.4f, 1f, 0f, 1f);

			mIndex = Mathf.Clamp(mIndex, 0, mAtlas.sprites.Count - 1);
			UIAtlas.Sprite sprite = (mIndex < mAtlas.sprites.Count) ? mAtlas.sprites[mIndex] : null;

			// New sprite button
			if (!mConfirmDelete)
			{
				GUI.backgroundColor = Color.green;

				if (GUILayout.Button("New Sprite"))
				{
					RegisterUndo();
					sprite = new UIAtlas.Sprite();
					sprite.name = "New Sprite";
					mAtlas.sprites.Add(sprite);
					mIndex = mAtlas.sprites.Count - 1;
				}
				GUI.backgroundColor = Color.white;
			}

			// Only proceed if we have a valid sprite to work with
			if (sprite != null)
			{
				if (mConfirmDelete)
				{
					// Show the confirmation dialog
					GUITools.DrawSeparator();
					GUILayout.Label("Are you sure you want to delete '" + sprite.name + "'?");
					GUITools.DrawSeparator();

					GUILayout.BeginHorizontal();
					{
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button("Cancel")) mConfirmDelete = false;
						GUI.backgroundColor = Color.red;

						if (GUILayout.Button("Delete"))
						{
							RegisterUndo();
							mAtlas.sprites.RemoveAt(mIndex);
							mConfirmDelete = false;
						}
						GUI.backgroundColor = Color.white;
					}
					GUILayout.EndHorizontal();
				}
				else
				{
					GUITools.DrawSeparator();

					// Navigation section
					GUILayout.BeginHorizontal();
					{
						if (mIndex == 0) GUI.backgroundColor = Color.grey;
						if (GUILayout.Button("<<")) { mConfirmDelete = false; --mIndex; }
						GUI.backgroundColor = Color.white;
						mIndex = EditorGUILayout.IntField(mIndex + 1, GUILayout.Width(40f)) - 1;
						GUILayout.Label("/ " + mAtlas.sprites.Count, GUILayout.Width(40f));
						if (mIndex + 1 == mAtlas.sprites.Count) GUI.backgroundColor = Color.grey;
						if (GUILayout.Button(">>")) { mConfirmDelete = false; ++mIndex; }
						GUI.backgroundColor = Color.white;
					}
					GUILayout.EndHorizontal();

					GUITools.DrawSeparator();

					string name = sprite.name;

					// Grab the sprite's inner and outer dimensions
					Rect inner = sprite.inner;
					Rect outer = sprite.outer;

					Texture2D tex = mat.mainTexture as Texture2D;

					if (tex != null)
					{
						Vector2 size = new Vector2(tex.width, tex.height);

						GUILayout.BeginHorizontal();
						{
							name = EditorGUILayout.TextField("Sprite Name", name);

							// Show the delete button
							GUI.backgroundColor = Color.red;
							if (GUILayout.Button("Delete", GUILayout.Width(55f)))
							{
								mConfirmDelete = true;
								return;
							}
							GUI.backgroundColor = Color.white;
						}
						GUILayout.EndHorizontal();

						// Draw the inner and outer rectangle dimensions
						GUI.backgroundColor = green;
						outer = EditorGUILayout.RectField("Outer Rect", sprite.outer);
						GUI.backgroundColor = blue;
						inner = EditorGUILayout.RectField("Inner Rect", sprite.inner);
						GUI.backgroundColor = Color.white;

						// Sanity checks to ensure that the inner rect is always inside the outer
						inner.xMin = Mathf.Clamp(inner.xMin, outer.xMin, outer.xMax);
						inner.xMax = Mathf.Clamp(inner.xMax, outer.xMin, outer.xMax);
						inner.yMin = Mathf.Clamp(inner.yMin, outer.yMin, outer.yMax);
						inner.yMax = Mathf.Clamp(inner.yMax, outer.yMin, outer.yMax);

						EditorGUILayout.Separator();

						// Draw the atlas
						Rect rect = GUITools.DrawAtlas(tex);

						// If the atlas uses pixel coordinates, calculate the UVs
						if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
						{
							GUITools.DrawOutline(rect, UIAtlas.ConvertToTexCoords(inner, size), blue);
							GUITools.DrawOutline(rect, UIAtlas.ConvertToTexCoords(outer, size), green);
						}
						else
						{
							GUITools.DrawOutline(rect, inner, blue);
							GUITools.DrawOutline(rect, outer, green);
						}
						EditorGUILayout.Separator();
					}

					if (GUI.changed)
					{
						RegisterUndo();
						sprite.name = name;
						sprite.outer = outer;
						sprite.inner = inner;
						mConfirmDelete = false;
					}
				}
			}
		}
	}
}