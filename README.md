# Unity Car Survival Prototype

> 탑다운 **차량 x 생존 슈팅** 프로토타입. 자동차에 무기를 장착해 적을 처치하고, 시간에 따라 스테이지를 올리며 버텨내는 구조입니다. 싱글톤 매니저, 무기/업그레이드, 스테이지/스폰, 리스폰, UI‧사운드까지 바로 확장 가능한 샘플로 구성했습니다.

## 🎮 게임 소개

차량을 조작(또는 AI 주행)하며 몰려오는 적을 처치하고 **시간이 지날수록 올라가는 Stage**를 버텨내는 탑다운 생존 슈팅입니다.  
골드/경험치를 모아 **무기를 업그레이드**하고, 위기 상황에서는 **Respawn 버튼**으로 트랙 위 최근접 노드에 복귀해 흐름을 잇습니다.

- 생존 목표: 가능한 오래 버티며 Stage를 상승시키고, 더 강한 적을 상대합니다.
- 성장 루프: 처치 보상 → 골드/EXP 획득 → **Upgrade 패널**에서 무기 레벨업/티어업.

---

## 🖼️ 플레이영상&GIF



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

## 🧯 트러블슈팅

1) **UnityException: FindObjectsOfType는 생성자에서 호출 불가**  
- **증상**: `UIConditionBinder` 등에서 필드 초기화/생성자 타이밍에 `Singleton.Instance` 접근 → 예외 발생.  
- **해결**: `Awake/Start`에서 참조를 가져오고, 이벤트는 `OnEnable`에서 구독, `OnDisable`에서 해제.  
- **팀 규칙**: “`Singleton.Instance`를 **필드 초기화에서 금지**”.

2) **UnassignedReferenceException: PlayerManager.carTransform 미할당**  
- **증상**: `RespawnManager`가 버튼에서 차 Transform을 참조하려는데 null.  
- **해결**: `RespawnManager`에서 **자체 Resolve**(PlayerManager → 씬 검색 → car.transform 순) or 인스펙터에서 명시 할당.  
- **보완**: Script Execution Order로 `PlayerManager.Awake`가 먼저 실행되도록 조정.

3) **이벤트 중복 구독으로 UI가 두 배로 갱신됨**  
- **원인**: `OnEnable`에서 `AddListener`만 하고 `OnDisable`에서 `RemoveListener` 누락.  
- **해결**: `OnDisable`에서 반드시 해제.

4) **빌보드 HP UI 떨림/뒤집힘**  
- **원인**: 월드 스페이스 Canvas가 카메라를 정확히 바라보지 않음.  
- **해결**: `LateUpdate`에서 `LookAt(camera.position, Vector3.up)` 또는 `Camera.main.transform.forward`를 정면으로 사용.

5) **WheelCollider 시각/물리 미스매치**  
- **증상**: 바퀴 메시가 뜨거나 박힘.  
- **해결**: `GetWorldPose`로 메시 위치/회전 동기화, `suspensionDistance`와 메시에 맞는 반지름/오프셋 재조정.

---

## 📜 License

학습/포트폴리오 용도로 자유 사용 가능(개별 리소스/에셋의 라이선스는 각 파일 출처를 따릅니다).

---


