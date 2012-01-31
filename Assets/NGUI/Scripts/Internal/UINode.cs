using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UIPanel creates one of these records for each child transform under it.
/// This makes it possible to watch for transform changes, and if something does
/// change -- rebuild the buffer as necessary.
/// </summary>

public class UINode
{
	int mVisibleFlag = -1;

	public Transform trans;			// Managed transform
	public Transform parent;		// Transform's parent, cached to see if it changes
	public UIWidget widget;			// Widget on this transform, if any

	public bool lastActive = false;	// Last active state
	public Vector3 lastPos;			// Last local position, used to see if it has changed
	public Quaternion lastRot;		// Last local rotation
	public Vector3 lastScale;		// Last local scale

	public List<Vector3> verts;		// Widget's vertices (before they get transformed)
	public List<Vector2> uvs;		// Widget's UVs
	public List<Color> cols;		// Widget's colors

	public List<Vector3> rtpVerts;	// Relative-to-panel vertices
	public Vector3 rtpNormal;		// Relative-to-panel normal
	public Vector4 rtpTan;			// Relative-to-panel tangent

	public int changeFlag = -1;		// -1 = not checked, 0 = not changed, 1 = changed

	/// <summary>
	/// -1 = not initialized, 0 = not visible, 1 = visible.
	/// </summary>

	public int visibleFlag
	{
		get
		{
			return (widget != null) ? widget.visibleFlag : mVisibleFlag;
		}
		set
		{
			if (widget != null) widget.visibleFlag = value;
			else mVisibleFlag = value;
		}
	}

	/// <summary>
	/// Must always have a transform.
	/// </summary>

	public UINode (Transform t)
	{
		trans = t;
		lastPos = trans.localPosition;
		lastRot = trans.localRotation;
		lastScale = trans.localScale;
	}

	/// <summary>
	/// Check to see if the local transform has changed since the last time this function was called.
	/// </summary>

	public bool HasChanged ()
	{
		bool isActive = trans.gameObject.active && (widget == null || (widget.enabled && widget.color.a > 0.001f));

		if (lastActive != isActive || (isActive &&
			(lastPos != trans.localPosition ||
			 lastRot != trans.localRotation ||
			 lastScale != trans.localScale)))
		{
			lastActive = isActive;
			lastPos = trans.localPosition;
			lastRot = trans.localRotation;
			lastScale = trans.localScale;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Rebuild the widget buffers.
	/// </summary>

	public void Rebuild (Vector3 pivotOffset)
	{
		// Cleanup
		if (verts	 != null) verts.Clear();
		if (uvs		 != null) uvs.Clear();
		if (cols	 != null) cols.Clear();
		if (rtpVerts != null) rtpVerts.Clear();

		// Ensure we have buffers to work with
		if (verts	== null) verts	= new List<Vector3>();
		if (uvs		== null) uvs	= new List<Vector2>();
		if (cols	== null) cols	= new List<Color>();

		// Fill the buffers
		widget.OnFill(verts, uvs, cols);

		// Append the offset
		for (int i = 0, imax = verts.Count; i < imax; ++i) verts[i] += pivotOffset;
	}

	/// <summary>
	/// Transform the vertices by the provided matrix.
	/// </summary>

	public void TransformVerts (Matrix4x4 widgetToPanel, bool normals)
	{
		if (rtpVerts != null) rtpVerts.Clear();

		if (verts != null && verts.Count > 0)
		{
			if (rtpVerts == null) rtpVerts = new List<Vector3>();

			// Transform all vertices from widget space to panel space
			foreach (Vector3 v in verts) rtpVerts.Add(widgetToPanel.MultiplyPoint3x4(v));

			// Calculate the widget's normal and tangent
			rtpNormal = widgetToPanel.MultiplyVector(Vector3.back).normalized;
			Vector3 tangent = widgetToPanel.MultiplyVector(Vector3.right).normalized;
			rtpTan = new Vector4(tangent.x, tangent.y, tangent.z, -1f);
		}
	}

	/// <summary>
	/// Fill the specified buffer.
	/// </summary>

	public void Fill (List<Vector3> v, List<Vector2> u, List<Color> c, List<Vector3> n, List<Vector4> t)
	{
		if (rtpVerts != null && rtpVerts.Count > 0)
		{
			if (n == null)
			{
				for (int i = 0, imax = rtpVerts.Count; i < imax; ++i)
				{
					v.Add(rtpVerts[i]);
					u.Add(uvs[i]);
					c.Add(cols[i]);
				}
			}
			else
			{
				for (int i = 0, imax = rtpVerts.Count; i < imax; ++i)
				{
					v.Add(rtpVerts[i]);
					u.Add(uvs[i]);
					c.Add(cols[i]);
					n.Add(rtpNormal);
					t.Add(rtpTan);
				}
			}
		}
	}
}