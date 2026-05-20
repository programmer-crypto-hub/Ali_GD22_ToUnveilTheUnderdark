using UnityEngine;
public class RuntimeStateController : MonoBehaviour{
    private void Awake() => DontDestroyOnLoad(this.gameObject);
}