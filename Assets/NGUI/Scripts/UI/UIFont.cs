using UnityEngine;
using System.Collections.Generic;
using System.IO;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Font")]
public class UIFont : MonoBehaviour
{
	public TextAsset fontData;
	public Texture2D fontTexture;

	BMFont mFont = new BMFont();

	/// <summary>
	/// Load the font on startup.
	/// </summary>

	void Awake () { Reload(); }

	/// <summary>
	/// Reload the font data.
	/// </summary>

	public void Reload ()
	{
		mFont.Load(Tools.GetHierarchy(gameObject), (fontData != null) ? fontData.bytes : null);
	}
}