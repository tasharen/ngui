using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class is meant to be used only internally. It's like Debug.Log, but prints using OnGUI to screen instead.
/// </summary>

[AddComponentMenu("NGUI/Internal/Debug")]
public class NGUIDebug : MonoBehaviour
{
	static List<string> mLines = new List<string>();
	static NGUIDebug mInstance = null;
	
	static public void Log (string text)
	{
		Debug.Log(text);

		if (mLines.Count > 20) mLines.RemoveAt(0);
		mLines.Add(text);
		
		if (mInstance == null)
		{
			GameObject go = new GameObject("_NGUI Debug");
			mInstance = go.AddComponent<NGUIDebug>();
			DontDestroyOnLoad(go);
		}
	}
	
	void OnGUI()
	{
		foreach (string text in mLines)
		{
			GUILayout.Label(text);
		}
	}
}