//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Helper class containing generic functions used throughout the UI library.
/// </summary>

static public class NGUIMath
{
	/// <summary>
	/// Lerp function that doesn't clamp the 'factor' in 0-1 range.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public float Lerp (float from, float to, float factor) { return from * (1f - factor) + to * factor; }

	/// <summary>
	/// Clamp the specified integer to be between 0 and below 'max'.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public int ClampIndex (int val, int max) { return (val < 0) ? 0 : (val < max ? val : max - 1); }

	/// <summary>
	/// Wrap the index using repeating logic, so that for example +1 past the end means index of '1'.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public int RepeatIndex (int val, int max)
	{
		if (max < 1) return 0;
		while (val < 0) val += max;
		while (val >= max) val -= max;
		return val;
	}

	/// <summary>
	/// Ensure that the angle is within -180 to 180 range.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public float WrapAngle (float angle)
	{
		while (angle > 180f) angle -= 360f;
		while (angle < -180f) angle += 360f;
		return angle;
	}

	/// <summary>
	/// In the shader, equivalent function would be 'fract'
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public float Wrap01 (float val) { return val - Mathf.FloorToInt(val); }

	/// <summary>
	/// Convert a hexadecimal character to its decimal value.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public int HexToDecimal (char ch)
	{
		switch (ch)
		{
			case '0': return 0x0;
			case '1': return 0x1;
			case '2': return 0x2;
			case '3': return 0x3;
			case '4': return 0x4;
			case '5': return 0x5;
			case '6': return 0x6;
			case '7': return 0x7;
			case '8': return 0x8;
			case '9': return 0x9;
			case 'a':
			case 'A': return 0xA;
			case 'b':
			case 'B': return 0xB;
			case 'c':
			case 'C': return 0xC;
			case 'd':
			case 'D': return 0xD;
			case 'e':
			case 'E': return 0xE;
			case 'f':
			case 'F': return 0xF;
		}
		return 0xF;
	}

	/// <summary>
	/// Convert a single 0-15 value into its hex representation.
	/// It's coded because int.ToString(format) syntax doesn't seem to be supported by Unity's Flash. It just silently crashes.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public char DecimalToHexChar (int num)
	{
		if (num > 15) return 'F';
		if (num < 10) return (char)('0' + num);
		return (char)('A' + num - 10);
	}

	/// <summary>
	/// Convert a decimal value to its hex representation.
	/// It's coded because num.ToString("X6") syntax doesn't seem to be supported by Unity's Flash. It just silently crashes.
	/// string.Format("{0,6:X}", num).Replace(' ', '0') doesn't work either. It returns the format string, not the formatted value.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public string DecimalToHex (int num)
	{
		num &= 0xFFFFFF;
#if UNITY_FLASH
		StringBuilder sb = new StringBuilder();
		sb.Append(DecimalToHexChar((num >> 20) & 0xF));
		sb.Append(DecimalToHexChar((num >> 16) & 0xF));
		sb.Append(DecimalToHexChar((num >> 12) & 0xF));
		sb.Append(DecimalToHexChar((num >> 8) & 0xF));
		sb.Append(DecimalToHexChar((num >> 4) & 0xF));
		sb.Append(DecimalToHexChar(num & 0xF));
		return sb.ToString();
#else
		return num.ToString("X6");
#endif
	}

	/// <summary>
	/// Convert the specified color to RGBA32 integer format.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public int ColorToInt (Color c)
	{
		int retVal = 0;
		retVal |= Mathf.RoundToInt(c.r * 255f) << 24;
		retVal |= Mathf.RoundToInt(c.g * 255f) << 16;
		retVal |= Mathf.RoundToInt(c.b * 255f) << 8;
		retVal |= Mathf.RoundToInt(c.a * 255f);
		return retVal;
	}

	/// <summary>
	/// Convert the specified RGBA32 integer to Color.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public Color IntToColor (int val)
	{
		float inv = 1f / 255f;
		Color c = Color.black;
		c.r = inv * ((val >> 24) & 0xFF);
		c.g = inv * ((val >> 16) & 0xFF);
		c.b = inv * ((val >> 8) & 0xFF);
		c.a = inv * (val & 0xFF);
		return c;
	}

	/// <summary>
	/// Convert the specified integer to a human-readable string representing the binary value. Useful for debugging bytes.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public string IntToBinary (int val, int bits)
	{
		string final = "";

		for (int i = bits; i > 0; )
		{
			if (i == 8 || i == 16 || i == 24) final += " ";
			final += ((val & (1 << --i)) != 0) ? '1' : '0';
		}
		return final;
	}

	/// <summary>
	/// Convenience conversion function, allowing hex format (0xRrGgBbAa).
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	static public Color HexToColor (uint val)
	{
		return IntToColor((int)val);
	}

	/// <summary>
	/// Convert from top-left based pixel coordinates to bottom-left based UV coordinates.
	/// </summary>

	static public Rect ConvertToTexCoords (Rect rect, int width, int height)
	{
		Rect final = rect;

		if (width != 0f && height != 0f)
		{
			final.xMin = rect.xMin / width;
			final.xMax = rect.xMax / width;
			final.yMin = 1f - rect.yMax / height;
			final.yMax = 1f - rect.yMin / height;
		}
		return final;
	}

	/// <summary>
	/// Convert from bottom-left based UV coordinates to top-left based pixel coordinates.
	/// </summary>

	static public Rect ConvertToPixels (Rect rect, int width, int height, bool round)
	{
		Rect final = rect;

		if (round)
		{
			final.xMin = Mathf.RoundToInt(rect.xMin * width);
			final.xMax = Mathf.RoundToInt(rect.xMax * width);
			final.yMin = Mathf.RoundToInt((1f - rect.yMax) * height);
			final.yMax = Mathf.RoundToInt((1f - rect.yMin) * height);
		}
		else
		{
			final.xMin = rect.xMin * width;
			final.xMax = rect.xMax * width;
			final.yMin = (1f - rect.yMax) * height;
			final.yMax = (1f - rect.yMin) * height;
		}
		return final;
	}

	/// <summary>
	/// Round the pixel rectangle's dimensions.
	/// </summary>

	static public Rect MakePixelPerfect (Rect rect)
	{
		rect.xMin = Mathf.RoundToInt(rect.xMin);
		rect.yMin = Mathf.RoundToInt(rect.yMin);
		rect.xMax = Mathf.RoundToInt(rect.xMax);
		rect.yMax = Mathf.RoundToInt(rect.yMax);
		return rect;
	}

	/// <summary>
	/// Round the texture coordinate rectangle's dimensions.
	/// </summary>

	static public Rect MakePixelPerfect (Rect rect, int width, int height)
	{
		rect = ConvertToPixels(rect, width, height, true);
		rect.xMin = Mathf.RoundToInt(rect.xMin);
		rect.yMin = Mathf.RoundToInt(rect.yMin);
		rect.xMax = Mathf.RoundToInt(rect.xMax);
		rect.yMax = Mathf.RoundToInt(rect.yMax);
		return ConvertToTexCoords(rect, width, height);
	}

	/// <summary>
	/// Constrain 'rect' to be within 'area' as much as possible, returning the Vector2 offset necessary for this to happen.
	/// This function is useful when trying to restrict one area (window) to always be within another (viewport).
	/// </summary>

	static public Vector2 ConstrainRect (Vector2 minRect, Vector2 maxRect, Vector2 minArea, Vector2 maxArea)
	{
		Vector2 offset = Vector2.zero;

		float contentX = maxRect.x - minRect.x;
		float contentY = maxRect.y - minRect.y;

		float areaX = maxArea.x - minArea.x;
		float areaY = maxArea.y - minArea.y;

		if (contentX > areaX)
		{
			float diff = contentX - areaX;
			minArea.x -= diff;
			maxArea.x += diff;
		}

		if (contentY > areaY)
		{
			float diff = contentY - areaY;
			minArea.y -= diff;
			maxArea.y += diff;
		}

		if (minRect.x < minArea.x) offset.x += minArea.x - minRect.x;
		if (maxRect.x > maxArea.x) offset.x -= maxRect.x - maxArea.x;
		if (minRect.y < minArea.y) offset.y += minArea.y - minRect.y;
		if (maxRect.y > maxArea.y) offset.y -= maxRect.y - maxArea.y;
		
		return offset;
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in world space).
	/// </summary>

	static public Bounds CalculateAbsoluteWidgetBounds (Transform trans)
	{
		UIWidget[] widgets = trans.GetComponentsInChildren<UIWidget>() as UIWidget[];
		if (widgets.Length == 0) return new Bounds(trans.position, Vector3.zero);

		Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

		for (int i = 0, imax = widgets.Length; i < imax; ++i)
		{
			UIWidget w = widgets[i];
			if (!w.enabled) continue;

			Vector3[] corners = w.worldCorners;

			for (int j = 0; j < 4; ++j)
			{
				vMax = Vector3.Max(corners[j], vMax);
				vMin = Vector3.Min(corners[j], vMin);
			}
		}

		Bounds b = new Bounds(vMin, Vector3.zero);
		b.Encapsulate(vMax);
		return b;
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
	/// </summary>

	static public Bounds CalculateRelativeWidgetBounds (Transform trans)
	{
		return CalculateRelativeWidgetBounds(trans, trans, false);
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
	/// </summary>

	static public Bounds CalculateRelativeWidgetBounds (Transform trans, bool considerInactive)
	{
		return CalculateRelativeWidgetBounds(trans, trans, considerInactive);
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
	/// </summary>

	static public Bounds CalculateRelativeWidgetBounds (Transform root, Transform child)
	{
		return CalculateRelativeWidgetBounds(root, child, false);
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
	/// </summary>

	static public Bounds CalculateRelativeWidgetBounds (Transform root, Transform child, bool considerInactive)
	{
		UIWidget[] widgets = child.GetComponentsInChildren<UIWidget>(considerInactive);

		if (widgets.Length > 0)
		{
			Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			Matrix4x4 toLocal = root.worldToLocalMatrix;
			bool isSet = false;
			Vector3 v;

			for (int i = 0, imax = widgets.Length; i < imax; ++i)
			{
				UIWidget w = widgets[i];
				if (!considerInactive && !w.enabled) continue;

				Vector3[] corners = w.worldCorners;

				for (int j = 0; j < 4; ++j)
				{
					//v = root.InverseTransformPoint(corners[j]);
					v = toLocal.MultiplyPoint3x4(corners[j]);
					vMax = Vector3.Max(v, vMax);
					vMin = Vector3.Min(v, vMin);
				}
				isSet = true;
			}

			if (isSet)
			{
				Bounds b = new Bounds(vMin, Vector3.zero);
				b.Encapsulate(vMax);
				return b;
			}
		}
		return new Bounds(Vector3.zero, Vector3.zero);
	}

	/// <summary>
	/// This code is not framerate-independent:
	/// 
	/// target.position += velocity;
	/// velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 9f);
	/// 
	/// But this code is:
	/// 
	/// target.position += NGUIMath.SpringDampen(ref velocity, 9f, Time.deltaTime);
	/// </summary>

	static public Vector3 SpringDampen (ref Vector3 velocity, float strength, float deltaTime)
	{
		if (deltaTime > 1f) deltaTime = 1f;
		float dampeningFactor = 1f - strength * 0.001f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		float totalDampening = Mathf.Pow(dampeningFactor, ms);
		Vector3 vTotal = velocity * ((totalDampening - 1f) / Mathf.Log(dampeningFactor));
		velocity = velocity * totalDampening;
		return vTotal * 0.06f;
	}

	/// <summary>
	/// Same as the Vector3 version, it's a framerate-independent Lerp.
	/// </summary>

	static public Vector2 SpringDampen (ref Vector2 velocity, float strength, float deltaTime)
	{
		if (deltaTime > 1f) deltaTime = 1f;
		float dampeningFactor = 1f - strength * 0.001f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		float totalDampening = Mathf.Pow(dampeningFactor, ms);
		Vector2 vTotal = velocity * ((totalDampening - 1f) / Mathf.Log(dampeningFactor));
		velocity = velocity * totalDampening;
		return vTotal * 0.06f;
	}

	/// <summary>
	/// Calculate how much to interpolate by.
	/// </summary>

	static public float SpringLerp (float strength, float deltaTime)
	{
		if (deltaTime > 1f) deltaTime = 1f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		deltaTime = 0.001f * strength;
		float cumulative = 0f;
		for (int i = 0; i < ms; ++i) cumulative = Mathf.Lerp(cumulative, 1f, deltaTime);
		return cumulative;
	}

	/// <summary>
	/// Mathf.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public float SpringLerp (float from, float to, float strength, float deltaTime)
	{
		if (deltaTime > 1f) deltaTime = 1f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		deltaTime = 0.001f * strength;
		for (int i = 0; i < ms; ++i) from = Mathf.Lerp(from, to, deltaTime);
		return from;
	}

	/// <summary>
	/// Vector2.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public Vector2 SpringLerp (Vector2 from, Vector2 to, float strength, float deltaTime)
	{
		return Vector2.Lerp(from, to, SpringLerp(strength, deltaTime));
	}

	/// <summary>
	/// Vector3.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public Vector3 SpringLerp (Vector3 from, Vector3 to, float strength, float deltaTime)
	{
		return Vector3.Lerp(from, to, SpringLerp(strength, deltaTime));
	}

	/// <summary>
	/// Quaternion.Slerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public Quaternion SpringLerp (Quaternion from, Quaternion to, float strength, float deltaTime)
	{
		return Quaternion.Slerp(from, to, SpringLerp(strength, deltaTime));
	}

	/// <summary>
	/// Since there is no Mathf.RotateTowards...
	/// </summary>

	static public float RotateTowards (float from, float to, float maxAngle)
	{
		float diff = WrapAngle(to - from);
		if (Mathf.Abs(diff) > maxAngle) diff = maxAngle * Mathf.Sign(diff);
		return from + diff;
	}

	/// <summary>
	/// Determine the distance from the specified point to the line segment.
	/// </summary>

	static float DistancePointToLineSegment (Vector2 point, Vector2 a, Vector2 b)
	{
		float l2 = (b - a).sqrMagnitude;
		if (l2 == 0f) return (point - a).magnitude;
		float t = Vector2.Dot(point - a, b - a) / l2;
		if (t < 0f) return (point - a).magnitude;
		else if (t > 1f) return (point - b).magnitude;
		Vector2 projection = a + t * (b - a);
		return (point - projection).magnitude;
	}

	/// <summary>
	/// Determine the distance from the mouse position to the screen space rectangle specified by the 4 points.
	/// </summary>

	static public float DistanceToRectangle (Vector2[] screenPoints, Vector2 mousePos)
	{
		bool oddNodes = false;
		int j = 4;

		for (int i = 0; i < 5; i++)
		{
			Vector3 v0 = screenPoints[NGUIMath.RepeatIndex(i, 4)];
			Vector3 v1 = screenPoints[NGUIMath.RepeatIndex(j, 4)];

			if ((v0.y > mousePos.y) != (v1.y > mousePos.y))
			{
				if (mousePos.x < (v1.x - v0.x) * (mousePos.y - v0.y) / (v1.y - v0.y) + v0.x)
				{
					oddNodes = !oddNodes;
				}
			}
			j = i;
		}

		if (!oddNodes)
		{
			float dist, closestDist = -1f;

			for (int i = 0; i < 4; i++)
			{
				Vector3 v0 = screenPoints[i];
				Vector3 v1 = screenPoints[NGUIMath.RepeatIndex(i + 1, 4)];

				dist = DistancePointToLineSegment(mousePos, v0, v1);

				if (dist < closestDist || closestDist < 0f) closestDist = dist;
			}
			return closestDist;
		}
		else return 0f;
	}

	/// <summary>
	/// Determine the distance from the mouse position to the world rectangle specified by the 4 points.
	/// </summary>

	static public float DistanceToRectangle (Vector3[] worldPoints, Vector2 mousePos, Camera cam)
	{
		Vector2[] screenPoints = new Vector2[4];
		for (int i = 0; i < 4; ++i)
			screenPoints[i] = cam.WorldToScreenPoint(worldPoints[i]);
		return DistanceToRectangle(screenPoints, mousePos);
	}

	/// <summary>
	/// Helper function that converts the widget's pivot enum into a 0-1 range vector.
	/// </summary>

	static public Vector2 GetPivotOffset (UIWidget.Pivot pv)
	{
		Vector2 v = Vector2.zero;

		if (pv == UIWidget.Pivot.Top || pv == UIWidget.Pivot.Center || pv == UIWidget.Pivot.Bottom) v.x = 0.5f;
		else if (pv == UIWidget.Pivot.TopRight || pv == UIWidget.Pivot.Right || pv == UIWidget.Pivot.BottomRight) v.x = 1f;
		else v.x = 0f;

		if (pv == UIWidget.Pivot.Left || pv == UIWidget.Pivot.Center || pv == UIWidget.Pivot.Right) v.y = 0.5f;
		else if (pv == UIWidget.Pivot.TopLeft || pv == UIWidget.Pivot.Top || pv == UIWidget.Pivot.TopRight) v.y = 1f;
		else v.y = 0f;

		return v;
	}

	/// <summary>
	/// Adjust the widget's position using the specified local delta coordinates.
	/// </summary>

	static public void MoveWidget (UIWidget w, float x, float y)
	{
		int ix = Mathf.FloorToInt(x + 0.5f);
		int iy = Mathf.FloorToInt(y + 0.5f);

		Transform t = w.cachedTransform;
		t.localPosition += new Vector3(ix, iy);
		int anchorCount = 0;

		if (w.leftAnchor.target)
		{
			++anchorCount;
			w.leftAnchor.absolute += ix;
		}

		if (w.rightAnchor.target)
		{
			++anchorCount;
			w.rightAnchor.absolute += ix;
		}

		if (w.bottomAnchor.target)
		{
			++anchorCount;
			w.bottomAnchor.absolute += iy;
		}

		if (w.topAnchor.target)
		{
			++anchorCount;
			w.topAnchor.absolute += iy;
		}

		// If all sides were anchored, we're done
		if (anchorCount != 0) w.UpdateAnchors();
	}

	/// <summary>
	/// Given the specified dragged pivot point, adjust the widget's dimensions.
	/// </summary>

	static public void ResizeWidget (UIWidget w, UIWidget.Pivot pivot, float x, float y, int minWidth, int minHeight)
	{
		if (pivot == UIWidget.Pivot.Center)
		{
			MoveWidget(w, x, y);
			return;
		}

		Vector3 v = new Vector3(x, y);
		v = Quaternion.Inverse(w.cachedTransform.localRotation) * v;

		switch (pivot)
		{
			case UIWidget.Pivot.BottomLeft:
			ResizeWidget(w, v.x, v.y, 0, 0, minWidth, minHeight);
			break;

			case UIWidget.Pivot.Left:
			ResizeWidget(w, v.x, 0, 0, 0, minWidth, minHeight);
			break;

			case UIWidget.Pivot.TopLeft:
			ResizeWidget(w, v.x, 0, 0, v.y, minWidth, minHeight);
			break;

			case UIWidget.Pivot.Top:
			ResizeWidget(w, 0, 0, 0, v.y, minWidth, minHeight);
			break;

			case UIWidget.Pivot.TopRight:
			ResizeWidget(w, 0, 0, v.x, v.y, minWidth, minHeight);
			break;

			case UIWidget.Pivot.Right:
			ResizeWidget(w, 0, 0, v.x, 0, minWidth, minHeight);
			break;

			case UIWidget.Pivot.BottomRight:
			ResizeWidget(w, 0, v.y, v.x, 0, minWidth, minHeight);
			break;

			case UIWidget.Pivot.Bottom:
			ResizeWidget(w, 0, v.y, 0, 0, minWidth, minHeight);
			break;
		}
	}

	/// <summary>
	/// Adjust the widget's rectangle based on the specified modifier values.
	/// </summary>

	static public void ResizeWidget (UIWidget w, float left, float bottom, float right, float top, int minWidth, int minHeight)
	{
		// This entire function can be replaced with the line below, but it would only work for non-rotated widgets.
		w.AdjustRect(left, bottom, right, top, minWidth, minHeight);

		/*int anchorCount = 0;

		if (w.leftAnchor.target)
		{
		    ++anchorCount;
		    w.leftAnchor.absolute += Mathf.FloorToInt(left);
		    left = 0;
		}

		if (w.rightAnchor.target)
		{
		    ++anchorCount;
		    w.rightAnchor.absolute += Mathf.FloorToInt(right);
		    right = 0;
		}

		if (w.bottomAnchor.target)
		{
		    ++anchorCount;
		    w.bottomAnchor.absolute += Mathf.FloorToInt(bottom);
		    bottom = 0;
		}

		if (w.topAnchor.target)
		{
		    ++anchorCount;
		    w.topAnchor.absolute += Mathf.FloorToInt(top);
		    top = 0;
		}

		if (anchorCount != 0) w.UpdateAnchors();
		if (anchorCount == 4) return;

		Transform t = w.cachedTransform;
		Vector2 pivot = w.pivotOffset;
		Vector3 pos = t.localPosition;
		Quaternion rot = t.localRotation;
		int width = w.width;
		int height = w.height;

		int il = Mathf.FloorToInt(left + 0.5f);
		int ir = Mathf.FloorToInt(right + 0.5f);
		int ib = Mathf.FloorToInt(bottom + 0.5f);
		int it = Mathf.FloorToInt(top + 0.5f);

		// Centered pivot means adjustments should be done by two pixels instead of 1
		if (pivot.x == 0.5f)
		{
			ir = ((ir >> 1) << 1);
			il = ((il >> 1) << 1);
		}

		if (pivot.y == 0.5f)
		{
			ib = ((ib >> 1) << 1);
			it = ((it >> 1) << 1);
		}

		width += ir - il;
		height += it - ib;

		left = il;
		right = ir;
		bottom = ib;
		top = it;

		// Centered pivot means width and height must be dividable by two
		if (pivot.x == 0.5f) width = ((width >> 1) << 1);
		if (pivot.y == 0.5f) height = ((height >> 1) << 1);

		Vector3 rotatedTL = rot * new Vector3(left, top);
		Vector3 rotatedTR = rot * new Vector3(right, top);
		Vector3 rotatedBL = rot * new Vector3(left, bottom);
		Vector3 rotatedBR = rot * new Vector3(right, bottom);
		Vector3 rotatedL = rot * new Vector3(left, 0f);
		Vector3 rotatedR = rot * new Vector3(right, 0f);
		Vector3 rotatedT = rot * new Vector3(0f, top);
		Vector3 rotatedB = rot * new Vector3(0f, bottom);

		Vector3 offset = Vector3.zero;

		if (pivot.x == 0f && pivot.y == 1f)
		{
			offset.x = rotatedTL.x;
			offset.y = rotatedTL.y;
		}
		else if (pivot.x == 1f && pivot.y == 0f)
		{
			offset.x = rotatedBR.x;
			offset.y = rotatedBR.y;
		}
		else if (pivot.x == 0f && pivot.y == 0f)
		{
			offset.x = rotatedBL.x;
			offset.y = rotatedBL.y;
		}
		else if (pivot.x == 1f && pivot.y == 1f)
		{
			offset.x = rotatedTR.x;
			offset.y = rotatedTR.y;
		}
		else if (pivot.x == 0f && pivot.y == 0.5f)
		{
			offset.x = rotatedL.x + (rotatedT.x + rotatedB.x) * 0.5f;
			offset.y = rotatedL.y + (rotatedT.y + rotatedB.y) * 0.5f;
		}
		else if (pivot.x == 1f && pivot.y == 0.5f)
		{
			offset.x = rotatedR.x + (rotatedT.x + rotatedB.x) * 0.5f;
			offset.y = rotatedR.y + (rotatedT.y + rotatedB.y) * 0.5f;
		}
		else if (pivot.x == 0.5f && pivot.y == 1f)
		{
			offset.x = rotatedT.x + (rotatedL.x + rotatedR.x) * 0.5f;
			offset.y = rotatedT.y + (rotatedL.y + rotatedR.y) * 0.5f;
		}
		else if (pivot.x == 0.5f && pivot.y == 0f)
		{
			offset.x = rotatedB.x + (rotatedL.x + rotatedR.x) * 0.5f;
			offset.y = rotatedB.y + (rotatedL.y + rotatedR.y) * 0.5f;
		}
		else if (pivot.x == 0.5f && pivot.y == 0.5f)
		{
			offset.x = (rotatedL.x + rotatedR.x + rotatedT.x + rotatedB.x) * 0.5f;
			offset.y = (rotatedT.y + rotatedB.y + rotatedL.y + rotatedR.y) * 0.5f;
		}
		else
		{
			Debug.LogWarning("Pivot " + pivot + " dragging is not supported");
		}

		pos += offset;

		int minx = Mathf.Max(minWidth, w.minWidth);
		int miny = Mathf.Max(minHeight, w.minHeight);

		offset = Vector3.zero;

		if (width < minx)
		{
			if (il != 0)
			{
				offset.x -= Mathf.Lerp(minx - width, 0f, pivot.x);
			}
			else
			{
				offset.x += Mathf.Lerp(0f, minx - width, pivot.x);
			}
			width = minx;
		}

		if (height < miny)
		{
			if (ib != 0)
			{
				offset.y -= Mathf.Lerp(miny - height, 0f, pivot.y);
			}
			else
			{
				offset.y += Mathf.Lerp(0f, miny - height, pivot.y);
			}
			height = miny;
		}

		t.localPosition = pos + rot * offset;
		w.width = width;
		w.height = height;*/
	}
}
