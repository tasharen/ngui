using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UIPanel creates one of these records for each child transform under it.
/// This makes it possible to watch for transform changes, and if something does
/// change -- rebuild the buffer as necessary.
/// </summary>

public class UINode
{
	public class Data
	{
		public List<Vector3> verts;
		public List<Vector3> norms;
		public List<Vector3> tans;

		public void Clear ()
		{
			if (verts != null) verts.Clear();
			if (norms != null) norms.Clear();
			if (tans != null) tans.Clear();
		}

		public void Release ()
		{
			verts = null;
			norms = null;
			tans = null;
		}
	}

	int mVisibleFlag = -1;

	public Transform trans;			// Managed transform
	public UIWidget widget;			// Widget on this transform, if any

	public Vector3 lastPos;			// Last local position, used to see if it has changed
	public Quaternion lastRot;		// Last local rotation
	public Vector3 lastScale;		// Last local scale

	public Data raw;				// Raw vertices, filled by the widget
	public Data transformed;		// Transformed vertices, relative to the panel

	public List<Vector2> uvs;		// Widget's UVs
	public List<Color> cols;		// Widget's colors

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

	public void Clear ()
	{
		if (raw != null) raw.Clear();
		if (transformed != null) transformed.Clear();
		if (uvs != null) uvs.Clear();
		if (cols != null) cols.Clear();
	}

	public void Release ()
	{
		if (raw != null) raw.Release();
		if (transformed != null) transformed.Release();
		raw = null;
		transformed = null;
		uvs = null;
		cols = null;
	}

	/// <summary>
	/// Check to see if the local transform has changed since the last time this function was called.
	/// </summary>

	public bool HasChanged ()
	{
		if (lastPos != trans.localPosition ||
			lastRot != trans.localRotation ||
			lastScale != trans.localScale)
		{
			lastPos = trans.localPosition;
			lastRot = trans.localRotation;
			lastScale = trans.localScale;
			return true;
		}
		return false;
	}
}