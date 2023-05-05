using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    private Vector2 direction; // 공의 이동 방향
    private readonly float speed = 10f; // 속도
    
    // 최초 공의 방향을 결정
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        
        // 게임 시작시 공의 이동 방향은
        // 무작위성이 조금 추가된 왼쪽 방향
        direction = 
            (Vector2.left + Random.insideUnitCircle * 0.1f).normalized;
    }
    
    private void FixedUpdate()
    {
        // 서버가 아니거나 게임이 종료된 경우 이동 처리를 하지 않음
        if (!IsServer || !GameManager.Instance.IsGameActive)
        {
            return;
        }

        // 공의 이동 거리를 계산
        var distance = speed * Time.deltaTime;
        
        // 공의 이동 방향으로 레이캐스트를 통해 충돌 검사
        var hit = Physics2D.Raycast(transform.position, direction, distance);

        // 무언가와 충돌한 경우
        if (hit.collider != null)
        {
            // 충돌한 게임 오브젝트가 스코어 존인 경우
            if (hit.collider.CompareTag("ScoringZone"))
            {
                // 왼쪽 스코어 존인 경우 플레이어 1번에 점수 추가
                if (hit.point.x < 0f)
                {
                    GameManager.Instance.AddScore(1, 1);
                    // 공 위차가 리셋될때
                    // 공을 놓친 플레이어 방향으로 공이 날아감 
                    direction = Vector2.left;
                }
                else
                {
                    // 오른쪽 스코어 존인 경우 플레이어 0번에 점수 추가
                    GameManager.Instance.AddScore(0, 1);
                    direction = Vector2.right;
                }
                
                // 공을 시작 위치로 되돌림
                transform.position = Vector3.zero;
                return;
            }

            // 스코어링 존에 충돌하지 않은 경우
            // 충돌 방향에 반사되는 방향으로 공을 튕김
            direction = Vector2.Reflect(direction, hit.normal);
            // 공의 이동 방향에 랜덤성을 더함
            direction 
                = (direction + Random.insideUnitCircle * 0.1f).normalized;
        }
        // 위치 이동 적용
        transform.position += (Vector3)(direction * distance);
    }
}