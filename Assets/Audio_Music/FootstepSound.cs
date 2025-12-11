using UnityEngine;

public class FootstepSound : StateMachineBehaviour
{
    AudioSource audioSource;
    public AudioClip footstepClip;

    public float interval = 0.4f;   // 발소리 간격
    float timer = 0f;
    bool isMoving = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 컴포넌트 한 번만 가져오기
        if (audioSource == null)
            animator.TryGetComponent(out audioSource);

        isMoving = true;
        timer = 0f;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!isMoving || audioSource == null || footstepClip == null) return;

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            audioSource.PlayOneShot(footstepClip);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        isMoving = false;
        timer = 0f;
    }
}
