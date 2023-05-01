using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    private Vector2 direction; // 공의 이동 방향
    private readonly float speed = 10f; // 속도

    // 공이 스폰될때 방향을 랜덤하게 지정함
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        
        // 기본 왼쪽 방향에 랜덤한 방향을 더함
        direction = Vector2.left + Random.insideUnitCircle;
        // 방향을 정규화
        direction = direction.normalized;
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

        // 충돌한 경우
        if (hit.collider != null)
        {
            // 충돌한 오브젝트가 골대인 경우
            var goalpost = hit.collider.GetComponent<Goalpost>();
            if (goalpost != null)
            {
                // 골대에 공이 들어갔음을 알림
                goalpost.OnGoal();
                // 공을 리스폰
                Respawn();
                return;
            }

            // 골대에 충돌하지 않은 경우
            // 충돌한 표면에서 공을 튕김
            direction = Vector2.Reflect(direction, hit.normal);
            // 공의 이동 방향에 랜덤한 방향을 미세하게 더함
            direction += Random.insideUnitCircle * 0.01f;
            direction = direction.normalized;
        }
        // 위치 이동 적용
        transform.position = (Vector2) transform.position + direction * distance;
    }
    
    // 공을 리스폰
    private void Respawn()
    {
        // 정위치로 되돌림
        transform.position = Vector3.zero;
        // 방향을 랜덤하게 지정
        direction = Random.insideUnitCircle.normalized;
    }
}