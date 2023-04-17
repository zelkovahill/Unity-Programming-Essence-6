using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    private Vector2 direction;
    private readonly float speed = 10f;

    public override void OnNetworkSpawn()
    {
        direction = Vector2.left + Random.insideUnitCircle;
        direction = direction.normalized;
    }
    
    private void FixedUpdate()
    {
        if (!IsServer || !GameManager.Instance.IsGameActive)
        {
            return;
        }

        var distance = speed * Time.deltaTime;
        var hit = Physics2D.Raycast(transform.position, direction, distance);

        if (hit.collider != null)
        {
            var goalpost = hit.collider.GetComponent<Goalpost>();
            if (goalpost)
            {
                goalpost.OnGoal();
                Respawn();
                return;
            }

            direction = Vector2.Reflect(direction, hit.normal);
            direction += Random.insideUnitCircle * 0.01f; // 조금 랜덤하게 튕기게
            direction = direction.normalized;
        }

        transform.position = (Vector2) transform.position + direction * distance;
    }
    
    private void Respawn()
    {
        transform.position = Vector3.zero;
        direction = Random.insideUnitCircle.normalized;
    }
}