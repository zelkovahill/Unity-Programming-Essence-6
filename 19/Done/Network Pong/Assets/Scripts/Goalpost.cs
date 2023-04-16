using Unity.Netcode;
using UnityEngine;

public class Goalpost : MonoBehaviour
{
    public ulong opponentId;
    
    // Invoke when goal
    public void OnGoal()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            return;
        }
        
        GameManager.Instance.AddScore(opponentId, 1);
    }
}
