using Unity.MLAgents.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgentInput : MonoBehaviour,IInputActionAssetProvider{

    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;

    [SerializeField] private Transform tankTower;
    [SerializeField] private Transform gun;

    private AgentActions inputActions;
    private AgentMovement movement;

    Rigidbody m_PlayerRb;

    private void Awake() {
        movement = GetComponent<AgentMovement>();
        LazyInitializeAction();
        m_PlayerRb = GetComponent<Rigidbody>();
    }
    void LazyInitializeAction() {
        if (inputActions != null) {
            return;
        }
        inputActions = new AgentActions();
        inputActions.Enable();
    }
    private void OnEnable() {
        inputActions.Tank.Enable();
    }     
    private void OnDisable() {
        inputActions.Tank.Disable();
    }  
    private void FixedUpdate() {
        movement.Move(inputActions.Tank.movement.ReadValue<Vector2>());       
        movement.TankTowerRotation(inputActions.Tank.towerMovement.ReadValue<Vector2>());
    }
    public (InputActionAsset, IInputActionCollection2) GetInputActionAsset() {
        LazyInitializeAction();
        return (inputActions.asset, inputActions);
    }   
}
