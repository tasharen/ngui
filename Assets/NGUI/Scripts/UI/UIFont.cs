using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Font")]
public class UIFont : MonoBehaviour
{
	[SerializeField] TextAsset mData;
	[SerializeField] Material mMat;

	BMFont mFont = new BMFont();

	/// <summary>
	/// Get or set the text asset containing the font's exported data.
	/// </summary>

	public TextAsset data
	{
		get
		{
			return mData;
		}
		set
		{
			if (mData != value)
			{
				mData = value;
				mFont.Load(Tools.GetHierarchy(gameObject), (mData != null) ? mData.bytes : null);
				Refresh();
			}
		}
	}

	/// <summary>
	/// Pixel-perfect size of this font.
	/// </summary>

	public int size { get { if (!mFont.isValid) Awake(); return mFont.charSize; } }

	/// <summary>
	/// Get or set the material used by this font.
	/// </summary>

	public Material material
	{
		get
		{
			return mMat;
		}
		set
		{
			if (mMat != value)
			{
				mMat = value;
				Refresh();
			}
		}
	}

	/// <summary>
	/// Load the font data on awake.
	/// </summary>

	void Awake ()
	{
		if (mData != null)
		{
			mFont.Load(Tools.GetHierarchy(gameObject), mData.bytes);
		}
	}

	/// <summary>
	/// Refresh all labels that use this font.
	/// </summary>

	public void Refresh ()
	{
		if (!Application.isPlaying)
		{
			UILabel[] labels = (UILabel[])Object.FindSceneObjectsOfType(typeof(UILabel));

			foreach (UILabel lbl in labels)
			{
				if (lbl.font == this)
				{
					lbl.Refresh();
				}
			}
		}
	}

	/// <summary>
	/// Get the printed size of the specified string.
	/// </summary>

	public Vector2 CalculatePrintedSize (string text, Vector2 scale)
	{
		Vector2 v = Vector2.zero;

		if (mFont != null && mFont.isValid)
		{
			scale.x *= (float)mFont.charSize / mFont.texWidth;
			scale.y *= (float)mFont.charSize / mFont.texHeight;

			int maxX = 0;
			int x = 0;
			int y = 0;
			int prev = 0;

			foreach (char c in text)
			{
				if (c == '\n')
				{
					if (x > maxX) maxX = x;
					x = 0;
					y += mFont.charSize;
				}
				else if (c == '\r') continue;
				else
				{
					BMGlyph glyph = mFont.GetGlyph(c);

					if (glyph != null)
					{
						x += (prev != 0) ? glyph.advance + glyph.GetKerning(prev) : glyph.advance;
						prev = c;
					}
				}
			}

			if (x > maxX) maxX = x;
			v.x = scale.x * maxX;
			v.y = scale.y * (y + mFont.charSize);
		}
		return v;
	}

	/// <summary>
	/// Print the specified text into the buffers.
	/// </summary>

	public void Print (string text, Vector2 scale, Color color, List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		if (mFont != null && mFont.isValid)
		{
			//scale.x *= (float)mFont.charSize / mFont.texWidth;
			//scale.y *= (float)mFont.charSize / mFont.texHeight;

			int maxX = 0;
			int x = 0;
			int y = 0;
			int prev = 0;
			Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
			Vector2 u0 = Vector2.zero, u1 = Vector2.zero;
			float invX = 1f / mFont.texWidth;
			float invY = 1f / mFont.texHeight;

			foreach (char c in text)
			{
				if (c == '\n')
				{
					if (x > maxX) maxX = x;
					x = 0;
					y += mFont.charSize;
				}
				else if (c == '\r') continue;
				else
				{
					BMGlyph glyph = mFont.GetGlyph(c);

					if (glyph != null)
					{
						if (prev != 0) x += glyph.GetKerning(prev);

						v0.x =  scale.x * (x + glyph.offsetX);
						v0.y = -scale.y * (y + glyph.offsetY);

						v1.x = v0.x + scale.x * glyph.width;
						v1.y = v0.y - scale.y * glyph.height;

						u0.x = invX * glyph.x;
						u0.y = 1f - invY * glyph.y;

						u1.x = u0.x + invX * glyph.width;
						u1.y = u0.y - invY * glyph.height;

						verts.Add(new Vector3(v1.x, v0.y, 0f));
						verts.Add(v1);
						verts.Add(new Vector3(v0.x, v1.y, 0f));
						verts.Add(v0);

						uvs.Add(new Vector2(u1.x, u0.y));
						uvs.Add(u1);
						uvs.Add(new Vector2(u0.x, u1.y));
						uvs.Add(u0);

						cols.Add(color);
						cols.Add(color);
						cols.Add(color);
						cols.Add(color);

						x += glyph.advance;
						prev = c;
					}
				}
			}
		}
	}
}