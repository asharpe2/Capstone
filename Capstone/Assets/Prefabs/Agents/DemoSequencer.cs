using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum DemoActionType
{
    Move,          // continuous movement for 'duration' seconds
    Punch,         // a single ThrowPunch(...)
    BlockStart,    // turn blocking on
    BlockEnd,      // turn blocking off
    CounterWindup, // prepare to counter (sets hurtbox.tag = "Counter")
    CounterExecute // end counter state
}

[System.Serializable]
public struct DemoStep
{
    public DemoActionType type;

    [Header("Punch Settings")]
    public string punchName;  // e.g. "Jab", "Straight", etc.
    public float staminaCost; // cost passed into ThrowPunch(...)

    [Header("Move Settings")]
    public Vector3 moveDirection; // world‐space dir (e.g. Vector3.forward)
    public float duration;        // seconds to run this step
}

public class DemoSequencer : MonoBehaviour
{
    [Tooltip("Agent (PlayerController or Enemy) to drive")]
    public Agent targetAgent;

    [Tooltip("Seconds to wait before the first step")]
    public float startDelay = 0f;

    [Tooltip("Build your action list here")]
    public List<DemoStep> steps = new List<DemoStep>();

    bool isRunning = false;

    /// <summary>
    /// Kick off the scripted sequence.
    /// </summary>
    public void StartDemo()
    {
        if (targetAgent == null)
        {
            Debug.LogError($"[DemoSequencer] No targetAgent assigned on '{name}'");
            return;
        }
        if (!isRunning)
            StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        isRunning = true;

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        foreach (var step in steps)
        {
            switch (step.type)
            {
                case DemoActionType.Move:
                    yield return MoveRoutine(step.moveDirection, step.duration);
                    break;

                case DemoActionType.BlockStart:
                    targetAgent.HandleBlocking(true);   // :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}
                    yield return new WaitForSeconds(step.duration);
                    break;

                case DemoActionType.BlockEnd:
                    targetAgent.HandleBlocking(false);  // :contentReference[oaicite:2]{index=2}:contentReference[oaicite:3]{index=3}
                    yield return new WaitForSeconds(step.duration);
                    break;

                case DemoActionType.Punch:
                    targetAgent.ThrowPunch(step.punchName, step.staminaCost); // :contentReference[oaicite:4]{index=4}:contentReference[oaicite:5]{index=5}
                    yield return new WaitForSeconds(step.duration);
                    break;

                case DemoActionType.CounterWindup:
                    targetAgent.StartCounter();         // :contentReference[oaicite:6]{index=6}:contentReference[oaicite:7]{index=7}
                    yield return new WaitForSeconds(step.duration);
                    break;

                case DemoActionType.CounterExecute:
                    targetAgent.StopCounter();          // :contentReference[oaicite:8]{index=8}:contentReference[oaicite:9]{index=9}
                    yield return new WaitForSeconds(step.duration);
                    break;
            }
        }

        isRunning = false;
    }

    IEnumerator MoveRoutine(Vector3 direction, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            // If this Agent is a PlayerController, use its DemoMove wrapper
            if (targetAgent is PlayerController pc)
            {
                pc.DemoMove(direction);
            }

            timer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // stop movement
        if (targetAgent is PlayerController pcStop)
            pcStop.DemoMove(Vector3.zero);
    }
}
