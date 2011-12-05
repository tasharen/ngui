using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Inventory System Tools
/// </summary>

public static class InvTools
{
	/// <summary>
	/// Combine the stats of specified list of items.
	/// </summary>

	static public List<InvStat> CombineStats (InvItem[] items)
	{
		List<InvStat> list = new List<InvStat>();

		if (items != null)
		{
			foreach (InvItem item in items)
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
	}
}