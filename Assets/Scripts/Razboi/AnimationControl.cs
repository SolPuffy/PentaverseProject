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
    private void Start()
    {
        //fakeConsole.MoreText($"{name} Animation starting ...");
        StartCoroutine(InfiniteLOOP());
    }

    IEnumerator InfiniteLOOP()
    {
        yield return new WaitForSeconds(ListOfAnimations[IndexOfAnimationToIdle].AnimationDelay + Random.Range(0.3f, 2.7f));
        while(true)
        {
            IdleAnimation();
            yield return new WaitForSeconds(ListOfAnimations[IndexOfAnimationToIdle].AnimationDelay + Random.Range(0.3f, 2.7f));
        }

    }
    private void  IdleAnimation()
    {
        if (ObjectAnimator == null) return;

        //fakeConsole.MoreText($"{name} Trying to start Idle ...");
        ObjectAnimator.Play(ListOfAnimations[IndexOfAnimationToIdle].AnimationClip.name);
        //fakeConsole.MoreText($"{name} Idle done ...");
        //await Task.Delay((int)(ListOfAnimations[IndexOfAnimationToIdle].AnimationDelay + Random.Range(0.3f, 2.7f)) * 1000);
        //IdleAnimation();
    } 

}
