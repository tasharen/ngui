using UnityEngine;

/// <summary>
/// One of the scripts tying the UI with the inventory system
/// </summary>

[AddComponentMenu("NGUI/Examples/UI Item Slot")]
public class UIItemSlot : MonoBehaviour
{
	public InvBaseItem.Slot slot;
	public InvEquipment equipment;
	public UISprite icon;
	public UIWidget background;
	public UILabel label;

	InvGameItem mItem;

	void OnTooltip (bool show)
	{
		UITooltip.ShowItem(show ? mItem : null);
	}

	void Update ()
	{
		InvGameItem item = (equipment != null) ? equipment.GetItem(slot) : null;

		if (mItem != item)
		{
			mItem = item;

			InvBaseItem baseItem = (item != null) ? item.baseItem : null;

			if (label != null)
			{
				string itemName = (item != null) ? item.name : null;
				label.text = (itemName != null) ? itemName : slot.ToString();
			}
			
			if (icon != null)
			{
				if (baseItem == null || baseItem.iconAtlas == null)
				{
					icon.enabled = false;
				}
				else
				{
					icon.atlas = baseItem.iconAtlas;
					icon.spriteName = baseItem.iconName;
					icon.enabled = true;
					icon.MakePixelPerfect();
				}
			}

			if (background != null)
			{
				background.color = (item != null) ? item.color : Color.grey;
			}
		}
	}
}