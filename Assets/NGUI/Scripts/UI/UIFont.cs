using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UIFont contains everything needed to be able to print text.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Font")]
public class UIFont : MonoBehaviour
{
	[SerializeField] TextAsset mData;
	[SerializeField] Material mMat;
	[SerializeField] Rect mUVRect = new Rect(0f, 0f, 1f, 1f);

	BMFont mFont = new BMFont();
	Stack<Color> mColors = new Stack<Color>();

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
				mFont.Load(NGUITools.GetHierarchy(gameObject), (mData != null) ? mData.bytes : null);
				Refresh();
			}
		}
	}

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
	/// Offset and scale applied to all UV coordinates.
	/// </summary>

	public Rect uvRect
	{
		get
		{
			return mUVRect;
		}
		set
		{
			if (mUVRect != value)
			{
				mUVRect = value;
				Refresh();
			}
		}
	}

	/// <summary>
	/// Pixel-perfect size of this font.
	/// </summary>

	public int size { get { Awake(); return mFont.charSize; } }

	/// <summary>
	/// Load the font data on awake.
	/// </summary>

	void Awake ()
	{
		if (mData != null && !mFont.isValid)
		{
			mFont.Load(NGUITools.GetHierarchy(gameObject), mData.bytes);
		}
	}

	/// <summary>
	/// Refresh all labels that use this font.
	/// </summary>

	void Refresh ()
	{
		if (!Application.isPlaying)
		{
			UILabel[] labels = (UILabel[])Object.FindSceneObjectsOfType(typeof(UILabel));

			foreach (UILabel lbl in labels)
			{
				if (lbl.font == this)
				{
					lbl.MarkAsChanged();
				}
			}
		}
	}

	/// <summary>
	/// Get the printed size of the specified string.
	/// </summary>

	public Vector2 CalculatePrintedSize (string text, bool encoding)
	{
		Vector2 v = Vector2.zero;

		if (mFont != null)
		{
			Awake();

			if (mFont.isValid)
			{
				if (encoding) text = NGUITools.StripSymbols(text);

				Vector2 scale = mFont.charSize > 0 ? new Vector2(1f / mFont.charSize, 1f / mFont.charSize) : Vector2.one;

				int maxX = 0;
				int x = 0;
				int y = 0;
				int prev = 0;

				for (int i = 0, imax = text.Length; i < imax; ++i)
				{
					char c = text[i];

					if (c == '\n' || (encoding && (c == '\\') && (i + 1 < imax) && (text[i + 1] == 'n')))
					{
						if (x > maxX) maxX = x;
						x = 0;
						y += mFont.charSize;
						prev = 0;
						if (c != '\n') ++i;
						continue;
					}

					if (c < ' ')
					{
						prev = 0;
						continue;
					}

					BMGlyph glyph = mFont.GetGlyph(c);

					if (glyph != null)
					{
						x += (prev != 0) ? glyph.advance + glyph.GetKerning(prev) : glyph.advance;
						prev = c;
					}
				}

				if (x > maxX) maxX = x;
				v.x = scale.x * maxX;
				v.y = scale.y * (y + mFont.charSize);
			}
		}
		return v;
	}

	/// <summary>
	/// Print the specified text into the buffers.
	/// </summary>

	public void Print (string text, Color color, List<Vector3> verts, List<Vector2> uvs, List<Color> cols, bool encoding)
	{
		if (mFont != null)
		{
			Awake();

			if (!mFont.isValid)
			{
				Debug.LogError("Attempting to print using an invalid font!");
				return;
			}

			mColors.Clear();
			mColors.Push(color);

			Vector2 scale = mFont.charSize > 0 ? new Vector2(1f / mFont.charSize, 1f / mFont.charSize) : Vector2.one;

			int maxX = 0;
			int x = 0;
			int y = 0;
			int prev = 0;
			Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
			Vector2 u0 = Vector2.zero, u1 = Vector2.zero;
			float invX = mUVRect.width / mFont.texWidth;
			float invY = mUVRect.height / mFont.texHeight;
			float invYUV = 1f - mUVRect.yMax;

			for (int i = 0, imax = text.Length; i < imax; ++i)
			{
				char c = text[i];

				if (c == '\n' || (encoding && (c == '\\') && (i + 1 < imax) && (text[i + 1] == 'n')))
				{
					if (x > maxX) maxX = x;
					x = 0;
					y += mFont.charSize;
					prev = 0;
					if (c != '\n') ++i;
					continue;
				}

				if (c < ' ')
				{
					prev = 0;
					continue;
				}

				if (encoding && c == '[')
				{
					int retVal = NGUITools.ParseSymbol(text, i, mColors);

					if (retVal > 0)
					{
						color = mColors.Peek();
						i += retVal - 1;
						continue;
					}
				}

				BMGlyph glyph = mFont.GetGlyph(c);

				if (glyph != null)
				{
					if (prev != 0) x += glyph.GetKerning(prev);

					v0.x =  scale.x * (x + glyph.offsetX);
					v0.y = -scale.y * (y + glyph.offsetY);

					v1.x = v0.x + scale.x * glyph.width;
					v1.y = v0.y - scale.y * glyph.height;

					u0.x = mUVRect.xMin + invX * glyph.x;
					u0.y = invYUV + (1f - invY * glyph.y);

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