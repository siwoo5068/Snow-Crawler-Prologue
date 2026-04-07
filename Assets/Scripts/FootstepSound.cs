using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public AudioSource audioSource;
    public AudioClip[] stepClips; // 여러 개의 발자국 소리를 넣을 공간

    [Header("Step Settings")]
    public float stepInterval = 0.5f; // 발자국 소리가 나는 간격 (초 단위)
    private float stepTimer;

    void Update()
    {
        // 플레이어가 바닥에 닿아있고 && WASD로 움직이고 있을 때만 작동해요
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            stepTimer += Time.deltaTime;

            // 설정한 시간(0.5초)마다 소리를 한 번씩 냅니다
            if (stepTimer >= stepInterval)
            {
                PlayRandomStep();
                stepTimer = 0f; // 타이머 초기화
            }
        }
        else
        {
            // 가만히 서 있을 때는 타이머를 0으로 돌려놔서, 다음 걸음에 바로 소리가 나게 해요
            stepTimer = 0f;
        }
    }

    void PlayRandomStep()
    {
        // 소리 파일이 하나라도 등록되어 있다면
        if (stepClips.Length > 0)
        {
            // 1. 등록된 소리 중 무작위로 하나를 고릅니다
            int randomIndex = Random.Range(0, stepClips.Length);
            audioSource.clip = stepClips[randomIndex];

            // 2. 음높이(Pitch)와 볼륨(Volume)을 살짝씩 흔들어서 매번 다른 소리처럼 속입니다!
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.volume = Random.Range(0.8f, 1.0f);

            // 3. 재생!
            audioSource.Play();
        }
    }
}