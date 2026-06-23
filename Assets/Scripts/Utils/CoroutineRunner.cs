using UnityEngine;

namespace Utils
{
    public class CoroutineRunner : MonoBehaviour {
        private static readonly string CoroutineRunnerObjectName = "CoroutineRunnerObject";
        private static CoroutineRunner _instance;
        public static CoroutineRunner Instance {
            get {
                if (_instance == null) {
                    var go = new GameObject(CoroutineRunnerObjectName);
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
    }
}