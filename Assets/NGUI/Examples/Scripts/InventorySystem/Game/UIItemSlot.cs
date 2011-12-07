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
				if (item == null)
				{
					background.color = Color.grey;
				}
				else
				{
					Color c = Color.grey;

					switch (item.quality)
					{
						case InvGameItem.Quality.Cursed:	c = Color.red;							break;
						case InvGameItem.Quality.Broken:	c = new Color(0.1f, 0.1f, 0.1f);		break;
						case InvGameItem.Quality.Damaged:	c = new Color(0.4f, 0.4f, 0.4f);		break;
						case InvGameItem.Quality.Worn:		c = new Color(0.7f, 0.7f, 0.7f);		break;
						case InvGameItem.Quality.Sturdy:	c = new Color(1.0f, 1.0f, 1.0f);		break;
						case InvGameItem.Quality.Polished:	c = NGUITools.HexToColor(0xe0ffbeff);	break;
						case InvGameItem.Quality.Improved:	c = NGUITools.HexToColor(0x93d749ff);	break;
						case InvGameItem.Quality.Crafted:	c = NGUITools.HexToColor(0x4eff00ff);	break;
						case InvGameItem.Quality.Superior:	c = NGUITools.HexToColor(0x00baffff);	break;
						case InvGameItem.Quality.Enchanted: c = NGUITools.HexToColor(0x7376fdff);	break;
						case InvGameItem.Quality.Epic:		c = NGUITools.HexToColor(0x9600ffff);	break;
						case InvGameItem.Quality.Legendary:	c = NGUITools.HexToColor(0xff9000ff);	break;
					}
					background.color = c;
				}
			}
		}
	}
}