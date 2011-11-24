using UnityEngine;

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
}