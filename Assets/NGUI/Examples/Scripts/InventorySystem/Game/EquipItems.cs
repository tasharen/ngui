using UnityEngine;

/// <summary>
/// Equip the specified items on the character when the script is started.
/// </summary>

[AddComponentMenu("NGUI/Examples/Equip Items")]
public class EquipItems : MonoBehaviour
{
	public int[] itemIDs;

	void Start ()
	{
		if (itemIDs != null && itemIDs.Length > 0)
		{
			InvEquipment eq = GetComponent<InvEquipment>();
			if (eq == null) eq = gameObject.AddComponent<InvEquipment>();

			int qualityLevels = (int)InvGameItem.Quality._LastDoNotUse;

			foreach (int i in itemIDs)
			{
				InvBaseItem item = InvDatabase.FindByID(i);

				if (item != null)
				{
					InvGameItem gi = new InvGameItem(i, item);
					gi.quality = (InvGameItem.Quality)Random.Range(0, qualityLevels);
					gi.itemLevel = NGUITools.RandomRange(item.minItemLevel, item.maxItemLevel);
					eq.Equip(gi);
				}
				else
				{
					Debug.LogWarning("Can't resolve the item ID of " + i);
				}
			}
		}
		Destroy(this);
	}
}