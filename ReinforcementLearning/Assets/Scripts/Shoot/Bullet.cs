using System;
using System.Collections;
using UnityEngine;
public class Bullet : MonoBehaviour { 

    Rigidbody rb;

    private Vector3 shootDir;
    private float moveSpeed = 15f;
    public float destroyDelay = 5f;
    private TankAgent agent;

    public event EventHandler OnBulletDestroyAction;
    public void Setup(TankAgent agent, Vector3 shootDir) {
        rb = gameObject.GetComponent<Rigidbody>();
        this.shootDir = shootDir;
        this.agent = agent;
        InitiateDestroy();
        BulletMovemenet(shootDir);
    }
    public TankAgent GetAgent() {
        return agent;
    }
    public void InitiateDestroy() {
        StartCoroutine(DestroyAfterDelay(destroyDelay));
    }
    private IEnumerator DestroyAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        OnBulletDestroyAction.Invoke(this, EventArgs.Empty);
        Destroy(gameObject);
    }
    private void BulletMovemenet(Vector3 shootDir) {
        rb.AddForce(shootDir * moveSpeed , ForceMode.Impulse);            
    }
    private void OnCollisionEnter(Collision collision) { 
        shootDir = ChangeShootDir(collision);
    }
    private Vector3 ChangeShootDir(Collision collision) {
        Vector3 normal = collision.contacts[0].normal;      
        shootDir = Vector3.Reflect(shootDir, normal);    
        if(!collision.gameObject.CompareTag("Wall")) {
            return shootDir;
        }
        gameObject.transform.rotation = Quaternion.LookRotation(shootDir);
        shootDir = gameObject.transform.forward;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(shootDir * moveSpeed , ForceMode.Impulse);       
        return shootDir;
    }
}
