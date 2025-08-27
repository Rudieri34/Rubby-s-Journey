using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform _player; // Reference to the player's transform
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(_player.position.x, transform.position.y, _player.position.z);
    }
}
