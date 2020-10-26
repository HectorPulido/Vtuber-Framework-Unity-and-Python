using UnityEngine;
using System.Collections.Generic;

public class FaceHandler : MonoBehaviour
{
	public string layerName = "Face";
	public string[] emotion = new string[]{
		"Angry",
        "Disgust",
        "Fear",
        "Happy",
        "Sad",
        "Surprised",
        "Neutral"
	};
	public AnimationClip[] animations;
	private Dictionary<string, AnimationClip> emotionAnimationBind;
	private Animator anim;

	private void Start()
	{
		anim = GetComponent<Animator>();
		anim.SetLayerWeight (anim.GetLayerIndex(layerName), 1);

		emotionAnimationBind = new Dictionary<string, AnimationClip>();
		for (int i = 0; i < emotion.Length; i++)
		{
			emotionAnimationBind.Add(emotion[i], animations[i]);
		}
	}
	public void ChangeEmotion(string emotion)
	{
		anim.CrossFade(emotionAnimationBind[emotion].name, 0);
	}
}