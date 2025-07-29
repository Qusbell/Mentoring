using UnityEngine;

// 제네릭 싱글톤 패턴을 MonoBehaviour 기반으로 구현한 클래스
public class SingletonT<T> : MonoBehaviour where T : MonoBehaviour
{
    // 싱글톤 인스턴스를 저장하는 static 변수
    private static T instance;
    // 멀티스레드 환경에서 동시 접근을 막기 위한 lock 객체
    private static object lockObj = new object();
    // 애플리케이션이 종료 중임을 표시하는 플래그
    private static bool applicationIsQuitting = false;

    // 싱글톤 인스턴스에 접근하는 프로퍼티
    public static T Instance
    {
        get
        {
            // 애플리케이션이 종료 중이면 인스턴스를 반환하지 않고 경고 출력
            if (applicationIsQuitting)
            {
                Debug.LogWarning("[SingletonT] 이미 종료 중이므로 인스턴스를 반환하지 않습니다.");
                return null;
            }

            // 멀티스레드 환경에서 동시 접근 방지
            lock (lockObj)
            {
                // 인스턴스가 아직 생성되지 않았다면
                if (instance == null)
                {
                    // 씬에서 T 타입의 오브젝트를 찾음
                    instance = (T)FindObjectOfType(typeof(T));
                    // 씬에 존재하지 않으면
                    if (instance == null)
                    {
                        // 새로운 GameObject를 생성
                        GameObject singletonObj = new GameObject();
                        // GameObject에 T 타입 컴포넌트를 추가하고 인스턴스에 할당
                        instance = singletonObj.AddComponent<T>();
                        // GameObject의 이름을 "T (Singleton)"으로 지정
                        singletonObj.name = typeof(T).ToString() + " (Singleton)";

                        // 씬 전환 시 파괴되지 않도록 설정
                        DontDestroyOnLoad(singletonObj); // <- 필요한가?
                    }
                }
                // 인스턴스 반환
                return instance;
            }
        }
    }

    // 오브젝트가 파괴될 때 호출됨
    protected virtual void OnDestroy()
    {
        // 애플리케이션이 종료 중임을 표시
        applicationIsQuitting = true;
    }
}
