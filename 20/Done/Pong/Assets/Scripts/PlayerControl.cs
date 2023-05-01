using Unity.Netcode;
using UnityEngine;

public class PlayerControl : NetworkBehaviour
{
    // 플레이어의 색상을 변경하기 위한 컴포넌트
    private SpriteRenderer _spriteRenderer;
    public float speed = 5f; // 이동 속도

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 플레이어의 색상을 변경
    [ClientRpc]
    public void SetRendererColorClientRpc(Color color)
    {
        _spriteRenderer.color = color;
    }

    // 플레이어의 위치를 변경
    [ClientRpc]
    public void SpawnToPositionClientRpc(Vector3 position)
    {
        transform.position = position;
    }

    private void Update()
    {
        // 게임 매니저가 존재하지 않거나 게임이 종료된 경우 이동 처리를 하지 않음
        if (GameManager.Instance == null 
            || !GameManager.Instance.IsGameActive)
        {
            return;
        }
        
        // 소유권을 가진 경우에만 이동 처리
        if (!IsOwner)
        {
            return;
        }
        
        var input = Input.GetAxis("Vertical");
        
        var distance = input * speed * Time.deltaTime;
        var position = transform.position;
        position.y += distance;
        position.y = Mathf.Clamp(position.y, -4.5f, 4.5f);
        transform.position = position;
    }
}
