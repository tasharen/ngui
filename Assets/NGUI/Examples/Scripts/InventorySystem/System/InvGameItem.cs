using UnityEngine;

/// <summary>
/// Since it would be incredibly tedious to create thousands of unique items by hand, a simple solution is needed.
/// Separating items into 2 parts is that solution. Base item contains stats that the item would have if it was max
/// level. All base items are created with their stats at max level. Game item, the second item class, has an
/// effective item level which is used to calculate effective item stats. Game items can be generated with a random
/// level (clamped within base item's min/max level range), and with random quality affecting the item's stats.
/// </summary>

[System.Serializable]
public class InvGameItem
{
	public enum Quality
	{
		Broken,
		Cursed,
		Damaged,
		Worn,
		Sturdy,		// Normal quality
		Polished,
		Improved,
		Crafted,
		Superior,
		Enchanted,
		Epic,
		Legendary,
	}

	// ID of the base item used to create this game item
	[SerializeField] int mBaseItemID = 0;

	/// <summary>
	/// Item quality -- applies a penalty or bonus to all base stats.
	/// </summary>

	public Quality quality = Quality.Sturdy;

	/// <summary>
	/// Item's effective level.
	/// </summary>

	public int itemLevel = 1;

	// Cached for speed
	InvBaseItem mBaseItem;

	/// <summary>
	/// ID of the base item used to create this one.
	/// </summary>

	public int baseItemID { get { return mBaseItemID; } }

	/// <summary>
	/// Base item used by this game item.
	/// </summary>

	public InvBaseItem baseItem
	{
		get
		{
			if (mBaseItem == null)
			{
				mBaseItem = InvDatabase.FindByID(baseItemID);
			}
			return mBaseItem;
		}
	}

	/// <summary>
	/// Game item's name should prefix the quality
	/// </summary>

	public string name
	{
		get
		{
			if (baseItem == null) return null;
			return quality.ToString() + " " + baseItem.name;
		}
	}

	/// <summary>
	/// Put your formula for calculating the item stat modifier here.
	/// Simplest formula -- scale it with quality and item level.
	/// Since all stats on base items are specified at max item level,
	/// calculating the effective multiplier is as simple as itemLevel/maxLevel.
	/// </summary>

	public float statMultiplier
	{
		get
		{
			float mult = 0f;

			switch (quality)
			{
				case Quality.Cursed:	mult = -1f;		break;
				case Quality.Broken:	mult = 0f;		break;
				case Quality.Damaged:	mult = 0.25f;	break;
				case Quality.Worn:		mult = 0.9f;	break;
				case Quality.Sturdy:	mult = 1f;		break;
				case Quality.Polished:	mult = 1.1f;	break;
				case Quality.Improved:	mult = 1.25f;	break;
				case Quality.Crafted:	mult = 1.5f;	break;
				case Quality.Superior:	mult = 1.75f;	break;
				case Quality.Enchanted:	mult = 2f;		break;
				case Quality.Epic:		mult = 2.5f;	break;
				case Quality.Legendary:	mult = 3f;		break;
			}

			// Take item's level into account
			float linear = itemLevel / 50f;

			// Add a curve for more interesting results
			mult *= Mathf.Lerp(linear, linear * linear, 0.5f);
			return mult;
		}
	}

	/// <summary>
	/// Item's color based on quality. You will likely want to change this to your own colors.
	/// </summary>

	public Color color
	{
		get
		{
			Color c = Color.grey;

			switch (quality)
			{
				case Quality.Cursed:	c = Color.red; break;
				case Quality.Broken:	c = new Color(0.1f, 0.1f, 0.1f); break;
				case Quality.Damaged:	c = new Color(0.4f, 0.4f, 0.4f); break;
				case Quality.Worn:		c = new Color(0.7f, 0.7f, 0.7f); break;
				case Quality.Sturdy:	c = new Color(1.0f, 1.0f, 1.0f); break;
				case Quality.Polished:	c = NGUITools.HexToColor(0xe0ffbeff); break;
				case Quality.Improved:	c = NGUITools.HexToColor(0x93d749ff); break;
				case Quality.Crafted:	c = NGUITools.HexToColor(0x4eff00ff); break;
				case Quality.Superior:	c = NGUITools.HexToColor(0x00baffff); break;
				case Quality.Enchanted: c = NGUITools.HexToColor(0x7376fdff); break;
				case Quality.Epic:		c = NGUITools.HexToColor(0x9600ffff); break;
				case Quality.Legendary: c = NGUITools.HexToColor(0xff9000ff); break;
			}
			return c;
		}
	}

	/// <summary>
	/// Create a game item with the specified ID.
	/// </summary>

	public InvGameItem (int id) { mBaseItemID = id; }

	/// <summary>
	/// Create a game item with the specified ID and base item.
	/// </summary>

	public InvGameItem (int id, InvBaseItem bi) { mBaseItemID = id; mBaseItem = bi; }
}