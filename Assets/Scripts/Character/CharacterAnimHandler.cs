using UnityEngine;

public class CharacterAnimHandler : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetMoveState(bool isMoving)
    {
        // 캐릭터마다 파라미터 이름이 다를 수도 있으므로 여기서 공통화하거나 각 프리팹별로 파라미터 이름 다르게 지정 가능
        animator.SetBool("isMove", isMoving);
    }
}
