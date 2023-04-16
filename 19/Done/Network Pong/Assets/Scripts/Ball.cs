using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    private Vector2 direction = Vector2.right;
    private readonly float speed = 10f;
    private readonly float randomRefectionIntensity = 0.1f;
    
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
            direction += Random.insideUnitCircle * randomRefectionIntensity;
            direction = direction.normalized;
        }

        transform.position = (Vector2) transform.position + direction * distance;
    }
    
    private void Respawn()
    {
        transform.position = Vector3.zero;
        direction = Random.onUnitSphere;
    }
}