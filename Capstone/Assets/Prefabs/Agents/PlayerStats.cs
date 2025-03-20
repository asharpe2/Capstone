using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerStats : MonoBehaviour
{
    public int totalDamageDealt = 0;
    public int totalCombos = 0;

    // Current combo being built
    private List<string> currentCombo = new List<string>();
    private float comboTimer = 0f;
    public float comboWindow = 1.5f; // Time allowed between punches to count as a combo

    // Track completed combos and their frequency
    public Dictionary<string, int> comboHistory = new Dictionary<string, int>();

    void Update()
    {
        if (currentCombo.Count > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                FinalizeCombo();
            }
        }
    }

    public void AddDamage(int amount)
    {
        totalDamageDealt += amount;
    }

    public void AddPunchToCombo(string punchType)
    {
        currentCombo.Add(punchType);
        comboTimer = comboWindow;
    }

    public void FinalizeCombo()
    {
        if (currentCombo.Count > 1) // Only store if meaningful
        {
            string comboKey = string.Join(" -> ", currentCombo);

            if (comboHistory.ContainsKey(comboKey))
                comboHistory[comboKey]++;
            else
                comboHistory[comboKey] = 1;

            totalCombos++;
        }
        currentCombo.Clear();
    }

    public void ResetStats()
    {
        totalDamageDealt = 0;
        totalCombos = 0;
        comboHistory.Clear();
        currentCombo.Clear();
        comboTimer = 0f;
    }

    // Get top 3 most used combos
    public List<KeyValuePair<string, int>> GetTopCombos(int topN)
    {
        return comboHistory
            .OrderByDescending(kvp => kvp.Value)
            .Take(topN)
            .ToList();
    }
}
