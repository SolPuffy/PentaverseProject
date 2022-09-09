using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;

[System.Serializable]
public class AnimList
{
    public AnimationClip AnimationName;
    public float AnimationDelay;
}
public class AnimationControl : MonoBehaviour
{
    public Animator ObjectAnimator;
    public bool DoIdleAnimation;
    private bool DoAnimation;
    public int IndexOfAnimationToIdle;
    public List<AnimList> ListOfAnimations = new List<AnimList>();

    private async void Start()
    {
       
        if (DoIdleAnimation)
        {
            await StartDelay();
        }
    }
    private async void Update()
    {
        if (DoAnimation)
        {
            await IdleAnimation();
        }
    }

    private async Task StartDelay()
    {
        await Task.Delay((int)(Random.Range(0.3f, 2.7f) * 1000));
        DoAnimation = true;
    }
    private async Task IdleAnimation()
    {
        ObjectAnimator.Play(ListOfAnimations[IndexOfAnimationToIdle].AnimationName.name);
        await Task.Delay((int)(ListOfAnimations[IndexOfAnimationToIdle].AnimationDelay + Random.Range(0.5f,2.5f))  * 1000);
    }
    public void PlayAnimationOfIndex(int Index)
    {
        ObjectAnimator.Play(ListOfAnimations[Index].AnimationName.name);
    }

}
