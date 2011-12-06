using UnityEngine;

/// <summary>
/// One of the scripts tying the UI with the inventory system
/// </summary>

[AddComponentMenu("NGUI/Examples/UI Item Slot")]
public class UIItemSlot : MonoBehaviour
{
	public InvItem.Slot slot;
	public InvEquipment equipment;
	public UISprite icon;
	public UIWidget background;
	public UILabel label;

	InvItem mItem;

	void Update ()
	{
		InvItem item = (equipment != null) ? equipment.GetItem(slot) : null;

		if (mItem != item)
		{
			mItem = item;

			if (label != null) label.text = (mItem != null) ? mItem.name : slot.ToString();
		}
	}
}