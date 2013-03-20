//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sliced sprite is obsolete. It's only kept for backwards compatibility.
/// </summary>

[ExecuteInEditMode]
public class UISlicedSprite : UISprite
{
	override protected void Awake ()
	{
		mType = UISprite.Type.Sliced;
		base.Awake();
	}

	override protected void OnStart () { mType = Type.Sliced; }
}
