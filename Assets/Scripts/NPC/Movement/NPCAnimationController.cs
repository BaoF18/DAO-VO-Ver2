// File: NPCAnimationController.cs
// Mô tả: Đồng bộ hóa animation NPC theo trạng thái di chuyển bằng Blend Tree.
// Parameter "State": 0 = Idle, 1 = Walk. Blend Tree tự blend giữa 2 animation.
// Sử dụng Animator parameter hash để tối ưu hiệu năng.

using UnityEngine;

public class NPCAnimationController : MonoBehaviour
{
    // Blend Tree parameter: 0 = Idle, 1 = Walk
    private static readonly int StateHash = Animator.StringToHash("State");
    private static readonly int TurnLeftHash = Animator.StringToHash("IsLeft");
    private static readonly int TurnRightHash = Animator.StringToHash("IsRight");
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int LeaveInHash = Animator.StringToHash("LeaveIn");
    private static readonly int LeaveOutHash = Animator.StringToHash("LeaveOut");

    private Animator animator;
    private Coroutine turnResetRoutine;

    [Header("=== CẤU HÌNH ANIMATION ===")]
    [Tooltip("Thời gian chuyển tiếp mượt giữa các animation (giây)")]
    [SerializeField] private float dampTime = 0.15f;

    void Awake()
    {
        // Tìm Animator trong object hoặc con (hỗ trợ model có Animator ở child)
        animator = GetComponentInChildren<Animator>();

        // Tắt Root Motion để NavMeshAgent kiểm soát hoàn toàn vị trí NPC.
        // Nếu Root Motion bật, Animator sẽ ghi đè position mỗi frame → NPC không di chuyển theo waypoint.
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    /// <summary>
    /// Cập nhật animation mỗi frame - gọi bởi NPCMovement.
    /// state: 0 = Idle, 1 = Walk
    /// </summary>
    public void UpdateAnimation(float state)
    {
        if (animator == null || !HasFloatParameter(StateHash)) return;
        animator.SetFloat(StateHash, state, dampTime, Time.deltaTime);
    }

    /// <summary>
    /// Cập nhật trạng thái rẽ trái/phải
    /// </summary>
    public void UpdateTurn(bool isTurnLeft, bool isTurnRight, float resetDelaySeconds)
    {
        if (animator == null) return;
        if (!HasTurnParameters()) return;
        if (turnResetRoutine != null)
        {
            StopCoroutine(turnResetRoutine);
            turnResetRoutine = null;
        }

        animator.SetBool(TurnLeftHash, isTurnLeft);
        animator.SetBool(TurnRightHash, isTurnRight);

        if (isTurnLeft || isTurnRight)
        {
            turnResetRoutine = StartCoroutine(ResetTurnBools(resetDelaySeconds));
        }
    }

    public void SetIdle(bool isIdle)
    {
        if (animator == null) return;
        if (!HasIdleParameter()) return;
        animator.SetBool(IdleHash, isIdle);
    }

    public void PlayLeaveIn()
    {
        if (animator == null) return;
        if (!HasTriggerParameter(LeaveInHash)) return;
        animator.SetTrigger(LeaveInHash);
    }

    public void PlayLeaveOut()
    {
        if (animator == null) return;
        if (!HasTriggerParameter(LeaveOutHash)) return;
        animator.SetTrigger(LeaveOutHash);
    }

    private bool HasTurnParameters()
    {
        foreach (var parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                if (parameter.nameHash == TurnLeftHash || parameter.nameHash == TurnRightHash)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasIdleParameter()
    {
        foreach (var parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool && parameter.nameHash == IdleHash)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasTriggerParameter(int parameterHash)
    {
        foreach (var parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.nameHash == parameterHash)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasFloatParameter(int parameterHash)
    {
        foreach (var parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Float && parameter.nameHash == parameterHash)
            {
                return true;
            }
        }

        return false;
    }

    private System.Collections.IEnumerator ResetTurnBools(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        animator.SetBool(TurnLeftHash, false);
        animator.SetBool(TurnRightHash, false);
        turnResetRoutine = null;
    }

    /// <summary>
    /// Kích hoạt trigger animation đặc biệt (vẫy tay, ngồi xuống...)
    /// </summary>
    public void PlayTrigger(string triggerName)
    {
        if (animator == null) return;
        animator.SetTrigger(triggerName);
    }

    /// <summary>
    /// Kiểm tra có Animator hay không
    /// </summary>
    public bool HasAnimator()
    {
        return animator != null;
    }
}
