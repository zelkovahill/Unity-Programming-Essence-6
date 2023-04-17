using Unity.Netcode;
using UnityEngine;

public class Goalpost : MonoBehaviour
{
    public ulong opponentId;
    
    public void OnGoal()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            return;
        }
        
        GameManager.Instance.AddScore(opponentId, 1);
    }
}
