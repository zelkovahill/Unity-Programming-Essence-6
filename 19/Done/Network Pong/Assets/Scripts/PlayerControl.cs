using Unity.Netcode;
using UnityEngine;

public class PlayerControl : NetworkBehaviour
{
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>();
    private SpriteRenderer _spriteRenderer;
    
    public float speed = 5f;

    public override void OnNetworkSpawn()
    {
        playerColor.OnValueChanged += OnPlayerColorChanged;
    }

    private void OnPlayerColorChanged(Color previousvalue, Color newvalue)
    {
        _spriteRenderer.color = newvalue;
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    [ClientRpc]
    public void SetRenderActiveClientRpc(bool active)
    {
        _spriteRenderer.enabled = active;
    }
    
    [ClientRpc]
    public void SpawnToPositionClientRpc(Vector3 position)
    {
        transform.position = position;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive)
        {
            return;
        }
        
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
