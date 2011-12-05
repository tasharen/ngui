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

	public static void SelectIndex (InvDatabase db, InvItem item)
	{
		mIndex = 0;

		foreach (InvItem i in db.items)
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

		InvItem item = null;

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
				item = new InvItem();
				item.name = "New Item";
				item.description = "Item Description";
				item.id16 = (db.items.Count > 0) ? db.items[db.items.Count - 1].id16 + 1 : 0;
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

				// Item properties
				string itemDesc = GUILayout.TextArea(item.description, 200, GUILayout.Height(100f));
				InvItem.Slot slot = (InvItem.Slot)EditorGUILayout.EnumPopup("Slot", item.slot);
				GameObject go = (GameObject)EditorGUILayout.ObjectField("Attachment", item.attachment, typeof(GameObject), false);
				Color color = EditorGUILayout.ColorField("Color", item.color);

				if (!string.Equals(itemDesc, item.description) || slot != item.slot || go != item.attachment || color != item.color)
				{
					Undo.RegisterUndo(db, "Item Properties");
					item.description = itemDesc;
					item.slot = slot;
					item.attachment = go;
					item.color = color;
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
					item.stats.Add(new InvStat());
				}
			}
		}
	}
}