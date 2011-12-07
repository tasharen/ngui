using UnityEngine;

/// <summary>
/// Abstract UI component observing an item somewhere in the inventory. This item can be equipped on
/// the character, it can be lying in a chest, or it can be hot-linked by another player. Either way,
/// all the common behavior is in this class. What the observed item actually is...
/// that's up to the derived class to determine.
/// </summary>

public abstract class UIItemSlot : MonoBehaviour
{
	public UISprite icon;
	public UIWidget background;
	public UILabel label;

	InvGameItem mItem;
	string mText = "";

	/// <summary>
	/// This function should return the item observed by this UI class.
	/// </summary>

	abstract protected InvGameItem observedItem { get; }

	/// <summary>
	/// Show a tooltip for the item.
	/// </summary>

	void OnTooltip (bool show)
	{
		UITooltip.ShowItem(show ? mItem : null);
	}

	/// <summary>
	/// Keep an eye on the item and update the icon when it changes.
	/// </summary>

	void Update ()
	{
		InvGameItem i = observedItem;

		if (mItem != i)
		{
			mItem = i;

			InvBaseItem baseItem = (i != null) ? i.baseItem : null;

			if (label != null)
			{
				string itemName = (i != null) ? i.name : null;
				if (string.IsNullOrEmpty(mText)) mText = label.text;
				label.text = (itemName != null) ? itemName : mText;
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
				background.color = (i != null) ? i.color : Color.grey;
			}
		}
	}
}