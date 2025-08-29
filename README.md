# Unity Car Survival Prototype

> 탑다운 **차량 x 생존 슈팅** 프로토타입. 자동차에 무기를 장착해 적을 처치하고, 시간에 따라 스테이지를 올리며 버텨내는 구조입니다. 싱글톤 매니저, 무기/업그레이드, 스테이지/스폰, 리스폰, UI‧사운드까지 바로 확장 가능한 샘플로 구성했습니다.

## 🎮 게임 소개

차량을 조작(또는 AI 주행)하며 몰려오는 적을 처치하고 **시간이 지날수록 올라가는 Stage**를 버텨내는 탑다운 생존 슈팅입니다.  
골드/경험치를 모아 **무기를 업그레이드**하고, 위기 상황에서는 **Respawn 버튼**으로 트랙 위 최근접 노드에 복귀해 흐름을 잇습니다.

- 생존 목표: 가능한 오래 버티며 Stage를 상승시키고, 더 강한 적을 상대합니다.
- 성장 루프: 처치 보상 → 골드/EXP 획득 → **Upgrade 패널**에서 무기 레벨업/티어업.

---

## 🖼️ 플레이영상&GIF
![Animation (1)](https://github.com/user-attachments/assets/14b433c7-ded1-4470-af88-10040b8de8d0)



## 🕹️ 플레이 방법

1. **시작**: 씬을 재생하면 `StageManager`가 타이머를 돌리며 Stage가 주기적으로 상승합니다.  
2. **이동**:  
   - 기본은 **CarAI**가 등록된 `trackNodes`를 따라 주행합니다(루프/랜덤/커스텀 경로 옵션).  
   - 전방 센서(`SensorManager`)가 장애물과 접촉 시 일시 정지했다가, 이탈하면 재출발합니다.
3. **전투**: `WeaponSlotsManager`가 슬롯 mount에 무기를 스폰하며, `BaseWeapon` 파이프라인에 따라 자동 조준·발사합니다.  
4. **성장**: 적 처치 보상으로 골드/EXP를 얻고, **Upgrade 패널**에서 레벨업/티어업을 진행합니다.  
5. **복구**: 길 밖으로 이탈하면 **Respawn 버튼**으로 최근접 트랙 노드에 즉시 복귀합니다.

> 기본 흐름은 **AI 주행 + 자동 사격**이며, 필요 시 `controllingCarAI`를 붙여 주행 파라미터를 런타임에 조절할 수 있습니다.

---

## ✨ Features

- **Generic Singleton 베이스**: 모든 매니저는 `ClassName.Instance`로 전역 접근. (중복 생성 방지/선 생성 필요 없음)
- **무기 시스템(스크립터블 오브젝트)**: `WeaponData`로 수치(피해/사거리/발사 속도/쿨타임/티어) 정의, `BaseWeapon`은 타게팅/발사 공통 처리, 파생 무기(예: `CannonWeapon`)로 구체 로직 확장.
- **업그레이드 & 슬롯**: `WeaponSlotsManager`가 슬롯에 무기 프리팹을 생성/교체하고 레벨‧티어 업 로직 실행. `UpgradePanelController`로 UI와 연동.
- **스테이지 & 적 스폰**: `StageManager`의 타이머가 주기적으로 Stage를 증가시키고, `EnemyManager`가 살아있는 적 수를 유지하며 NavMesh 영역에 스폰.
- **Respawn 버튼**: 버튼 클릭 시 “가장 가까운 트랙 노드”로 차량을 즉시 복귀(텔레포트)시키고 AI 경로 상태 초기화.
- **UI/HUD**: 체력/레벨/경험치 바인딩, 골드 표시, 차량 속도 표시, 적 머리 위 HP 빌보드.
- **사운드 시스템**: BGM, 풀링 기반 SFX, UI 슬라이더로 볼륨 제어.
- **센서 & 이동 제어**: 전방 센서로 정지/재가속, 런타임에서 CarAI 파라미터 조절 샘플 제공.

---

## 📁 프로젝트 구조(02. Scripts)

```
02. Scripts
├─ Base
│  ├─ BaseCondition.cs
│  ├─ BaseWeapon.cs
│  └─ Singleton.cs
├─ Camera
│  └─ FollowCamera.cs
├─ Controller
│  └─ UpgradePanelController.cs
├─ Enemy
│  ├─ AI
│  │  └─ EnemyAI.cs
│  ├─ Attack
│  │  ├─ EnemyAttack.cs
│  │  └─ EnemyCondition.cs
│  └─ Interface
│     └─ IDamageable.cs
├─ Manager
│  ├─ Enemy
│  │  └─ EnemyManager.cs
│  ├─ Player
│  │  ├─ PlayerManager.cs
│  │  └─ RespawnManager.cs
│  ├─ SkillManager.cs
│  ├─ CurrencyManager.cs
│  ├─ Weapon
│  │  └─ WeaponSlotsManager.cs
│  ├─ GameManager.cs
│  ├─ SoundManager.cs
│  └─ StageManager.cs
├─ Player
│  ├─ Skill
│  │  ├─ AttackController.cs (partial)
│  │  ├─ AttackController.Heal.cs (partial)
│  │  └─ AttackController.SpinAttack.cs (partial)
│  ├─ CarAI.cs
│  └─ PlayerCondition.cs
├─ Scriptable
│  └─ WeaponData.cs
├─ UI
│  ├─ Enemy
│  │  └─ EnemyHealthUI.cs
│  ├─ Player
│  │  ├─ CarSpeedUI.cs
│  │  └─ GoldUI.cs
│  └─ UIConditionBinder.cs
├─ Util
│  ├─ controllingCarAI.cs
│  └─ SensorManager.cs
└─ Weapon
   ├─ Projectile
   │  └─ SimpleProjectile.cs
   └─ CannonWeapon.cs
```

---

## 🧱 설계/디자인 패턴

- **Singleton(제네릭)**  
  `Singleton<T>`를 상속한 매니저들이 전역 접근점(`.Instance`)을 제공합니다. 중복 생성 방지, 씬 어디서든 호출.

- **Observer(이벤트 기반 UI/게임플로우)**  
  `PlayerCondition`(HP/MP/EXP/Level) 변경 → UnityEvent 발행 → `UIConditionBinder` 등 UI가 구독/갱신.  
  *규칙*: `OnEnable`에서 AddListener, `OnDisable`에서 RemoveListener(중복 방지).

- **Strategy(무기 확장 구조)**  
  `BaseWeapon`이 타게팅/발사 파이프라인의 공통 틀을 제공, `CannonWeapon` 같은 파생으로 구체 로직을 구성.  
  데이터는 `WeaponData`(SO)로 주입 → **코드 수정 없이** 새 무기 추가/튜닝 가능.

- **Interface(결합도↓)**  
  `IDamageable`로 피해 전파. 투사체/근접공격/스킬이 대상이 무엇이든 동일 API로 처리.

- **Partial Class(관심사 분리)**  
  `AttackController`를 **SpinAttack/Heal**로 파일 분할. 기능 단위로 나눠 협업/가독성 향상.

- **Data-Driven & Composition**  
  체력/상태(`PlayerCondition`), 스킬(`AttackController`), 이동(`CarAI`) 등 **컴포넌트 조합**으로 기능 구성.  
  파라미터는 인스펙터/ScriptableObject로 데이터화.

---

## 🧰 사용 기술 & 시스템

- **NavMesh**: CarAI의 웨이포인트 생성/스냅, EnemyManager 스폰 지점 샘플링.
- **WheelCollider & Rigidbody**: 바퀴 `GetWorldPose`로 메시 정렬, 모터 토크/브레이크/조향 제어.
- **Catmull‑Rom 스플라인**: `trackNodes` 사이를 부드럽게 보간해 웨이포인트 생성(샘플 수/루프 옵션).
- **Physics Overlap/Trigger**: Spin 공격 `OverlapSphere`; EnemyAttack/Projectile 충돌 처리로 `IDamageable` 호출.
- **TMP + UGUI**: 체력/속도/골드/업그레이드 패널 등 HUD 출력.

---

## 🚗 핵심 동작 요약

- **CarAI (트랙 주행 + 랜덤/커스텀 경로)**  
  - `trackNodes` → Catmull‑Rom으로 웨이포인트 생성 → 바퀴/조향/가속 갱신.  
  - `carFront` 기준 도달 판정, `CurrentSpeedKmh`로 속도 UI 제공.  
  - 디버그: 웨이포인트/노드/FOV 기즈모 표시.

- **Respawn(가장 가까운 트랙 노드)**  
  - UI 버튼 → 현재 위치(가능하면 `carFront`) 기준 최근접 노드 탐색 → `CarAI.TeleportToNode()` 호출.  
  - Rigidbody 속도/각속도 리셋 + 웨이포인트 초기화 → 즉시 정상 주행 재개.

- **무기/업그레이드**  
  - `WeaponSlotsManager`가 슬롯 mount에 무기 프리팹 스폰, `WeaponData`로 레벨업/티어업.  
  - `UpgradePanelController`가 버튼/아이콘/텍스트 반영.

- **스테이지 & 적 스폰**  
  - `StageManager` 타이머로 Stage 상승(이벤트 발행).  
  - `EnemyManager`가 NavMesh 영역 내에서 최대 생존 수를 유지하며 스폰, Stage 이벤트에 연동 가능.

- **UI/HUD**  
  - `UIConditionBinder`(HP/MP/EXP/Level), `EnemyHealthUI`(빌보드 HP), `GoldUI`, `CarSpeedUI`.

---

## 🧪 씬 세팅 체크리스트

1. **매니저 배치**: `Game/Stage/Enemy/Sound/Currency/Skill/WeaponSlots/PlayerManager`(싱글톤 1개씩).  
2. **플레이어 차량**: `CarAI` + 바퀴(콜라이더/메시) + `carFront` 지정, `trackNodes` 배열에 노드 등록.  
3. **무기**: `WeaponData`(SO) 만들고 슬롯 mount에 연결 → 시작 시 자동 스폰.  
4. **UI**: 슬라이더/텍스트를 각 UI 스크립트에 인스펙터로 연결.  
5. **Respawn 버튼**: `RespawnManager.RespawnToNearestTrackNode()`를 OnClick에 연결.  
6. **NavMesh**: 스폰 영역/트랙 주변에 NavMesh 베이크.

---

## 🧯 Troubleshooting (Deep Dive)

> 단순 현상 나열이 아니라 **왜 문제가 생겼고** → **어떻게 고쳤으며** → **다시는 안 나오게 무엇을 규칙화했는지**를 적었습니다.

### 1) `FindObjectsOfType`를 생성자/필드 초기화에서 호출
**증상**  
`UIConditionBinder`가 생성자 타이밍에 `Singleton.Instance`에 접근 → UnityException 발생.

**원인**  
MonoBehaviour 생성자/필드 초기화 시점엔 씬 오브젝트가 완전히 준비되지 않았습니다.

**해결**  
- `Awake/Start`에서 참조 획득, `OnEnable`에서 이벤트 구독, `OnDisable`에서 해제.
- 인스펙터 할당 가능하면 `[SerializeField]`로 수동 바인딩.

**재발 방지**  
- 팀 규칙: “**Singleton.Instance를 필드 초기화에서 호출 금지**”.  
- 코드 리뷰 체크리스트에 추가.

---

### 2) `PlayerManager.carTransform` 미할당으로 Respawn 버튼 NRE
**증상**  
버튼 누를 때 `RespawnManager`가 `carTransform`에 접근하다 `UnassignedReferenceException`.

**원인**  
- `PlayerManager.Awake`가 아직 실행되지 않았거나, 인스펙터 미할당.  
- 씬 전환/활성 순서에 따라 참조 타이밍이 뒤엉킴.

**해결**  
- `RespawnManager`에 `ResolveRefs()` 추가: PlayerManager → 씬 검색 → `car.transform` 순으로 **자체 복구**.  
- Script Execution Order에서 `PlayerManager.Awake`가 먼저 돌도록 지정.

**재발 방지**  
- 싱글톤 참조는 **런타임에서 한 번 더 유효성 검사**.  
- 중요한 참조는 인스펙터에 **명시 바인딩 우선**.

---

### 3) `SoundManager` NRE (UI에서 OnEnable 중 효과음 재생)
**증상**  
`UpgradePanelController.OnEnable()`에서 `PlaySfx()` 호출 시 NRE.

**원인**  
- 사운드 풀/오디오소스가 `SoundManager.Awake()` 이전이라 아직 준비 전.  
- 또는 클립/슬라이더 미할당.

**해결**  
- UI 쪽에서 **지연 호출**(예: `Start`, `WaitForEndOfFrame`) 또는 `SoundManager.IsReady` 플래그/널가드.  
- `SoundManager`는 `Awake()`에서 풀을 반드시 초기화.

```csharp
// UI 예시
if (SoundManager.Instance && SoundManager.Instance.IsReady)
    SoundManager.Instance.PlaySfx(SoundManager.SfxId.Upgrade);
```

**재발 방지**  
- “UI의 OnEnable에서 오디오 바로 재생 금지” 가이드.  
- 씬 부팅 순서(Execution Order) 문서화.

---

### 4) `BaseCondition`의 보호된 필드 접근(CS0122)
**증상**  
`EnemyHealthUI`에서 `BaseCondition.health`/`maxHealth` 접근 시 접근 제한자 에러.

**원인**  
캡슐화 위반. 외부 UI에서 내부 상태 필드를 직접 읽으려 함.

**해결**  
- **Public 프로퍼티/이벤트**로 접근(`Health`, `Health01` 등).  
- 또는 상태 변경 이벤트를 구독해 슬라이더만 업데이트.

**재발 방지**  
- “UI는 **이벤트/프로퍼티만** 사용” 원칙 고지.

---

### 5) 빌보드 HP UI 떨림/뒤집힘
**증상**  
카메라가 빠르게 회전할 때 HP 슬라이더가 순간적으로 뒤집히거나 떨림.

**원인**  
- Update 순서/회전축 처리 부정확.  
- 월드 스페이스 Canvas가 LookAt을 Z축까지 뒤집음.

**해결**  
- `LateUpdate`에서 카메라를 향하도록 처리.  
- 필요 시 Y축만 회전(수평 빌보드).

```csharp
void LateUpdate() {
    var cam = Camera.main.transform;
    Vector3 dir = cam.position - transform.position; dir.y = 0f;
    if (dir.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(dir);
}
```

**재발 방지**  
- 모든 월드 스페이스 UI는 **LateUpdate**에서 방향/위치 갱신.

---

### 6) 투사체 임팩트 후 TrailRenderer/파티클 누수
**증상**  
충돌 직후 투사체 파괴 시 Trail/Impact VFX가 같이 사라지거나 씬에 고아 오브젝트로 남음.

**원인**  
Trail이 본체에 붙어 있어 함께 파괴되거나, 별도 라이프사이클이 없음.

**해결**  
- 충돌 시 Trail을 본체에서 **분리(detach)** → 수명 타이머로 제거.  
- Impact VFX는 **풀링** 또는 `Destroy(go, duration)`로 정리.

**재발 방지**  
- ‘투사체 본체/트레일/임팩트’를 **분리된 오브젝트**로 관리.

---

### 7) NavMesh 스폰 지점이 바깥/벽 내부로 샘플링
**증상**  
적이 벽 안/경계 밖에서 스폰되거나 즉시 추락.

**원인**  
`NavMesh.SamplePosition` 반경이 너무 작거나, 허용 에어리어 마스크가 누락.

**해결**  
- 스폰 후보 지점에서 `SamplePosition(point, out hit, radius, areaMask)`로 **여유 반경**을 주고 샘플.  
- `allowedAreaNames`로 레이어링/마스크를 명시.

**재발 방지**  
- 스폰 영역을 **BoxCollider/Gizmo**로 시각화하고, 반경/마스크를 프리셋화.

---

### 8) WheelCollider 시각/물리 불일치(튀거나 박힘)
**증상**  
빠른 속도/경사에서 바퀴 메시가 떠 보이거나 섀시가 튀는 현상.

**원인**  
- 메시 위치를 Update에서 갱신.  
- Rigidbody 보간/고정 타임스텝 불일치.

**해결**  
- **FixedUpdate**에서 `wheelCollider.GetWorldPose(out pos, out rot)`로 메시 동기화.  
- Rigidbody Interpolate 사용, Fixed Timestep 점검(0.02s 권장).

**재발 방지**  
- 차량 비주얼은 **항상 FixedUpdate**에서 동기화.

---

### 9) 이벤트 중복 구독으로 UI 두 배 갱신
**증상**  
씬 재로드/패널 토글 이후 HP/골드 텍스트가 중복 업데이트.

**원인**  
`OnEnable`에서 `AddListener`만 하고 `OnDisable`에서 `RemoveListener` 누락.

**해결**  
- 모든 구독은 **OnEnable ↔ OnDisable** 페어로 관리.  
- 구독 전 `RemoveListener`로 한번 청소하는 방어 코드도 허용.

**재발 방지**  
- 리뷰 체크리스트: “이벤트 구독은 반드시 해제했나?”.

---

### 10) Stage 타이머 드리프트/폭주
**증상**  
일정 시간 후 Stage가 너무 빨리 오르거나 UI가 밀림.

**원인**  
`Time.time` 기반 반복에서 씬 일시정지/프레임 드랍에 영향, 또는 중복 코루틴 실행.

**해결**  
- **단일 코루틴 가드**(isRunning 플래그) + `yield return new WaitForSeconds(…)` 사용.  
- 일시정지 대응이 필요하면 `Time.unscaledDeltaTime`로 별도 누적.

**재발 방지**  
- 모든 루프/타이머에 **중복 실행 가드**와 “정지/재개” 시나리오 점검.

---

## 📜 License

학습/포트폴리오 용도로 자유 사용 가능(개별 리소스/에셋의 라이선스는 각 파일 출처를 따릅니다).

---


