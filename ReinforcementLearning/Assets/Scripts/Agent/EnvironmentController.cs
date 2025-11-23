using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;


public class EnvironmentController : MonoBehaviour {

    public static EnvironmentController Instance;

    [SerializeField] private int maxTime;

    private int currentTime;
    private int currentUpdate = 0;
    public LayerMask spawnZoneLayerMask;
    public List<Transform> spawnZones;

    public List<TankAgent> agentsList = new List<TankAgent>();

    [SerializeField] private List<Transform> remainedSpawnZones = new List<Transform>();
    [SerializeField] private List<Transform> items;
    [SerializeField] private List<Transform> itemSpawned;    

    private AgentRewards agentRewards;
    private SimpleMultiAgentGroup blueAgentGroup;
    private SimpleMultiAgentGroup redAgentGroup;
    private void Awake() {
        if(Instance == null) {
            Instance = this;
        }
        agentRewards = new AgentRewards();
        blueAgentGroup = new SimpleMultiAgentGroup();
        redAgentGroup = new SimpleMultiAgentGroup();
        SubAgentsEvent();
        ResetScene();
    }
    private void ResetScene() {
        currentTime = 0;
        DestroyAllItem();
        SetAgentsToInActive();
        SetSpawns();

        foreach (var item in agentsList) {
            if (item.AgentTeam == Team.Blue) {
                blueAgentGroup.RegisterAgent(item);
            } else {
                redAgentGroup.RegisterAgent(item);
            }
        }    
    }
    
    private void FixedUpdate() {
        currentTime += 1;
        currentUpdate += 1;
        if (currentTime >= maxTime) {
            EpisodeInterrupted();
        }
        if(currentUpdate == 50) {
            CalculateTeamMateDistance(blueAgentGroup, agentsList[0], agentsList[1]);
            CalculateTeamMateDistance(redAgentGroup, agentsList[2], agentsList[3]);
            currentUpdate = 0;
        }           
    }
    private void CalculateTeamMateDistance(SimpleMultiAgentGroup group, TankAgent agent1, TankAgent agent2) { 
        if (group.GetRegisteredAgents().Count() == 2) {
            float maxDistance = 20;
            float teamMateDistance = Vector3.Distance(agent1.transform.position, agent2.transform.position);
            agentRewards.AddTeamMateDistancePenalty(group, teamMateDistance, maxDistance);
        } 
    }
    private void SetSpawnZones() {
        remainedSpawnZones.Clear();
        foreach (var item in spawnZones) {
            remainedSpawnZones.Add(item);
        }
    }
    
    private void SetAgentsToInActive() {
        foreach (var agent in agentsList) {
            agent.gameObject.SetActive(false);
        }
    }
    private void SubAgentsEvent() {
        foreach (var agent in agentsList) {
            agent.OnAccidentEvent += Agent_OnAccidentEvent;
            agent.OnDieEvent += Agent_OnDieEvent;
        }
    }
    private void Agent_OnAccidentEvent(object sender, OnAccidentDestroyEventArgs e) {
        CheckAccident(e.gameObject);
    } 
    private void Agent_OnDieEvent(object sender, OnDestroyEventArgs e) {
        CheckWhichObjToDelete(e.gameObject);
    }
    private void SetSpawns() {      
        SetSpawnZones();

        Vector3 offset = new Vector3(3.5f, 0f, 0f);
        var pos = new Vector3(0, 1f, 0);

        var randomBlueTeamZoneIndex = UnityEngine.Random.Range(0, remainedSpawnZones.Count);
        var blueZone = remainedSpawnZones[randomBlueTeamZoneIndex];
        remainedSpawnZones.RemoveAt(randomBlueTeamZoneIndex);

        var randomRedTeamZoneIndex = UnityEngine.Random.Range(0, remainedSpawnZones.Count);      
        var redZone = remainedSpawnZones[randomRedTeamZoneIndex];
        remainedSpawnZones.RemoveAt(randomRedTeamZoneIndex);

        for (int i = 0; i < agentsList.Count; i++) {
            agentsList[i].gameObject.SetActive(true);
            agentsList[i].SetStartingHealth();
            agentsList[i].SetStartingAmmunation();

            if (agentsList[i].AgentTeam == Team.Blue) {
                var randomRot = Quaternion.Euler(0, UnityEngine.Random.Range(-180, 180), 0);
                agentsList[i].transform.position = blueZone.position + (offset * i) + pos;               
                agentsList[i].transform.rotation = randomRot;
            } else {
                var randomRot = Quaternion.Euler(0, UnityEngine.Random.Range(-180, 180), 0);
                agentsList[i].transform.position = redZone.position + (offset * (i % 2)) + pos;               
                agentsList[i].transform.rotation = randomRot;
            }
        }

        SpawnItem();
        remainedSpawnZones.Clear();
    }
    private void SpawnItem() {

        int randomItemIndex = UnityEngine.Random.Range(0, items.Count);
        Transform randomItem = items[randomItemIndex];

        if(GetZonesAvailable().Count != 0) {
            
            int randomSpawnZone = UnityEngine.Random.Range(0, GetZonesAvailable().Count);
            Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-4f, 4f), 1.5f, UnityEngine.Random.Range(-4f, 4f));

            Transform currentSpawnZone = GetZonesAvailable()[randomSpawnZone];
            Transform instantiateTransform = Instantiate(randomItem, currentSpawnZone.position + randomPos, Quaternion.identity, transform);

            itemSpawned.Add(instantiateTransform);

            StartCoroutine(SpawnItemCooldown());
        } else {
            StartCoroutine(SpawnItemCooldown());
        }
    }
    IEnumerator SpawnItemCooldown() {
        yield return new WaitForSeconds(10);
        SpawnItem();
    }
    public List<Transform> GetItemsTransform() {
        List<Transform> items = new List<Transform>();
        foreach (var item in itemSpawned) {
            items.Add(item);
        }
        return items;
    }

    public List<TankAgent> GetRemainedAgents(TankAgent agent) {
        List<TankAgent> agents = new List<TankAgent>();
        foreach (var item in redAgentGroup.GetRegisteredAgents()) {
            if(agent != item) {
                agents.Add(item as TankAgent);
            }      
        }
        foreach (var item in blueAgentGroup.GetRegisteredAgents()) {
            if (agent != item) {
                agents.Add(item as TankAgent);
            }
        }       
        return agents;
    }

    private List<Transform> GetZonesAvailable() {
        List<Transform> availableZones = new List<Transform>();
        foreach (var item in spawnZones) {
            availableZones.Add(item);
        }      
        foreach (var agent in redAgentGroup.GetRegisteredAgents()) {
            Ray ray = new Ray(agent.transform.position, -agent.transform.up);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 5, spawnZoneLayerMask)) {
                availableZones.Remove(hit.transform);
            }
        }
        foreach (var agent in blueAgentGroup.GetRegisteredAgents()) {
            Ray ray = new Ray(agent.transform.position, -agent.transform.up);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 5, spawnZoneLayerMask)) {
                availableZones.Remove(hit.transform);
            }
        }
        foreach (var item in itemSpawned) {
            if (item != null) {
                Ray ray = new Ray(item.transform.position, -item.transform.up);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 5, spawnZoneLayerMask)) {
                    availableZones.Remove(hit.transform);
                }
            }               
        }      
        return availableZones;
    }
    public TankAgent GetTeamMate(TankAgent currentAgent) {
        if(currentAgent.ID == blueAgentGroup.GetId()) {
            foreach (var item in blueAgentGroup.GetRegisteredAgents()) {
                if (item != currentAgent) {
                    return item as TankAgent;
                }
            }
        } else {
            foreach (var item in redAgentGroup.GetRegisteredAgents()) {
                if (item != currentAgent) {
                    return item as TankAgent;
                }
            }
        }
        return null;
    }
    private void TryToEndEpisodes() {
        if (blueAgentGroup.GetRegisteredAgents().Count == 0) {
            agentRewards.AddNegativPositivRewards(redAgentGroup, blueAgentGroup, 1);
            EndEpisode();
        } else if(redAgentGroup.GetRegisteredAgents().Count == 0) {
            agentRewards.AddNegativPositivRewards(blueAgentGroup, redAgentGroup, 1);
            EndEpisode();           
        }           
    }
    private void EndEpisode() {
        blueAgentGroup.EndGroupEpisode();
        redAgentGroup.EndGroupEpisode();
        ResetScene();
    }  
    private void EpisodeInterrupted() {
        blueAgentGroup.GroupEpisodeInterrupted();
        redAgentGroup.GroupEpisodeInterrupted();
        ResetScene();
    }
    private void CheckWhichObjToDelete(GameObject gameObjectToDelete) {
        if(gameObjectToDelete.TryGetComponent(out TankAgent agent)) {
            if (blueAgentGroup.GetRegisteredAgents().Contains(agent)) {
                agentRewards.AddNegativPositivRewards(redAgentGroup, blueAgentGroup, 1);
            } else if (redAgentGroup.GetRegisteredAgents().Contains(agent)) {
                agentRewards.AddNegativPositivRewards(blueAgentGroup, redAgentGroup, 1);
            }      
            agent.gameObject.SetActive(false);
            TryToEndEpisodes();          
        } else {
            itemSpawned.Remove(gameObjectToDelete.transform);
            Destroy(gameObjectToDelete);
        }
    }
    private void CheckAccident(GameObject gameObjectToDelete) {
        if (gameObjectToDelete.TryGetComponent(out TankAgent agent)) {
            if (blueAgentGroup.GetRegisteredAgents().Contains(agent)) {
                blueAgentGroup.AddGroupReward(-1);
            } else if (redAgentGroup.GetRegisteredAgents().Contains(agent)) {
                redAgentGroup.AddGroupReward(-1);
            }
            agent.gameObject.SetActive(false);
            TryToEndEpisodes();
        } 
    }
    private void DestroyAllItem() {
        for (int i = itemSpawned.Count - 1; i >= 0; i--) {
            var objToDelete = itemSpawned[i];
            itemSpawned.Remove(itemSpawned[i]);
            Destroy(objToDelete.gameObject);
        }
    }
}
