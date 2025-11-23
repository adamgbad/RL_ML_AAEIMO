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

    private void Awake() {
        movement = GetComponent<AgentMovement>();
        InitializeAction();
    }
    void InitializeAction() {
        if (inputActions != null) {
            return;
        }
        inputActions = new AgentActions();
        inputActions.Enable();
    }
    private void FixedUpdate() {
        movement.Move(inputActions.Tank.movement.ReadValue<Vector2>());       
        movement.TankTowerRotation(inputActions.Tank.towerMovement.ReadValue<Vector2>());
    }
    public (InputActionAsset, IInputActionCollection2) GetInputActionAsset() {
        InitializeAction();
        return (inputActions.asset, inputActions);
    }   
}
