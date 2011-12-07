using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Very simple UI sprite -- a simple quad of specified size, drawn using a part of the texture atlas.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Basic)")]
public class UISprite : UIWidget
{
	// Cached and saved values
	[SerializeField] UIAtlas mAtlas;
	[SerializeField] string mSpriteName;

	protected UIAtlas.Sprite mSprite;
	protected Rect mOuter;
	protected Rect mOuterUV;

	/// <summary>
	/// Outer set of UV coordinates.
	/// </summary>

	public Rect outerUV { get { UpdateUVs(); return mOuterUV; } }

	/// <summary>
	/// Atlas used by this widget.
	/// </summary>
 
	public UIAtlas atlas
	{
		get
		{
			return mAtlas;
		}
		set
		{
			if (mAtlas != value)
			{
				mAtlas = value;

				// Update the material
				material = (mAtlas != null) ? mAtlas.material : null;

				// Re-link the sprite
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					string sprite = mSpriteName;
					mSpriteName = "";
					spriteName = sprite;
					mChanged = true;
				}
			}
		}
	}

	/// <summary>
	/// Sprite within the atlas used to draw this widget.
	/// </summary>
 
	public string spriteName
	{
		get
		{
			return mSpriteName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				// If the sprite name hasn't been set yet, no need to do anything
				if (string.IsNullOrEmpty(mSpriteName)) return;

				// Clear the sprite name and the sprite reference
				mSpriteName = "";
				mSprite = null;
				mChanged = true;
			}
			else if (string.IsNullOrEmpty(mSpriteName) || !string.Equals(mSpriteName, value,
				System.StringComparison.OrdinalIgnoreCase))
			{
				// If the sprite name changes, the sprite reference should also be updated
				mSpriteName = value;
				mSprite = (mAtlas != null) ? mAtlas.GetSprite(mSpriteName) : null;
				UpdateUVs();
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Update the texture UVs used by the widget.
	/// </summary>

	virtual protected void UpdateUVs()
	{
		Init();

		if (mSprite != null && mOuter != mSprite.outer)
		{
			Texture2D tex = mainTexture;

			if (tex != null)
			{
				mOuter = mSprite.outer;
				mOuterUV = mOuter;

				if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
				{
					mOuterUV = NGUITools.ConvertToTexCoords(mOuterUV, tex.width, tex.height);
				}
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Adjust the scale of the widget to make it pixel-perfect.
	/// </summary>

	override public void MakePixelPerfect ()
	{
		Texture2D tex = mainTexture;

		if (tex != null)
		{
			Rect rect = NGUITools.ConvertToPixels(outerUV, tex.width, tex.height, true);
			Vector3 scale = cachedTransform.localScale;
			scale.x = rect.width;
			scale.y = rect.height;
			scale.z = 1f;
			cachedTransform.localScale = scale;
		}
		base.MakePixelPerfect();
	}

	/// <summary>
	/// Ensure that the sprite has been initialized properly.
	/// This is necessary because the order of execution is unreliable.
	/// Sometimes the sprite's functions may be called prior to Start().
	/// </summary>

	protected void Init ()
	{
		if (mAtlas != null)
		{
			if (material == null) material = mAtlas.material;
			if (mSprite == null) mSprite = string.IsNullOrEmpty(mSpriteName) ? null : mAtlas.GetSprite(mSpriteName);
		}
	}

	/// <summary>
	/// Set the atlas and the sprite.
	/// </summary>

	void Start ()
	{
		if (mAtlas != null)
		{
			UpdateUVs();
		}
	}

	/// <summary>
	/// Update the UV coordinates.
	/// </summary>

	override public bool OnUpdate ()
	{
		if (!mPlayMode) UpdateUVs();
		return false;
	}

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>

	override public void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Vector2 uv0 = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
		Vector2 uv1 = new Vector2(mOuterUV.xMax, mOuterUV.yMax);

		if (centered)
		{
			verts.Add(new Vector3( 0.5f,  0.5f, 0f));
			verts.Add(new Vector3( 0.5f, -0.5f, 0f));
			verts.Add(new Vector3(-0.5f, -0.5f, 0f));
			verts.Add(new Vector3(-0.5f,  0.5f, 0f));
		}
		else
		{
			verts.Add(new Vector3(1f,  0f, 0f));
			verts.Add(new Vector3(1f, -1f, 0f));
			verts.Add(new Vector3(0f, -1f, 0f));
			verts.Add(new Vector3(0f,  0f, 0f));
		}

		uvs.Add(uv1);
		uvs.Add(new Vector2(uv1.x, uv0.y));
		uvs.Add(uv0);
		uvs.Add(new Vector2(uv0.x, uv1.y));

		cols.Add(color);
		cols.Add(color);
		cols.Add(color);
		cols.Add(color);
	}
}