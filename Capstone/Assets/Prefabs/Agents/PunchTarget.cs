using UnityEngine;

public class PunchTarget : MonoBehaviour
{
    public Transform chin;
    public Transform leftCheek;
    public Transform rightCheek;

    // Retrieve attack target based on punch type
    public Transform GetTarget(string punchType)
    {
        switch (punchType)
        {
            case "Jab": return chin;
            case "Right_Hook": return leftCheek;
            case "Left_Hook": return rightCheek;
            default: return chin;
        }
    }
}
