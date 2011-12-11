using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit the UIAtlas.
/// </summary>

[CustomEditor(typeof(UIAtlas))]
public class UIAtlasInspector : Editor
{
	enum View
	{
		Atlas,
		Sprite,
	}

	static View mView = View.Sprite;
	static bool mUseShader = false;

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
			mat = EditorGUILayout.ObjectField("Material", mat, typeof(Material), false) as Material;

			TextAsset ta = EditorGUILayout.ObjectField("TP Import", null, typeof(TextAsset), false) as TextAsset;
			
			if (ta != null)
			{
				Undo.RegisterUndo(mAtlas, "Import Sprites");
				MiniJSON.LoadSpriteData(mAtlas, ta);
			}

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

		if (mat != null)
		{
			Color blue = new Color(0f, 0.7f, 1f, 1f);
			Color green = new Color(0.4f, 1f, 0f, 1f);

			mIndex = Mathf.Min(mIndex, mAtlas.sprites.Count - 1);
			mIndex = Mathf.Max(mIndex, 0);

			UIAtlas.Sprite sprite = (mIndex < mAtlas.sprites.Count) ? mAtlas.sprites[mIndex] : null;

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
				GUI.backgroundColor = Color.green;
				
				if (GUILayout.Button("New Sprite"))
				{
					RegisterUndo();
					UIAtlas.Sprite newSprite = new UIAtlas.Sprite();

					if (sprite != null)
					{
						newSprite.name = "Copy of " + sprite.name;
						newSprite.outer = sprite.outer;
						newSprite.inner = sprite.inner;
					}
					else
					{
						newSprite.name = "New Sprite";
					}

					mAtlas.sprites.Add(newSprite);
					mIndex = mAtlas.sprites.Count - 1;
					sprite = newSprite;
				}
				GUI.backgroundColor = Color.white;

				if (sprite != null)
				{
					GUITools.DrawSeparator();

					// Navigation section
					GUILayout.BeginHorizontal();
					{
						if (mIndex == 0) GUI.color = Color.grey;
						if (GUILayout.Button("<<")) { mConfirmDelete = false; --mIndex; }
						GUI.color = Color.white;
						mIndex = EditorGUILayout.IntField(mIndex + 1, GUILayout.Width(40f)) - 1;
						GUILayout.Label("/ " + mAtlas.sprites.Count, GUILayout.Width(40f));
						if (mIndex + 1 == mAtlas.sprites.Count) GUI.color = Color.grey;
						if (GUILayout.Button(">>")) { mConfirmDelete = false; ++mIndex; }
						GUI.color = Color.white;
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

						if (outer.xMax < outer.xMin) outer.xMax = outer.xMin;
						if (outer.yMax < outer.yMin) outer.yMax = outer.yMin;

						if (outer != sprite.outer)
						{
							float x = outer.xMin - sprite.outer.xMin;
							float y = outer.yMin - sprite.outer.yMin;

							inner.x += x;
							inner.y += y;
						}

						// Sanity checks to ensure that the inner rect is always inside the outer
						inner.xMin = Mathf.Clamp(inner.xMin, outer.xMin, outer.xMax);
						inner.xMax = Mathf.Clamp(inner.xMax, outer.xMin, outer.xMax);
						inner.yMin = Mathf.Clamp(inner.yMin, outer.yMin, outer.yMax);
						inner.yMax = Mathf.Clamp(inner.yMax, outer.yMin, outer.yMax);

						EditorGUILayout.Separator();

						// Create a button that can make the coordinates pixel-perfect on click
						GUILayout.BeginHorizontal();
						{
							GUILayout.Label("Correction", GUILayout.Width(75f));

							Rect corrected0 = outer;
							Rect corrected1 = inner;

							if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
							{
								corrected0 = NGUITools.MakePixelPerfect(corrected0);
								corrected1 = NGUITools.MakePixelPerfect(corrected1);
							}
							else
							{
								corrected0 = NGUITools.MakePixelPerfect(corrected0, tex.width, tex.height);
								corrected1 = NGUITools.MakePixelPerfect(corrected1, tex.width, tex.height);
							}

							if (corrected0 == sprite.outer && corrected1 == sprite.inner)
							{
								GUI.color = Color.grey;
								GUILayout.Button("Make Pixel-Perfect");
								GUI.color = Color.white;
							}
							else if (GUILayout.Button("Make Pixel-Perfect"))
							{
								outer = corrected0;
								inner = corrected1;
								GUI.changed = true;
							}
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						{
							mView = (View)EditorGUILayout.EnumPopup("Show", mView);
							GUILayout.Label("Shader", GUILayout.Width(45f));

							if (mUseShader != EditorGUILayout.Toggle(mUseShader, GUILayout.Width(20f)))
							{
								mUseShader = !mUseShader;

								if (mUseShader && mView == View.Sprite)
								{
									// TODO: Remove this when Unity fixes the bug with DrawPreviewTexture not being affected by BeginGroup
									Debug.LogWarning("There is a bug in Unity that prevents the texture from getting clipped properly.\n" +
										"Until it's fixed by Unity, your texture may spill onto the rest of the Unity's GUI while using this mode.");
								}
							}
						}
						GUILayout.EndHorizontal();

						Rect uv0 = outer;
						Rect uv1 = inner;

						if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
						{
							uv0 = NGUITools.ConvertToTexCoords(uv0, tex.width, tex.height);
							uv1 = NGUITools.ConvertToTexCoords(uv1, tex.width, tex.height);
						}

						// Draw the atlas
						EditorGUILayout.Separator();
						Material m = mUseShader ? mAtlas.material : null;
						Rect rect = (mView == View.Atlas) ? GUITools.DrawAtlas(tex, m) : GUITools.DrawSprite(tex, uv0, m);

						// Draw the sprite outline
						GUITools.DrawOutline(rect, uv1, blue);
						GUITools.DrawOutline(rect, uv0, green);

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