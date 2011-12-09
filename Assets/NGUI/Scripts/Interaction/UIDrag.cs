using UnityEngine;

/// <summary>
/// Allows dragging of the camera and restricts camera's movement within bounds of the area created by colliders.
/// </summary>

[AddComponentMenu("NGUI/Examples/Drag")]
public class UIDrag : MonoBehaviour
{
	public Transform target;
	public Transform rootForBounds;
	public Vector2 scale = Vector2.one;

	void OnDrag (Vector2 delta)
	{
		if (target != null)
		{
			target.position += new Vector3(delta.x * scale.x, delta.y * scale.y, 0f);
			UpdateTargetPosition();
		}
	}

	public void UpdateTargetPosition ()
	{
		if (rootForBounds != null)
		{
			Collider[] cols = rootForBounds.GetComponentsInChildren<Collider>();

			if (cols.Length > 0)
			{
				Bounds bounds = cols[0].bounds;
				foreach (Collider c in cols) bounds.Encapsulate(c.bounds);

				Vector3 pos = target.position;
				Vector3 min = bounds.min;
				Vector3 max = bounds.max;

				Camera cam = target.GetComponent<Camera>();

				if (cam != null)
				{
					Vector3 bottomLeft = new Vector3(cam.rect.xMin * Screen.width, cam.rect.yMin * Screen.height, 0f);
					Vector3 topRight = new Vector3(cam.rect.xMax * Screen.width, cam.rect.yMax * Screen.height, 0f);

					bottomLeft = cam.ScreenToWorldPoint(bottomLeft);
					topRight = cam.ScreenToWorldPoint(topRight);

					float width = topRight.x - bottomLeft.x;
					float height = topRight.y - bottomLeft.y;
					float halfWidth = width * 0.5f;
					float halfHeight = height * 0.5f;

					Vector3 min0 = min;
					Vector3 min1 = min;
					Vector3 max0 = max;
					Vector3 max1 = max;

					min0.x += halfWidth;
					max0.x -= halfWidth;

					min0.y += halfHeight;
					max0.y -= halfHeight;

					float xSize = (max.x - min.x);
					float ySize = (max.y - min.y);

					min1.x -= halfWidth - xSize;
					max1.x += halfWidth - xSize;

					min1.y -= halfHeight - ySize;
					max1.y += halfHeight - ySize;

					min.x = Mathf.Min(min0.x, min1.x);
					min.y = Mathf.Min(min0.y, min1.y);
					max.x = Mathf.Max(max0.x, max1.x);
					max.y = Mathf.Max(max0.y, max1.y);
				}

				if (max.x < min.x) pos.x = bounds.center.x;
				else pos.x = Mathf.Clamp(pos.x, min.x, max.x);

				if (max.y < min.y) pos.y = bounds.center.y;
				else pos.y = Mathf.Clamp(pos.y, min.y, max.y);

				target.position = pos;
			}
		}
	}
}