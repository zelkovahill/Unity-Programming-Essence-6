using Unity.Netcode;
using UnityEngine;

public class PlayerControl : NetworkBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public float speed = 5f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    [ClientRpc]
    public void SetRendererColorClientRpc(Color color)
    {
        _spriteRenderer.color = color;
    }

    [ClientRpc]
    public void SpawnToPositionClientRpc(Vector3 position)
    {
        transform.position = position;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive)
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
