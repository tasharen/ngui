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

			foreach (int i in itemIDs)
			{
				InvItem item = InvDatabase.FindByID(i);

				if (item != null)
				{
					eq.Equip(item);
				}
				else
				{
					Debug.LogWarning("Unable to find the item with the ID of " + i);
				}
			}
		}
		Destroy(this);
	}
}