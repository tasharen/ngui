using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Inventory system -- Equipment class works with InvAttachmentPoints and allows to visually equip and remove items.
/// </summary>

[AddComponentMenu("NGUI/Examples/Equipment")]
public class InvEquipment : MonoBehaviour
{
	/// <summary>
	/// List of items that have been equipped.
	/// </summary>

	InvGameItem[] mItems;
	InvAttachmentPoint[] mAttachments;

	/// <summary>
	/// Equip the specified item automatically replacing an existing one.
	/// </summary>

	void Equip (InvBaseItem.Slot slot, InvGameItem item)
	{
		InvBaseItem baseItem = (item != null) ? item.baseItem : null;

		if (slot != InvBaseItem.Slot.None)
		{
			if (mItems == null)
			{
				// Automatically figure out how many item slots we need
				int count = Enum.GetNames(typeof(InvBaseItem.Slot)).Length - 1;
				mItems = new InvGameItem[count];
			}

			// Equip this item
			mItems[(int)slot - 1] = item;

			// Get the list of all attachment points
			if (mAttachments == null) mAttachments = GetComponentsInChildren<InvAttachmentPoint>();

			// Equip the item visually
			foreach (InvAttachmentPoint ip in mAttachments)
			{
				if (ip.slot == slot)
				{
					GameObject go = ip.Attach(baseItem != null ? baseItem.attachment : null);

					if (go != null)
					{
						Renderer ren = go.renderer;
						if (ren != null) ren.material.color = baseItem.color;
					}
				}
			}
		}
		else if (item != null)
		{
			Debug.LogWarning("Can't equip \"" + item.name + "\" because it doesn't specify an item slot");
		}
	}

	/// <summary>
	/// Equip the specified item.
	/// </summary>

	public void Equip (InvGameItem item)
	{
		if (item != null)
		{
			InvBaseItem baseItem = item.baseItem;
			if (baseItem != null) Equip(baseItem.slot, item);
			else Debug.LogWarning("Can't resolve the item ID of " + item.baseItemID);
		}
	}

	/// <summary>
	/// Unequip the specified item.
	/// </summary>

	public void Unequip (InvGameItem item)
	{
		if (item != null)
		{
			InvBaseItem baseItem = item.baseItem;
			if (baseItem != null) Equip(baseItem.slot, null);
		}
	}

	/// <summary>
	/// Unequip the item from the specified slot.
	/// </summary>

	public void Unequip (InvBaseItem.Slot slot) { Equip(slot, null); }

	/// <summary>
	/// Whether the specified item is currently equipped.
	/// </summary>

	public bool HasEquipped (InvGameItem item)
	{
		if (mItems != null)
		{
			foreach (InvGameItem i in mItems)
			{
				if (i == item) return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Whether the specified slot currently has an item equipped.
	/// </summary>

	public bool HasEquipped (InvBaseItem.Slot slot)
	{
		if (mItems != null)
		{
			foreach (InvGameItem i in mItems)
			{
				InvBaseItem baseItem = i.baseItem;
				if (baseItem != null && baseItem.slot == slot) return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Retrieves the item in the specified slot.
	/// </summary>

	public InvGameItem GetItem (InvBaseItem.Slot slot)
	{
		if (slot != InvBaseItem.Slot.None)
		{
			int index = (int)slot - 1;

			if (mItems != null && index < mItems.Length)
			{
				return mItems[index];
			}
		}
		return null;
	}

	/// <summary>
	/// Get the summed up stats of all of the equipped items.
	/// </summary>

	/*public List<InvStat> GetStats ()
	{
		List<InvStat> list = new List<InvStat>();

		if (items != null)
		{
			foreach (InvBaseItem item in items)
			{
				if (item == null) continue;
				if (item.stats == null) continue;

				foreach (InvStat stat in item.stats)
				{
					bool found = false;

					foreach (InvStat existingStat in list)
					{
						if (existingStat.id == stat.id)
						{
							if (stat.id != InvStat.Identifier.Other && existingStat.modifier == stat.modifier)
							{
								// If the stat is already present, simply increment it
								InvStat copy = existingStat;
								copy.amount += stat.amount;
								found = true;
								break;
							}
						}
					}

					// InvStat is not present -- add a new one
					if (!found)
					{
						InvStat newStat = new InvStat();
						newStat.id = stat.id;
						newStat.modifier = stat.modifier;
						newStat.amount = stat.amount;
						list.Add(newStat);
					}
				}
			}
			list.Sort(InvStat.Compare);
		}
		return list;
	}*/
}