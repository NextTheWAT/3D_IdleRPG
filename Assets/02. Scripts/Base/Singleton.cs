using UnityEngine;

/// <summary>
/// Generic MonoBehaviour Singleton base.
/// 사용법: public class GameManager : Singleton<GameManager> { /* ... */ }
/// - 씬 어디서든 Instance로 접근
/// - 중복 생성 방지, 씬 전환 유지(DontDestroyOnLoad) 옵션
/// - 종료 시 새로 생성 안 함(유령 오브젝트 방지)
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;
    private static readonly object _lock = new object();

    /// <summary>싱글톤이 유효하게 초기화되었는지</summary>
    public static bool IsInitialized => _instance != null;

    /// <summary>전역 인스턴스(없으면 찾거나 생성)</summary>
    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;

            lock (_lock)
            {
                if (_instance != null) return _instance;

                // 씬에서 기존 인스턴스 찾기(비활성 포함)
#if UNITY_2023_1_OR_NEWER
                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
                _instance = FindObjectOfType<T>(true);
#endif
                if (_instance != null) return _instance;

                // 없으면 새로 생성
                var go = new GameObject(typeof(T).Name);
                _instance = go.AddComponent<T>();
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // 이미 다른 인스턴스가 있으면 자신 제거
            Destroy(gameObject);
            return;
        }

        _instance = (T)this;
    }

}
