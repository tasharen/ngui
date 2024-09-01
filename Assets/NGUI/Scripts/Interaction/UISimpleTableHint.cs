//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;

/// <summary>
/// When attached to an object, it will act as a hint to the UISimpleTable as to what it should do with it.
/// </summary>

public class UISimpleTableHint : MonoBehaviour
{
	public bool startNewColumn = false;
	public bool startNewRow = false;
	public bool ignoreWidth = false;
}
