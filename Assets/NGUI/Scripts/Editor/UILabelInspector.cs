using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UILabel))]
public class UILabelInspector : UIWidgetInspector
{
	UILabel mLabel;
	UIFont mFont;

	override protected bool OnCustomStart ()
	{
		mLabel = mWidget as UILabel;
		mFont = EditorGUILayout.ObjectField("Font", mLabel.font, typeof(UIFont), true) as UIFont;
		if (mFont == null) return false;
		GUITools.DrawSeparator();
		return true;
	}

	override protected void OnCustomEnd ()
	{
		Texture2D tex = mLabel.mainTexture;

		if (tex != null)
		{
			// Draw the atlas
			EditorGUILayout.Separator();
			Rect rect = GUITools.DrawAtlas(tex);

			// Sprite size label
			EditorGUI.DropShadowLabel(new Rect(rect.xMin, rect.yMax, rect.width, 18f), "Font Size: " + mFont.size);
			GUILayout.Space(22f);
		}

		GUITools.DrawSeparator();
		string text = EditorGUILayout.TextField("Text", mLabel.text);

		if (!string.Equals(text, mLabel.text))
		{
			RegisterUndo();
			mLabel.text = text;
		}
	}

	override protected void OnCustomSave ()
	{
		mLabel.font = mFont;
	}
}