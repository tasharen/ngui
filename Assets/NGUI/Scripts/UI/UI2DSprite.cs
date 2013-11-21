//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 2D Sprite is capable of drawing sprites added in Unity 4.3. When importing your textures,
/// import them as Sprites and you will be able to draw them with this widget.
/// If you provide a Packing Tag in your import settings, your sprites will get automatically
/// packed into an atlas for you, so creating an atlas beforehand is not necessary.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Unity2D Sprite")]
public class UI2DSprite : UIWidget
{
	[HideInInspector][SerializeField] Sprite mSprite;
	[HideInInspector][SerializeField] Material mMat;
	[HideInInspector][SerializeField] Shader mShader;

	int mPMA = -1;

	/// <summary>
	/// UnityEngine.Sprite drawn by this widget.
	/// </summary>

	public Sprite sprite2D
	{
		get
		{
			return mSprite;
		}
		set
		{
			if (mSprite != value)
			{
				mSprite = value;
				RemoveFromPanel();
			}
		}
	}

	/// <summary>
	/// Material used by the widget.
	/// </summary>

	public override Material material
	{
		get
		{
			return mMat;
		}
		set
		{
			if (mMat != value)
			{
				MarkAsChanged();
				drawCall = null;
				mPanel = null;
				mMat = value;
				mPMA = -1;
				MarkAsChanged();
			}
		}
	}

	/// <summary>
	/// Shader used by the texture when creating a dynamic material (when the texture was specified, but the material was not).
	/// </summary>

	public override Shader shader
	{
		get
		{
			if (mMat != null) return mMat.shader;
			if (mShader == null) mShader = Shader.Find("Unlit/Transparent Colored");
			return mShader;
		}
		set
		{
			if (mShader != value)
			{
				mShader = value;

				if (mMat == null)
				{
					mPMA = -1;
					MarkAsChanged();
				}
			}
		}
	}
	
	/// <summary>
	/// Texture used by the UITexture. You can set it directly, without the need to specify a material.
	/// </summary>

	public override Texture mainTexture
	{
		get
		{
			if (mSprite != null) return mSprite.texture;
			if (mMat != null) return mMat.mainTexture;
			return null;
		}
	}

	/// <summary>
	/// Whether the texture is using a premultiplied alpha material.
	/// </summary>

	public bool premultipliedAlpha
	{
		get
		{
			if (mPMA == -1)
			{
				Shader sh = shader;
				mPMA = (sh != null && sh.name.Contains("Premultiplied")) ? 1 : 0;
			}
			return (mPMA == 1);
		}
	}

	/// <summary>
	/// Widget's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
	/// This function automatically adds 1 pixel on the edge if the texture's dimensions are not even.
	/// It's used to achieve pixel-perfect sprites even when an odd dimension widget happens to be centered.
	/// </summary>

	Vector4 drawingDimensions
	{
		get
		{
			float left = 0f;
			float bottom = 0f;
			float right = 0f;
			float top = 0f;

			Texture tex = mainTexture;
			Rect rect = (tex != null) ? new Rect(0f, 0f, tex.width, tex.height) : new Rect(0f, 0f, mWidth, mHeight);

			Vector2 pv = pivotOffset;
			int w = Mathf.RoundToInt(rect.width);
			int h = Mathf.RoundToInt(rect.height);

			float paddedW = ((w & 1) == 0) ? w : w + 1;
			float paddedH = ((h & 1) == 0) ? h : h + 1;

			Vector4 v = new Vector4(
				left / paddedW,
				bottom / paddedH,
				(w - right) / paddedW,
				(h - top) / paddedH);

			v.x -= pv.x;
			v.y -= pv.y;
			v.z -= pv.x;
			v.w -= pv.y;

			v.x *= mWidth;
			v.y *= mHeight;
			v.z *= mWidth;
			v.w *= mHeight;

			return v;
		}
	}

	/// <summary>
	/// Texture rectangle.
	/// </summary>

	public Rect uvRect
	{
		get
		{
			Texture tex = mainTexture;

			if (tex != null)
			{
				Rect rect = mSprite.textureRect;

				rect.xMin /= tex.width;
				rect.xMax /= tex.width;
				rect.yMin /= tex.height;
				rect.yMax /= tex.height;

				return rect;
			}
			return new Rect(0f, 0f, 1f, 1f);
		}
	}

	/// <summary>
	/// Adjust the scale of the widget to make it pixel-perfect.
	/// </summary>

	public override void MakePixelPerfect ()
	{
		Texture tex = mainTexture;

		if (tex != null)
		{
			int x = tex.width;
			if ((x & 1) == 1) ++x;

			int y = tex.height;
			if ((y & 1) == 1) ++y;

			width = x;
			height = y;
		}
		base.MakePixelPerfect();
	}

	/// <summary>
	/// Virtual function called by the UIPanel that fills the buffers.
	/// </summary>

	public override void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		Color colF = color;
		colF.a *= mPanel.finalAlpha;
		Color32 col = premultipliedAlpha ? NGUITools.ApplyPMA(colF) : colF;
		Vector4 v = drawingDimensions;
		Rect rect = uvRect;

		verts.Add(new Vector3(v.x, v.y));
		verts.Add(new Vector3(v.x, v.w));
		verts.Add(new Vector3(v.z, v.w));
		verts.Add(new Vector3(v.z, v.y));

		uvs.Add(new Vector2(rect.xMin, rect.yMin));
		uvs.Add(new Vector2(rect.xMin, rect.yMax));
		uvs.Add(new Vector2(rect.xMax, rect.yMax));
		uvs.Add(new Vector2(rect.xMax, rect.yMin));

		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
	}
}
#endif
