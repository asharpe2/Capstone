using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrailerPunchDemo : MonoBehaviour
{
    [Tooltip("Drag your PlayerController here")]
    public PlayerController player;

    [Tooltip("Seconds to wait *after* round-start before the first punch")]
    public float startDelay = 0f;

    [Tooltip("Time between each punch in seconds")]
    public float punchInterval = 0.3f;
    public float punchIntervalJab = 0.15f;
    public float punchIntervalFeint = 1.5f;

    [System.Serializable]
    public struct PunchStep
    {
        public string punchName;  // e.g. "Jab", "Straight", "Left_Hook", "Right_Hook"
        public float staminaCost; // same values you use in ThrowPunch()
        public bool feint;
    }

    [Tooltip("Define your punch order here")]
    public List<PunchStep> sequence = new List<PunchStep>
    {
        new PunchStep { punchName = "Jab",        staminaCost = 5f,  feint = false   },
        new PunchStep { punchName = "Straight",   staminaCost = 10f, feint = false   },
        new PunchStep { punchName = "Left_Hook",  staminaCost = 15f, feint = false   },
        new PunchStep { punchName = "Right_Hook", staminaCost = 15f, feint = false   },
    };

    bool demoStarted = false;

    private IEnumerator RunDemo()
    {
        yield return new WaitForSeconds(startDelay);

        foreach (var step in sequence)
        {
            player.ThrowPunch(step.punchName, step.staminaCost);
            if (step.feint) yield return new WaitForSeconds(punchIntervalFeint);
            else if (step.punchName == "Jab") yield return new WaitForSeconds(punchIntervalJab);
            else yield return new WaitForSeconds(punchInterval);
        }
    }

    /// <summary>
    /// Call this the moment your round actually begins.
    /// </summary>
    public void StartDemo()
    {
        if (demoStarted) return;
        demoStarted = true;

        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (player == null)
        {
            Debug.LogError("TrailerPunchDemo: no PlayerController found!");
            return;
        }
        StartCoroutine(RunDemo());
    }
}