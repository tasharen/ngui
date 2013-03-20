//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tiled Sprite is obsolete. This script is kept only for backwards compatibility.
/// </summary>

[ExecuteInEditMode]
public class UITiledSprite : UISlicedSprite
{
	override protected void Awake ()
	{
		mType = UISprite.Type.Tiled;
		base.Awake();
	}

	override protected void OnStart () { mType = Type.Tiled; }
}
