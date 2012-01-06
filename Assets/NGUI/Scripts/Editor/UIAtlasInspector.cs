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
	bool mRegisteredUndo = false;
	bool mConfirmDelete = false;
	UIAtlas.Sprite mSprite;

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

			if (mat != null)
			{
				TextAsset ta = EditorGUILayout.ObjectField("TP Import", null, typeof(TextAsset), false) as TextAsset;

				if (ta != null)
				{
					Undo.RegisterUndo(mAtlas, "Import Sprites");
					NGUIJson.LoadSpriteData(mAtlas, ta);
					mRegisteredUndo = true;

					UISprite[] sprites = Resources.FindObjectsOfTypeAll(typeof(UISprite)) as UISprite[];

					foreach (UISprite sp in sprites)
					{
						if (sp.atlas == mAtlas)
						{
							sp.MarkAsChanged();
							EditorUtility.SetDirty(sp);
						}
					}

					UILabel[] labels = Resources.FindObjectsOfTypeAll(typeof(UILabel)) as UILabel[];

					foreach (UILabel lbl in labels)
					{
						if (lbl.font != null && lbl.font.atlas == mAtlas)
						{
							lbl.MarkAsChanged();
							EditorUtility.SetDirty(lbl);
						}
					}
				}
				coords = (UIAtlas.Coordinates)EditorGUILayout.EnumPopup("Coordinates", coords);
			}
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

			if (mSprite == null && mAtlas.sprites.Count > 0)
			{
				mSprite = mAtlas.sprites[0];
			}

			if (mConfirmDelete)
			{
				if (mSprite != null)
				{
					// Show the confirmation dialog
					GUITools.DrawSeparator();
					GUILayout.Label("Are you sure you want to delete '" + mSprite.name + "'?");
					GUITools.DrawSeparator();

					GUILayout.BeginHorizontal();
					{
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button("Cancel")) mConfirmDelete = false;
						GUI.backgroundColor = Color.red;

						if (GUILayout.Button("Delete"))
						{
							RegisterUndo();
							mAtlas.sprites.Remove(mSprite);
							mConfirmDelete = false;
						}
						GUI.backgroundColor = Color.white;
					}
					GUILayout.EndHorizontal();
				}
				else mConfirmDelete = false;
			}
			else
			{
				GUI.backgroundColor = Color.green;

				GUILayout.BeginHorizontal();
				{
					EditorGUILayout.PrefixLabel("Add/Delete");

					if (GUILayout.Button("New Sprite"))
					{
						RegisterUndo();
						UIAtlas.Sprite newSprite = new UIAtlas.Sprite();

						if (mSprite != null)
						{
							newSprite.name = "Copy of " + mSprite.name;
							newSprite.outer = mSprite.outer;
							newSprite.inner = mSprite.inner;
						}
						else
						{
							newSprite.name = "New Sprite";
						}

						mAtlas.sprites.Add(newSprite);
						mSprite = newSprite;
					}

					// Show the delete button
					GUI.backgroundColor = Color.red;

					if (GUILayout.Button("Delete", GUILayout.Width(55f)))
					{
						mConfirmDelete = true;
					}
					GUI.backgroundColor = Color.white;
				}
				GUILayout.EndHorizontal();

				if (!mConfirmDelete && mSprite != null)
				{
					GUITools.DrawSeparator();

					string spriteName = UISpriteInspector.SpriteField(mAtlas, mSprite.name);

					if (spriteName != mSprite.name)
					{
						mSprite = mAtlas.GetSprite(spriteName);
					}

					string name = mSprite.name;

					// Grab the sprite's inner and outer dimensions
					Rect inner = mSprite.inner;
					Rect outer = mSprite.outer;

					Texture2D tex = mat.mainTexture as Texture2D;

					if (tex != null)
					{
						name = EditorGUILayout.TextField("Edit Name", name);

						// Draw the inner and outer rectangle dimensions
						GUI.backgroundColor = green;
						outer = EditorGUILayout.RectField("Outer Rect", mSprite.outer);
						GUI.backgroundColor = blue;
						inner = EditorGUILayout.RectField("Inner Rect", mSprite.inner);
						GUI.backgroundColor = Color.white;

						if (outer.xMax < outer.xMin) outer.xMax = outer.xMin;
						if (outer.yMax < outer.yMin) outer.yMax = outer.yMin;

						if (outer != mSprite.outer)
						{
							float x = outer.xMin - mSprite.outer.xMin;
							float y = outer.yMin - mSprite.outer.yMin;

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

							if (corrected0 == mSprite.outer && corrected1 == mSprite.inner)
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
						mSprite.name = name;
						mSprite.outer = outer;
						mSprite.inner = inner;
						mConfirmDelete = false;
					}
				}
			}
		}

		// If something changed, mark the atlas as dirty
		if (mRegisteredUndo) EditorUtility.SetDirty(mAtlas);
	}
}