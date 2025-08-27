using UnityEngine;

/// <summary>
/// Generic MonoBehaviour Singleton base.
/// ����: public class GameManager : Singleton<GameManager> { /* ... */ }
/// - �� ��𼭵� Instance�� ����
/// - �ߺ� ���� ����, �� ��ȯ ����(DontDestroyOnLoad) �ɼ�
/// - ���� �� ���� ���� �� ��(���� ������Ʈ ����)
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;
    private static readonly object _lock = new object();

    /// <summary>�̱����� ��ȿ�ϰ� �ʱ�ȭ�Ǿ�����</summary>
    public static bool IsInitialized => _instance != null;

    /// <summary>���� �ν��Ͻ�(������ ã�ų� ����)</summary>
    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;

            lock (_lock)
            {
                if (_instance != null) return _instance;

                // ������ ���� �ν��Ͻ� ã��(��Ȱ�� ����)
#if UNITY_2023_1_OR_NEWER
                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
                _instance = FindObjectOfType<T>(true);
#endif
                if (_instance != null) return _instance;

                // ������ ���� ����
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
            // �̹� �ٸ� �ν��Ͻ��� ������ �ڽ� ����
            Destroy(gameObject);
            return;
        }

        _instance = (T)this;
    }

}
