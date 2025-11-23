using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentSensors : MonoBehaviour {

    [SerializeField] private Transform midPoint;
    [SerializeField] private float laserLength; 
    [SerializeField] private LayerMask laserLayerMask;
    [SerializeField] private LayerMask laserLayerMask2;
    [SerializeField] private Transform shootPosition;

    private int reflectionMaxCount = 5;
    private int reflectionCurrentCount;

    private RaycastHit currentHit;
    private RaycastHit currentBulletHit;
    private RaycastHit currentTeamMateHit;

    private Team hitAgentTeam;
    private Vector3 currentRayDirection;
    private Vector3 currentBulletRayDirection;
    public float LaserLength { get { return laserLength; } set { laserLength = value; }}
    public RaycastHit CurrentHit { get { return currentHit; } }
    public RaycastHit CurrentBulletHit { get { return currentBulletHit; } }
    public Vector3 CurrentRayDirection { get { return currentRayDirection; } }
    public Vector3 CurrentBulletRayDirection { get { return currentBulletRayDirection; } }
    public Team HitAgentTeam { get { return hitAgentTeam; } }

    private void FixedUpdate() { 
        CreateRay(shootPosition);
    }
    public void CollectEnviromentObservations(VectorSensor sensor, List<Transform> itemsTransforms, TankAgent teamMate, TankAgent agent) {
        sensor.AddObservation(GetNormalizedTeamMateDistance(teamMate, agent));
        sensor.AddObservation(GetNormalizedClosestItemDistance(itemsTransforms));
    }
    public void CollectRayObservations(VectorSensor sensor, ShootSystem shootSystem, TankAgent agent) {
        sensor.AddObservation(GetNormalizedDistanceBetweenAgents(CurrentHit, transform, agent));      
    }
    public void CollectAgentObservations(VectorSensor sensor, Transform top, TankAgent agent) {       
        sensor.AddObservation(GetBodyPartRotation(top));
    }
    public void CollectBufferSensorsObservation(BufferSensorComponent bufferSensor1, BufferSensorComponent bufferSensor2, EnvironmentController environment, Transform middle,ShootSystem shootSystem, TankAgent agent) {
        foreach (TankAgent item in environment.GetRemainedAgents(agent)) {
            if (item != this) {

                float[] agentInfo = new float[3];

                agentInfo[0] = NormalizeBetweenOneZero(item.Health, 0, item.MaxHealth);
                agentInfo[1] = NormalizeBetweenOneZero(item.CurrentBullet, 0, item.MaxBullet);
                agentInfo[2] = item.ID;

                bufferSensor1.AppendObservation(agentInfo);
            }
        }
        if (shootSystem.GetBulletsTransform() != null && shootSystem.GetBulletsTransform().Count != 0) {
            for (int i = 0; i < shootSystem.GetBulletsTransform().Count; i++) {
                float[] bulletInfo = new float[2];
                bulletInfo[0] = i + 1;
                bulletInfo[1] = (Vector3.Distance(shootSystem.GetBulletsTransform()[i].position, transform.position) - 0) / (80 - 0);

                bufferSensor2.AppendObservation(bulletInfo);
            }
        }
    }
    public void CollectGoalObservations(VectorSensorComponent vectorSensor, RayPerceptionSensorComponent3D rayPer3D) {        
        vectorSensor.GetSensor().AddOneHotObservation((int)hitAgentTeam, 3);
        vectorSensor.GetSensor().AddOneHotObservation((int)RayPer3DHitAgent(rayPer3D), 3);
    }    
    public void CreateRay(Transform t) {     
        if(t == null) {
            return;
        }

        Vector3 pos = t.position;
        Vector3 dir = t.forward;
        float remainingLength = laserLength;
        reflectionCurrentCount = 0;

        for (int i = 0; i < reflectionMaxCount; i++) {
            Ray ray = new Ray(pos, dir);
            RaycastHit hit;           
            if (Physics.Raycast(ray, out hit, remainingLength, laserLayerMask)) {
                pos = hit.point;
                dir = Vector3.Reflect(dir, hit.normal);
                remainingLength -= hit.distance;
                currentHit = hit;
                reflectionCurrentCount++;                        
                if (IsRayHitAgent(currentHit, t)) {
                    currentRayDirection = ray.origin - hit.point;
                    return;
                }
            } else {
                hitAgentTeam = Team.None;
                currentHit = hit;
            }
        }
    }
    private bool IsRayHitAgent(RaycastHit hit, Transform t) {
        if (hit.transform != null && t != null) {
            if (hit.transform.TryGetComponent(out TankAgent hitAgent)) {
                hitAgentTeam = hitAgent.AgentTeam;
                return true;
            }
        }
        return false;
    }
    public float GetNormalizedDistanceBetweenAgents(RaycastHit hit, Transform t, TankAgent agent) {
        if (IsRayHitAgent(hit, t)) {        
            float distance = Vector3.Distance(hit.transform.position, agent.transform.position);
            return NormalizeBetweenOneZero(distance, 0, 80);
        }
        return 0;
    }
    private float GetNormalizedClosestItemDistance(List<Transform> itemsSpawned) {  
        if (itemsSpawned.Count != 0) {
            float distance = Vector3.Distance(itemsSpawned[0].transform.position, gameObject.transform.position);

            for (int i = 1; i < itemsSpawned.Count; i++) {
                float currentDistance = Vector3.Distance(itemsSpawned[i].transform.position, gameObject.transform.position);
                if (currentDistance < distance) {
                    distance = currentDistance;
                }
            }
            return NormalizeBetweenOneZero(distance, 0, 80);
        }
        return 0;
    } 
    private Quaternion GetBodyPartRotation(Transform bodyPart) {
        return bodyPart.rotation;
    }
    private Team RayPer3DHitAgent(RayPerceptionSensorComponent3D rayPer3D) {
        
        var rayOut = rayPer3D.RaySensor.RayPerceptionOutput.RayOutputs;

        for (int i = 0; i < rayOut.Length; i++) {        
            GameObject hit = rayOut[i].HitGameObject;
            if (hit != null) {
                if (hit.gameObject.GetComponent<TankAgent>() != null) {               
                    return hit.gameObject.GetComponent<TankAgent>().AgentTeam;
                }
            }
        }
        return Team.None;
    }
    private float GetNormalizedTeamMateDistance(TankAgent teamMate, TankAgent agent) {
        if (teamMate != null) {
            float distance = Vector3.Distance(agent.transform.position, teamMate.transform.position);
            return NormalizeBetweenOneZero(distance, 0, 80);
        }
        return 0;
    }
    private float NormalizeBetweenOneZero(float value, float min, float max) {
        return ((value - min) / (max - min));
    }
}
