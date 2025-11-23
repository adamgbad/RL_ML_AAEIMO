using System;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using UnityEngine;
public class OnDestroyEventArgs : EventArgs {
    public GameObject gameObject;
}
public class OnAccidentDestroyEventArgs : EventArgs {
    public GameObject gameObject;
}
public class TankAgent : Agent {

    private AgentSensors agentSensors;
    private AgentRewards agentRewards;
    private AgentInput inputSystem;    
    private ShootSystem shootSystem;
    private BehaviorParameters behaviors;
    private VectorSensorComponent vectorSensor;
    private RayPerceptionSensorComponent3D rayPer3D;
    private EnvironmentParameters envParameters;
    private int missedShootsSinceLastHit = 0;
    private float secondsBetweenShots = 0;
    private float maxStep = 50000;
    private int id;


    [SerializeField] private BufferSensorComponent bufferSensor;
    [SerializeField] private BufferSensorComponent bufferSensor2;
    [SerializeField] private int health;
    [SerializeField] private int maxHealth;     
    [SerializeField] private Transform shootPosition;
    [SerializeField] private Team team;
    [SerializeField] private Transform top;
    [SerializeField] private Transform bottom;
    [SerializeField] private Transform bulletPrefab;
    [SerializeField] private int maxBullet;
    [SerializeField] private int currentBullet;
    [SerializeField] private Transform middlePoint;

    public event EventHandler<OnDestroyEventArgs> OnDieEvent;
    public event EventHandler<OnAccidentDestroyEventArgs> OnAccidentEvent;

    [Observable(numStackedObservations: 1)] public int MaxHealth { get {  return maxHealth; } }       
    [Observable(numStackedObservations: 1)] public int Health { get { return health; } private set { health = value; } }
    [Observable(numStackedObservations: 1)] public int MaxBullet { get { return maxBullet; } }
    [Observable(numStackedObservations: 1)] public int CurrentBullet { get { return currentBullet; } set { currentBullet = value; } }
    public Team AgentTeam { get { return team; } }
    public int ID { get { return id; } }

    public override void Initialize() {
        vectorSensor = gameObject.GetComponent<VectorSensorComponent>();
        behaviors = gameObject.GetComponent<BehaviorParameters>();
        rayPer3D = gameObject.GetComponent<RayPerceptionSensorComponent3D>();
        agentSensors = gameObject.GetComponent<AgentSensors>();
        inputSystem = gameObject.GetComponent<AgentInput>();
        shootSystem = gameObject.GetComponent<ShootSystem>();
        agentRewards = new AgentRewards();
        id = behaviors.TeamId;

        shootSystem.OnBulletDestroy -= ShootSystem_OnBulletDestroy;
        shootSystem.OnBulletDestroy += ShootSystem_OnBulletDestroy;
    }
    private void ShootSystem_OnBulletDestroy(object sender, EventArgs e) {
        missedShootsSinceLastHit++;
        agentRewards.AddMissedShotPenalty(this, (float)missedShootsSinceLastHit, maxBullet);
    }
    public override void OnEpisodeBegin() {
        SetStartingHealth();
        SetStartingAmmunation();
        SetShoot();
        SetReward(0);
        StartCoroutine(CountSeconds());
    }
    public override void CollectObservations(VectorSensor sensor) {
        agentRewards.AddStepPenalty(this, StepCount, maxStep);

        agentSensors.CollectRayObservations(sensor, shootSystem, this);
        agentSensors.CollectEnviromentObservations(sensor, EnvironmentController.Instance.GetItemsTransform(), EnvironmentController.Instance.GetTeamMate(this), this);
        agentSensors.CollectAgentObservations(sensor, top, this);

        agentSensors.CollectGoalObservations(vectorSensor, rayPer3D);

        agentSensors.CollectBufferSensorsObservation(bufferSensor, bufferSensor2, EnvironmentController.Instance, middlePoint, shootSystem, this);
    }
    public override void OnActionReceived(ActionBuffers actions) {
        int shoot = actions.DiscreteActions[0];
        if (shoot == 1) {
            shootSystem.Shoot(this, ref currentBullet, bulletPrefab);
        }
    }
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        if (!shootSystem.GetShootState() || shootSystem.GetAmmoState()) {
            actionMask.SetActionEnabled(0, 1, false);
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut) {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
    public void TakeDamage(int n) {
        health -= n;
        agentRewards.AddBulletHitPenalty(this, 0.5f);
        if (health <= 0) {
            OnDieEvent?.Invoke(this, new OnDestroyEventArgs { gameObject = gameObject });
        } 
    }
    private void OnCollisionEnter(Collision collision) {
        var other = collision.gameObject;
        if(other != null) {
            if (other.CompareTag("HealObject")) {                              
                agentRewards.AddHPReward(this, MaxHealth, Health);
                Health = other.GetComponent<HealingObject>().SetAgent(Health, MaxHealth);
                OnDieEvent?.Invoke(this, new OnDestroyEventArgs { gameObject = other });
            } else if (other.CompareTag("AmmoObject")) {              
                agentRewards.AddAmmoReward(this, MaxBullet, CurrentBullet);
                CurrentBullet = other.GetComponent<AmmoObject>().SetAgent(CurrentBullet, MaxBullet);
                OnDieEvent?.Invoke(this, new OnDestroyEventArgs { gameObject = other });
            } else if (other.transform.TryGetComponent(out Bullet bullet)) {
                agentRewards.AddBulletHitReward(bullet.GetAgent(), 1);
                Destroy(other);
                TakeDamage(20);
            } else if (other.CompareTag("OutsideWall")) {
                OnAccidentEvent?.Invoke(this, new OnAccidentDestroyEventArgs { gameObject = gameObject });
            }
        }       
    }
    public void SetShoot() {
        shootSystem.DestroyBullets();
        shootSystem.SetShoot();
        SetMissedShootSinceLastHit();
        SetMissedShootSinceLastHit();
        secondsBetweenShots = 0;
    }
    public void SetStartingHealth() {
        Health = maxHealth;
    }
    public void SetStartingAmmunation() {
        currentBullet = maxBullet;
    }
    public void SetMissedShootSinceLastHit() {
        missedShootsSinceLastHit = 0;
    }
    public IEnumerator CountSeconds() {
        while (true) {
            secondsBetweenShots += 1f;
            yield return new WaitForSeconds(1);
        }
    }
}
