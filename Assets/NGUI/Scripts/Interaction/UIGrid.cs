//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// All children added to the game object with this script will be repositioned to be on a grid of specified dimensions.
/// If you want the cells to automatically set their scale based on the dimensions of their content, take a look at UITable.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Grid")]
public class UIGrid : MonoBehaviour
{
	public enum Arrangement
	{
		Horizontal,
		Vertical,
	}

	public Arrangement arrangement = Arrangement.Horizontal;
	public int maxPerLine = 0;
	public float cellWidth = 200f;
	public float cellHeight = 200f;
	public bool repositionNow = false;
	public bool sorted = false;
	public bool hideInactive = true;

	void Start ()
	{
		Reposition();
	}

	void Update ()
	{
		if (repositionNow)
		{
			repositionNow = false;
			Reposition();
		}
	}

	static public int SortByName (Transform a, Transform b) { return string.Compare(a.name, b.name); }

	public void Reposition ()
	{
		Transform myTrans = transform;

		int x = 0;
		int y = 0;

		if (sorted)
		{
			List<Transform> list = new List<Transform>();

			for (int i = 0; i < myTrans.childCount; ++i) list.Add(myTrans.GetChild(i));
			list.Sort(SortByName);

			for (int i = 0, imax = list.Count; i < imax; ++i)
			{
				Transform t = list[i];
				if (!t.gameObject.active && hideInactive) continue;

				t.localPosition = (arrangement == Arrangement.Horizontal) ?
					new Vector3(cellWidth * x, -cellHeight * y, 0f) :
					new Vector3(cellWidth * y, -cellHeight * x, 0f);

				if (++x >= maxPerLine && maxPerLine > 0)
				{
					x = 0;
					++y;
				}
			}
		}
		else
		{
			for (int i = 0; i < myTrans.childCount; ++i)
			{
				Transform t = myTrans.GetChild(i);

				if (!t.gameObject.active && hideInactive) continue;

				t.localPosition = (arrangement == Arrangement.Horizontal) ?
					new Vector3(cellWidth * x, -cellHeight * y, 0f) :
					new Vector3(cellWidth * y, -cellHeight * x, 0f);

				if (++x >= maxPerLine && maxPerLine > 0)
				{
					x = 0;
					++y;
				}
			}
		}

		UIDraggablePanel drag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
		if (drag != null) drag.UpdateScrollbars(true);
	}
}