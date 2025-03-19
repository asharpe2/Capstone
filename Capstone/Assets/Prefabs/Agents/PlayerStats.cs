using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int totalDamageDealt = 0;
    public int totalCombos = 0;

    public void AddDamage(int amount)
    {
        totalDamageDealt += amount;
    }

    public void AddCombo()
    {
        totalCombos++;
    }

    public void ResetStats()
    {
        totalDamageDealt = 0;
        totalCombos = 0;
    }
}