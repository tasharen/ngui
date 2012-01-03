using UnityEngine;

/// <summary>
/// Change the shader level-of-detail to match the quality settings.
/// Also allows changing of the quality level from within the editor without having
/// to open up the quality preferences, seeing the results right away.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Examples/Editor Quality Settings")]
public class EditorQualitySettings : MonoBehaviour
{
	public QualityLevel qualityLevel = QualityLevel.Fantastic;

	QualityLevel mStartLevel = QualityLevel.Fantastic;
	bool mRestore = false;

	void Start ()
	{
		mRestore = Application.isPlaying;
		mStartLevel = qualityLevel;
	}

	void OnDestroy ()
	{
		if (mRestore)
		{
			qualityLevel = mStartLevel;
			Update();
		}
	}

	void Update ()
	{
		if (Application.isPlaying)
		{
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				if (Input.GetKeyDown(KeyCode.Minus))
				{
					qualityLevel = (QualityLevel)Mathf.Clamp((int)qualityLevel - 1, 0, 5);
				}
				else if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus))
				{
					qualityLevel = (QualityLevel)Mathf.Clamp((int)qualityLevel + 1, 0, 5);
				}
			}
		}

		if (qualityLevel != QualitySettings.currentLevel)
		{
			QualitySettings.currentLevel = qualityLevel;
			Shader.globalMaximumLOD = ((int)qualityLevel + 1) * 100;
		}
	}
}
