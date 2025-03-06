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
            case "Straight": return chin;
            case "Left_Hook": return rightCheek;
            case "Right_Hook": return leftCheek;
            default: return chin;
        }
    }
}
