using UnityEngine;

public class FollowPlayerUI : MonoBehaviour
{
    [SerializeField] private Transform target; // The player to follow
    [SerializeField] private Vector3 offset = Vector3.zero; // offset above player

    private void LateUpdate()
    {
        if (target != null)
        {
            // Match position with optional offset
            transform.position = target.position + offset;

            // Reset rotation so it always stays horizontal
            transform.rotation = Quaternion.identity;
        }
    }
}
