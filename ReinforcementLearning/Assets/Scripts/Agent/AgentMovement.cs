using UnityEngine;

public class AgentMovement: MonoBehaviour
{
    private TankAgent agent;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;

    [SerializeField] private Transform tankTower;
    [SerializeField] private Transform gun;

    public void Move(Vector2 v) {

        float x = v.x;
        float y = v.y;

        Vector3 moveDir = transform.forward * y * moveSpeed * Time.deltaTime;
        
        transform.position += moveDir;

        float rotation = x * rotateSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);
    }
    public void TankTowerRotation(Vector2 v) {
        float rotation = v.x * rotateSpeed * Time.deltaTime;
       tankTower.Rotate(Vector3.up, rotation);
    }
}
