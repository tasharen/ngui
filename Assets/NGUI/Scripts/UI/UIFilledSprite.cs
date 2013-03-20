//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Filled Sprite is obsolete. This script is kept only for backwards compatibility.
/// </summary>

[ExecuteInEditMode]
public class UIFilledSprite : UISprite
{
	override protected void Awake ()
	{
		mType = UISprite.Type.Filled;
		base.Awake();
	}

	override protected void OnStart () { mType = Type.Filled; }
}
