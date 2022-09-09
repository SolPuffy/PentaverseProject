using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;

[System.Serializable]
public class AnimList
{
    public AnimationClip AnimationClip;
    public float AnimationDelay;
}
public class AnimationControl : MonoBehaviour
{
    public Animator ObjectAnimator;
    public bool DoIdleAnimation = true;
    public int IndexOfAnimationToIdle;
    public List<AnimList> ListOfAnimations = new List<AnimList>();
    private async void Start()
    {

        if(DoIdleAnimation)
        {
            await Task.Delay((int)(ListOfAnimations[IndexOfAnimationToIdle].AnimationDelay + Random.Range(0.3f, 2.7f)) * 1000);
            await IdleAnimation();
        }
    }

    private async Task IdleAnimation()
    {
        ObjectAnimator.Play(ListOfAnimations[IndexOfAnimationToIdle].AnimationClip.name);
        await Task.Delay((int)(ListOfAnimations[IndexOfAnimationToIdle].AnimationDelay + Random.Range(0.3f, 2.7f)) * 1000);
        await IdleAnimation();
    }
    public void PlayAnimationOfIndex(int Index)
    {
        ObjectAnimator.Play(ListOfAnimations[Index].AnimationClip.name);
    }

}
