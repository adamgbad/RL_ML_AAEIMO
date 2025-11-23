using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ShootSystem: MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private Transform shootPosition;

    public event EventHandler OnBulletDestroy;

    private bool canShoot = true;
    private bool outOfAmmo = false;

    private Transform bulletTransform = null;
    private List<Transform> bulletsTransform = new List<Transform>();
    public void Shoot(TankAgent agent, ref int currentBullet, Transform prefab) {
        if (currentBullet > 0) {
            outOfAmmo = false;
            if (canShoot && !outOfAmmo) {
                
                currentBullet--;
                Vector3 shootDir = shootPosition.forward.normalized;
                
                bulletTransform = Instantiate(prefab, shootPosition.position, Quaternion.LookRotation(shootDir), parent);
                bulletsTransform.Add(bulletTransform); 
                
                bulletTransform.GetComponent<Bullet>().Setup(agent, shootDir);
                bulletTransform.GetComponent<Bullet>().OnBulletDestroyAction += ShootSystem_OnBulletDestroyAction;

                canShoot = false;
                if(currentBullet == 0) {
                    outOfAmmo = true;
                }
                StartCoroutine(Reloding());
            }
        } 
    }
    private void ShootSystem_OnBulletDestroyAction(object sender, EventArgs e) {
        OnBulletDestroy?.Invoke(this, EventArgs.Empty);
    }
    public void DestroyBullets() {
        if(GetBulletsTransform() != null && GetBulletsTransform().Count != 0) {
            foreach (var item in GetBulletsTransform()) {
                Destroy(item.gameObject);
            }
        }       
    }
    public List<Transform> GetBulletsTransform() {
        if (bulletsTransform != null && bulletsTransform.Count != 0) {
            for (int i = bulletsTransform.Count - 1; i >= 0; i--) {
                if (bulletsTransform[i] == null) {
                    bulletsTransform.Remove(bulletsTransform[i]);
                }
            }
            if (bulletsTransform != null && bulletsTransform.Count != 0) {
                return bulletsTransform;
            }
        }
        return null;
    }
    public void SetShoot() {
        canShoot = true;
        outOfAmmo = false;
    }
    public bool GetShootState() {
        return canShoot;
    }
    public bool GetAmmoState() {
        return outOfAmmo;
    }
    IEnumerator Reloding() {
        yield return new WaitForSeconds(1);
        canShoot = true;
    }
}
