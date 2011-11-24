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

	public Rect outerUV { get { return mOuterUV; } }

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
				material = mAtlas.material;

				// Re-link the sprite
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					string sprite = mSpriteName;
					mSpriteName = "";
					spriteName = sprite;
					mIsDirty = true;
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
				mIsDirty = true;
				mSpriteName = "";
				mSprite = null;
			}
			else if (string.IsNullOrEmpty(mSpriteName) || !string.Equals(mSpriteName, value,
				System.StringComparison.OrdinalIgnoreCase))
			{
				// If the sprite name changes, the sprite reference should also be updated
				mIsDirty = true;
				mSpriteName = value;
				mSprite = (mAtlas != null) ? mAtlas.GetSprite(mSpriteName) : null;
				UpdateUVs();
			}
		}
	}

	/// <summary>
	/// Update the texture UVs used by the widget.
	/// </summary>

	virtual protected void UpdateUVs ()
	{
		if (mSprite != null && mOuter != mSprite.outer)
		{
			Texture2D tex = mainTexture;

			if (tex != null)
			{
				mIsDirty = true;
				mOuter = mSprite.outer;
				mOuterUV = mOuter;

				if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
				{
					mOuterUV = UIAtlas.ConvertToTexCoords(mOuterUV, tex.width, tex.height);
				}
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
			Rect rect = UIAtlas.ConvertToPixels(outerUV, tex.width, tex.height);
			Vector3 scale = cachedTransform.localScale;
			scale.x = rect.width;
			scale.y = rect.height;
			scale.z = 1f;
			cachedTransform.localScale = scale;
		}
		base.MakePixelPerfect();
	}

	/// <summary>
	/// Virtual Awake functionality.
	/// </summary>

	override protected void OnAwake ()
	{
		// Re-assign the atlas, which will set the material and texture references.
		UIAtlas atl = mAtlas;
		mAtlas = null;
		atlas = atl;
	}

	/// <summary>
	/// Update the UV coordinates.
	/// </summary>

	override public bool OnScreenUpdate ()
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
			verts.Add(new Vector3(0.5f, 0.5f, 0f));
			verts.Add(new Vector3(0.5f, -0.5f, 0f));
			verts.Add(new Vector3(-0.5f, -0.5f, 0f));
			verts.Add(new Vector3(-0.5f, 0.5f, 0f));
		}
		else
		{
			verts.Add(new Vector3(1f, 0f, 0f));
			verts.Add(new Vector3(1f, -1f, 0f));
			verts.Add(new Vector3(0f, -1f, 0f));
			verts.Add(new Vector3(0f, 0f, 0f));
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