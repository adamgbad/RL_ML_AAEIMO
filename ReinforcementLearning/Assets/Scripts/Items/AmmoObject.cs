using UnityEngine;

public class AmmoObject : MonoBehaviour, IObject
{
    private int ammoAmount = 20;
    public bool isHit = false;
    public int SetAgent(int current, int max) {
        current += ammoAmount;
        if (current > max) {
            current = max;
        }
        return current;
    }
}
