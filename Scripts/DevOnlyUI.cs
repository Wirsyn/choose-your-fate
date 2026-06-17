using UnityEngine;

public class DevOnlyUI : MonoBehaviour
{
    void Awake()
    {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
        Destroy(gameObject);
#endif
    }
}