using UnityEngine;
using System.Collections.Generic;
using System;

[AddComponentMenu("NGUI/Examples/Equipment")]
public class InvEquipment : MonoBehaviour
{
	/// <summary>
	/// List of items that have been equipped.
	/// </summary>

	InvItem[] mItems;
	InvAttachmentPoint[] mAttachments;

	/// <summary>
	/// Equip the specified item automatically replacing an existing one.
	/// </summary>

	void Equip (InvItem.Slot slot, InvItem item)
	{
		if (slot != InvItem.Slot.None)
		{
			if (mItems == null)
			{
				// Automatically figure out how many item slots we need
				int count = Enum.GetNames(typeof(InvItem.Slot)).Length - 1;
				mItems = new InvItem[count];
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
					GameObject go = ip.Attach(item == null ? null : item.attachment);
					
					if (go != null)
					{
						Renderer ren = go.renderer;
						if (ren != null) ren.material.color = item.color;
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

	public void Equip (InvItem item) { if (item != null) Equip(item.slot, item); }

	/// <summary>
	/// Unequip the specified item.
	/// </summary>

	public void Unequip (InvItem item) { if (item != null) Equip(item.slot, null); }

	/// <summary>
	/// Unequip the item from the specified slot.
	/// </summary>

	public void Unequip (InvItem.Slot slot) { Equip(slot, null); }

	/// <summary>
	/// Whether the specified item is currently equipped.
	/// </summary>

	public bool HasEquipped (InvItem item)
	{
		if (mItems != null)
		{
			foreach (InvItem i in mItems)
			{
				if (i == item) return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Whether the specified slot currently has an item equipped.
	/// </summary>

	public bool HasEquipped (InvItem.Slot slot)
	{
		if (mItems != null)
		{
			foreach (InvItem i in mItems)
			{
				if (i.slot == slot) return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Get the summed up stats of all of the equipped items.
	/// </summary>

	public List<InvStat> GetStats ()
	{
		return InvTools.CombineStats(mItems);
	}

	/// <summary>
	/// Retrieves the item in the specified slot.
	/// </summary>

	public InvItem GetItem (InvItem.Slot slot)
	{
		if (slot != InvItem.Slot.None)
		{
			int index = (int)slot - 1;

			if (mItems != null && index < mItems.Length)
			{
				return mItems[index];
			}
		}
		return null;
	}
}