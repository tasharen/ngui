using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit Inventory Databases.
/// </summary>

[CustomEditor(typeof(InvDatabase))]
public class InvDatabaseInspector : Editor
{
	static int mIndex = 0;

	bool mConfirmDelete = false;

	/// <summary>
	/// Helper function that sets the index to the index of the specified item.
	/// </summary>

	public static void SelectIndex (InvDatabase db, InvBaseItem item)
	{
		mIndex = 0;

		foreach (InvBaseItem i in db.items)
		{
			if (i == item) break;
			++mIndex;
		}
	}

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		EditorGUIUtility.LookLikeControls(80f);
		InvDatabase db = target as InvDatabase;
		GUITools.DrawSeparator();

		InvBaseItem item = null;

		if (db.items == null || db.items.Count == 0)
		{
			mIndex = 0;
		}
		else
		{
			mIndex = Mathf.Clamp(mIndex, 0, db.items.Count - 1);
			item = db.items[mIndex];
		}

		if (mConfirmDelete)
		{
			// Show the confirmation dialog
			GUILayout.Label("Are you sure you want to delete '" + item.name + "'?");
			GUITools.DrawSeparator();

			GUILayout.BeginHorizontal();
			{
				GUI.backgroundColor = Color.green;
				if (GUILayout.Button("Cancel")) mConfirmDelete = false;
				GUI.backgroundColor = Color.red;

				if (GUILayout.Button("Delete"))
				{
					Undo.RegisterUndo(db, "Delete Inventory Item");
					db.items.RemoveAt(mIndex);
					mConfirmDelete = false;
				}
				GUI.backgroundColor = Color.white;
			}
			GUILayout.EndHorizontal();
		}
		else
		{
			// Database icon atlas
			UIAtlas atlas = EditorGUILayout.ObjectField("Icon Atlas", db.iconAtlas, typeof(UIAtlas), false) as UIAtlas;

			if (atlas != db.iconAtlas)
			{
				Undo.RegisterUndo(db, "Databse Atlas change");
				db.iconAtlas = atlas;
				foreach (InvBaseItem i in db.items) i.iconAtlas = atlas;
			}

			// Database ID
			int dbID = EditorGUILayout.IntField("Database ID", db.databaseID);

			if (dbID != db.databaseID)
			{
				Undo.RegisterUndo(db, "Database ID change");
				db.databaseID = dbID;
			}

			// "New" button
			GUI.backgroundColor = Color.green;

			if (GUILayout.Button("New Item"))
			{
				Undo.RegisterUndo(db, "Add Inventory Item");
				item = new InvBaseItem();
				item.name = "New Item";
				item.description = "Item Description";
				item.id16 = (db.items.Count > 0) ? db.items[db.items.Count - 1].id16 + 1 : 0;
				item.iconAtlas = db.iconAtlas;
				db.items.Add(item);
				mIndex = db.items.Count - 1;
			}
			GUI.backgroundColor = Color.white;

			if (item != null)
			{
				GUITools.DrawSeparator();

				// Navigation section
				GUILayout.BeginHorizontal();
				{
					if (mIndex == 0) GUI.color = Color.grey;
					if (GUILayout.Button("<<")) { mConfirmDelete = false; --mIndex; }
					GUI.color = Color.white;
					mIndex = EditorGUILayout.IntField(mIndex + 1, GUILayout.Width(40f)) - 1;
					GUILayout.Label("/ " + db.items.Count, GUILayout.Width(40f));
					if (mIndex + 1 == db.items.Count) GUI.color = Color.grey;
					if (GUILayout.Button(">>")) { mConfirmDelete = false; ++mIndex; }
					GUI.color = Color.white;
				}
				GUILayout.EndHorizontal();

				GUITools.DrawSeparator();

				// Item name and delete item button
				GUILayout.BeginHorizontal();
				{
					string itemName = EditorGUILayout.TextField("Item Name", item.name);

					GUI.backgroundColor = Color.red;

					if (GUILayout.Button("Delete", GUILayout.Width(55f)))
					{
						mConfirmDelete = true;
					}
					GUI.backgroundColor = Color.white;

					if (!string.Equals(itemName, item.name))
					{
						Undo.RegisterUndo(db, "Rename Item");
						item.name = itemName;
					}
				}
				GUILayout.EndHorizontal();

				string itemDesc = GUILayout.TextArea(item.description, 200, GUILayout.Height(100f));
				InvBaseItem.Slot slot = (InvBaseItem.Slot)EditorGUILayout.EnumPopup("Slot", item.slot);
				string iconName = "";
				float iconSize = 64f;
				bool drawIcon = false;
				float extraSpace = 0f;

				if (item.iconAtlas != null)
				{
					List<string> sprites = item.iconAtlas.GetListOfSprites();

					sprites.Insert(0, "<None>");

					int index = 0;
					string spriteName = (item.iconName != null) ? item.iconName : sprites[0];

					// We need to find the sprite in order to have it selected
					if (!string.IsNullOrEmpty(spriteName))
					{
						for (int i = 1; i < sprites.Count; ++i)
						{
							if (string.Equals(sprites[i], spriteName, System.StringComparison.OrdinalIgnoreCase))
							{
								index = i;
								break;
							}
						}
					}

					// Draw the sprite selection popup
					index = EditorGUILayout.Popup("Icon", index, sprites.ToArray());
					UIAtlas.Sprite sprite = (index > 0) ? item.iconAtlas.GetSprite(sprites[index]) : null;

					if (sprite != null)
					{
						iconName = sprite.name;

						Material mat = item.iconAtlas.material;

						if (mat != null)
						{
							Texture2D tex = mat.mainTexture as Texture2D;

							if (tex != null)
							{
								drawIcon = true;
								Rect rect = sprite.outer;

								if (item.iconAtlas.coordinates == UIAtlas.Coordinates.Pixels)
								{
									rect = NGUITools.ConvertToTexCoords(rect, tex.width, tex.height);
								}

								GUILayout.Space(4f);
								GUILayout.BeginHorizontal();
								{
									GUILayout.Space(Screen.width - iconSize);
									GUITools.DrawSprite(tex, rect, null);
								}
								GUILayout.EndHorizontal();

								extraSpace = iconSize * (float)sprite.outer.height / sprite.outer.width;
							}
						}
					}
				}

				// Item level range
				GUILayout.BeginHorizontal();
				GUILayout.Label("Level Range", GUILayout.Width(77f));
				EditorGUILayout.IntField(1, GUILayout.MinWidth(40f));
				EditorGUILayout.IntField(50, GUILayout.MinWidth(40f));
				if (drawIcon) GUILayout.Space(iconSize);
				GUILayout.EndHorizontal();

				// Game Object attachment field, left of the icon
				GUILayout.BeginHorizontal();
				GameObject go = (GameObject)EditorGUILayout.ObjectField("Attachment", item.attachment, typeof(GameObject), false);
				if (drawIcon) GUILayout.Space(iconSize);
				GUILayout.EndHorizontal();

				// Color tint field, left of the icon
				GUILayout.BeginHorizontal();
				Color color = EditorGUILayout.ColorField("Color", item.color);
				if (drawIcon) GUILayout.Space(iconSize);
				GUILayout.EndHorizontal();

				// Calculate the extra spacing necessary for the icon to show up properly and not overlap anything
				if (drawIcon)
				{
					extraSpace = Mathf.Max(0f, extraSpace - 60f);
					GUILayout.Space(extraSpace);
				}

				// Item stats
				GUITools.DrawSeparator();

				if (item.stats != null)
				{
					for (int i = 0; i < item.stats.Count; ++i)
					{
						InvStat stat = item.stats[i];

						GUILayout.BeginHorizontal();
						{
							InvStat.Identifier iden = (InvStat.Identifier)EditorGUILayout.EnumPopup(stat.id, GUILayout.Width(80f));

							// Color the field red if it's negative, green if it's positive
							if (stat.amount > 0) GUI.backgroundColor = Color.green;
							else if (stat.amount < 0) GUI.backgroundColor = Color.red;
							int amount = EditorGUILayout.IntField(stat.amount, GUILayout.Width(40f));
							GUI.backgroundColor = Color.white;

							InvStat.Modifier mod = (InvStat.Modifier)EditorGUILayout.EnumPopup(stat.modifier);

							GUI.backgroundColor = Color.red;
							if (GUILayout.Button("X", GUILayout.Width(20f)))
							{
								Undo.RegisterUndo(db, "Delete Item Stat");
								item.stats.RemoveAt(i);
								--i;
							}
							else if (iden != stat.id || amount != stat.amount || mod != stat.modifier)
							{
								Undo.RegisterUndo(db, "Item Stats");
								stat.id = iden;
								stat.amount = amount;
								stat.modifier = mod;
							}
							GUI.backgroundColor = Color.white;
						}
						GUILayout.EndHorizontal();
					}
				}

				if (GUILayout.Button("Add Stat", GUILayout.Width(80f)))
				{
					Undo.RegisterUndo(db, "Add Item Stat");
					InvStat stat = new InvStat();
					stat.id = InvStat.Identifier.Armor;
					item.stats.Add(stat);
				}

				// Save all values
				if (!string.Equals(itemDesc, item.description) || slot != item.slot || go != item.attachment || color != item.color ||
					!string.Equals(iconName, item.iconName))
				{
					Undo.RegisterUndo(db, "Item Properties");
					item.description = itemDesc;
					item.slot = slot;
					item.attachment = go;
					item.color = color;
					item.iconName = iconName;
				}
			}
		}
	}
}