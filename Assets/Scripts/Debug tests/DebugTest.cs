using UnityEngine;

public class DebugTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("✅ DebugTest is running — Start() has been called!");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("🖱️ Mouse clicked!");
        }
    }
}
