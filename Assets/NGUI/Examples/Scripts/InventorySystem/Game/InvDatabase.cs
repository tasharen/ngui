using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Examples/Item Database")]
public class InvDatabase : MonoBehaviour
{
	// Cached list of all available item databases
	static InvDatabase[] mList;
	static bool mIsDirty = true;

	/// <summary>
	/// Retrieves the list of item databases, finding all instances if necessary.
	/// </summary>

	static public InvDatabase[] list
	{
		get
		{
			if (mIsDirty)
			{
				mIsDirty = false;
				mList = GameObject.FindSceneObjectsOfType(typeof(InvDatabase)) as InvDatabase[];

				// Alternative method, considers prefabs:
				//mList = Resources.FindObjectsOfTypeAll(typeof(InvDatabase)) as InvDatabase[];
			}
			return mList;
		}
	}

	/// <summary>
	/// Each database should have a unique 16-bit ID. When the items are saved, database IDs
	/// get combined with item IDs to create 32-bit IDs containing both values.
	/// </summary>

	public int databaseID = 0;

	/// <summary>
	/// List of items in this database.
	/// </summary>

	public List<InvItem> items = new List<InvItem>();

	/// <summary>
	/// Add this database to the list.
	/// </summary>

	void OnEnable () { mIsDirty = true; }

	/// <summary>
	/// Remove this database from the list.
	/// </summary>

	void OnDisable () { mIsDirty = true; }

	/// <summary>
	/// Find an item by its 16-bit ID.
	/// </summary>

	InvItem GetItem (int id16)
	{
		foreach (InvItem item in items)
		{
			if (item.id16 == id16) return item;
		}
		return null;
	}

	/// <summary>
	/// Find a database given its ID.
	/// </summary>

	static InvDatabase GetDatabase (int dbID)
	{
		foreach (InvDatabase db in list)
		{
			if (db.databaseID == dbID)
			{
				return db;
			}
		}
		return null;
	}

	/// <summary>
	/// Find the specified item given its full 32-bit ID (not to be confused with individual 16-bit item IDs).
	/// </summary>

	static public InvItem FindByID (int id32)
	{
		InvDatabase db = GetDatabase(id32 >> 16);
		return (db != null) ? db.GetItem(id32 & 0xFFFF) : null;
	}

	/// <summary>
	/// Find the item with the specified name.
	/// </summary>

	static public InvItem FindByName (string exact)
	{
		foreach (InvDatabase db in list)
		{
			foreach (InvItem item in db.items)
			{
				if (item.name.Equals(exact, System.StringComparison.OrdinalIgnoreCase))
				{
					return item;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Get the full 32-bit ID of the specified item.
	/// Use this to get a list of items on the character that can get saved out to an external database or file.
	/// </summary>

	static public int FindItemID (InvItem item)
	{
		foreach (InvDatabase db in list)
		{
			if (db.items.Contains(item))
			{
				return (db.databaseID << 16) | item.id16;
			}
		}
		return -1;
	}
}