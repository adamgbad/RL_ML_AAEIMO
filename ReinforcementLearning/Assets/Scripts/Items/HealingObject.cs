using UnityEngine;

public class HealingObject : MonoBehaviour, IObject
{
    private int healAmount = 30;
    public bool isHit = false;
    public int SetAgent(int current, int max) {
        current += healAmount;
        if(current > max) {
            current = max;
        }
        return current;
    }
}
