using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;

[System.Serializable]
public class AnimList
{
    public string AnimationName;
    public float AnimationDelay;
}
public class AnimationControl : MonoBehaviour
{
    public Animator ObjectAnimator;
    public bool DoIdleAnimation;
    public int IndexOfAnimationToIdle;
    public List<AnimList> ListOfAnimations = new List<AnimList>();

    private async void Update()
    {
        if (DoIdleAnimation)
        {
            await IdleAnimation();
        }
    }
    private async Task IdleAnimation()
    {
        ObjectAnimator.Play(ListOfAnimations[IndexOfAnimationToIdle].AnimationName);
        await Task.Delay((int)ListOfAnimations[IndexOfAnimationToIdle].AnimationDelay * 1000);
    }
    public void PlayAnimationOfIndex(int Index)
    {
        ObjectAnimator.Play(ListOfAnimations[Index].AnimationName);
    }
}
