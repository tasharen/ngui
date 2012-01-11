using UnityEngine;

/// <summary>
/// All children added to the game object with this script will be repositioned to be on a grid of specified dimensions.
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
	public int maxRows = 0;
	public float cellWidth = 200f;
	public float cellHeight = 200f;
	public bool repositionNow = false;

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

	public void Reposition ()
	{
		Transform myTrans = transform;

		int x = 0;
		int y = 0;

		for (int i = 0; i < myTrans.childCount; ++i)
		{
			Transform t = myTrans.GetChild(i);

			t.localPosition = (arrangement == Arrangement.Horizontal) ?
				new Vector3(cellWidth * x, -cellHeight * y, 0f) :
				new Vector3(cellWidth * y, -cellHeight * x, 0f);

			if (x++ >= maxRows && maxRows > 0)
			{
				x = 0;
				++y;
			}
		}
	}
}