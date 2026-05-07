# Snow-Crawler: Prologue — 최종 인수인계 문서
> 최종 업데이트: 2026-05-07 (수) 학교 PC
> 이 문서를 AI에게 전달하면 즉시 맥락을 파악하고 작업을 이어갈 수 있습니다.

---

## ⚠️ 절대 주의사항 (AI가 반드시 숙지할 것)

1. **씬 파일(.unity)은 집 PC의 것이 최신입니다.** 학교에서 씬은 일절 건드리지 않았습니다.
2. **스크립트(.cs)는 GitHub의 것이 최신입니다.** `git pull`로 받으세요.
3. 아래 작업들은 전부 **"스크립트(뇌)만 만들어둔 상태"**입니다. 씬에 오브젝트를 배치하고 참조를 연결하는 **"몸 조립"**은 아직 안 했습니다.
4. **기존에 씬에 이미 붙어있는 스크립트들**(FogManager, FlashlightController, FootstepSound, SortieManager 등)은 코드를 고도화했기 때문에 별도 씬 작업 없이 알아서 새 기능이 적용됩니다.
5. **새로 만든 스크립트들**(SaveManager, SubtitleManager, GameDirector, HypothermiaHallucination)은 씬에 오브젝트를 만들어 부착해야 합니다.

---

## 🔧 집에 도착하면 할 일 (순서대로)

### 0단계: Git 동기화 (⚠️ 씬 백업 먼저!)
```
1. Unity를 닫은 상태에서 시작
2. 씬 파일 백업: Assets/Scenes/SampleScene.unity → 바탕화면에 복사해두기
3. git pull  (스크립트만 내려옴, 씬은 안 건드림)
4. Unity 열기 → 씬이 자동 로드됨 → Ctrl+S로 저장
5. git add . → git commit -m "씬+스크립트 동기화" → git push
```
> ⚠️ 만약 git pull 후 씬이 이상하면 바탕화면 백업 파일을 되돌려놓으면 됩니다.
> 학교에서 씬은 일절 커밋하지 않았으므로 정상적이라면 pull로 씬이 변하지 않습니다.

### 1단계: SaveManager 씬 배치
- 빈 게임오브젝트 생성 → 이름: `SaveManager`
- `SaveManager.cs` 부착
- Inspector 참조는 비워둬도 됨 (자동 탐색)

### 2단계: SubtitleManager 씬 배치
- 빈 게임오브젝트 생성 → 이름: `SubtitleManager`
- `SubtitleManager.cs` 부착
- **Canvas 안에 TextMeshPro-Text(UI) 생성:**
  - 위치: 화면 하단 중앙 (Anchor: bottom-center)
  - 정렬: Center
  - 폰트 크기: 24~28 정도
  - 색상: 흰색
  - 이름: `SubtitleText`
- `SubtitleManager` 컴포넌트의 `Subtitle Text` 필드에 위 텍스트를 드래그

### 3단계: GameDirector 씬 배치
- 빈 게임오브젝트 생성 → 이름: `GameDirector`
- `GameDirector.cs` 부착
- **Canvas 안에 Image(UI) 생성:**
  - 색상: 검은색 (R:0, G:0, B:0, A:0 → 투명하게 시작)
  - Stretch로 화면 꽉 채움
  - Raycast Target: **끄기**
  - 이름: `FadeOverlay`
- `GameDirector` 컴포넌트에 연결:
  - `Fade Overlay` ← 위의 검은색 Image
  - `Player Controller` ← 플레이어의 `PlayerController` 컴포넌트
  - `Footstep Sound` ← 플레이어의 `FootstepSound` 컴포넌트

### 4단계: HypothermiaHallucination 씬 배치
- **플레이어 게임오브젝트**에 `HypothermiaHallucination.cs` 부착 (새 오브젝트 불필요)
- Inspector의 `Fake Footsteps` 배열에 발소리 오디오 클립 2~3개 넣기
  - 기존 눈밭 발소리 클립을 그대로 넣어도 됨 (피치가 랜덤으로 변조됨)
- 나머지 참조(SurvivalTimer, Camera)는 자동 탐색

### 5단계: 확인만 하면 되는 것들 (추가 작업 불필요)
| 스크립트 | 상태 | 왜 작업 불필요? |
|---|---|---|
| FlashlightController.cs | 고도화 완료 | 이미 플레이어에 붙어있음. 배터리/깜빡임 자동 적용 |
| FogManager.cs | 고도화 완료 | 이미 씬에 붙어있음. 돌풍 웨이브 자동 적용 |
| FootstepSound.cs | 고도화 완료 | 이미 플레이어에 붙어있음. Wood 태그 바닥 자동 감지 |
| SortieManager.cs | 수정 완료 | 이미 씬에 붙어있음. 자동저장/배터리충전/자막 연동 |
| SurvivalTimer.cs | 수정 완료 | GameDirector/SubtitleManager 연동 코드 추가됨 |

---

## 📋 오늘 구현한 전체 기능 목록

### 신규 스크립트 (씬 배치 필요)
1. **SaveManager.cs** — 가구 배치/생존 상태/출격 횟수를 JSON으로 저장/복원. 귀환 시 자동 저장.
2. **SubtitleManager.cs** — 페이드 인/아웃 자막 큐 시스템. 긴급(urgent) 메시지 덮어쓰기 지원.
3. **GameDirector.cs** — 시네마틱 게임오버 연출. 조작 마비 → 자막 → 화면 암전 → 결과창.
4. **HypothermiaHallucination.cs** — 체온 15초 이하 시 FOV 울렁거림 + 등 뒤 가짜 발소리 + 환각 자막.

### 기존 스크립트 고도화 (씬 배치 불필요)
5. **FlashlightController.cs** — 배터리 소모(약 1분 지속) + 20% 이하 깜빡임 + 방전 시 강제 꺼짐 + 귀환 시 자동 100% 충전.
6. **FogManager.cs** — 출격 횟수 비례 다크 블리자드 + 45~75초 주기 랜덤 돌풍 화이트아웃(12초).
7. **FootstepSound.cs** — Raycast로 바닥 태그 감지, 눈밭/나무바닥 발소리 분리.

### 기존 스크립트 연동 수정
8. **SortieManager.cs** — 귀환 시 SaveManager.SaveGame() 호출 + FlashlightController.RechargeBattery() 호출 + 출격 자막 트리거.
9. **SurvivalTimer.cs** — 체온 25% 이하 시 SubtitleManager 경고 자막 + GameOver 시 GameDirector 시네마틱 호출 + timeRemaining 프로퍼티 추가.
10. **PlayerInventory.cs** — 가방 초과 시 SubtitleManager 경고 자막 트리거.

---

## 🤖 집에서 AI에게 보낼 프롬프트 (복사+붙여넣기)

아래 텍스트를 그대로 복사해서 집에 있는 AI에게 전달하세요:

---

> 안녕! 이 프로젝트의 `DEV_HANDOFF.md` 파일을 분석해서 맥락을 완전히 숙지해 줘.
>
> **중요:** 이 문서에 적힌 것은 학교에서 스크립트(코드)만 짜둔 거야. 씬 파일은 건드리지 않았어.
> 지금 내 PC(집)의 씬이 최신이고, `git pull`은 이미 했어.
>
> 너는 MCP 툴(mcp_unityMCP)을 사용해서 아래 작업을 순서대로 해줘:
>
> 1. **SaveManager 배치:** 씬에 빈 게임오브젝트 `SaveManager` 만들고 `SaveManager.cs` 부착
> 2. **SubtitleManager 배치:** 빈 게임오브젝트 `SubtitleManager` 만들고 `SubtitleManager.cs` 부착. Canvas 안에 TextMeshPro-Text(UI)를 만들어서 `subtitleText` 필드에 연결해 줘. (화면 하단 중앙, 흰색, 24pt)
> 3. **GameDirector 배치:** 빈 게임오브젝트 `GameDirector` 만들고 `GameDirector.cs` 부착. Canvas 안에 화면 꽉 채우는 검은색 Image(Alpha=0) 만들어서 `fadeOverlay`에 연결. 그리고 플레이어의 `PlayerController`와 `FootstepSound`를 찾아서 각각 `playerController`, `footstepSound` 필드에 연결해 줘.
> 4. **HypothermiaHallucination 배치:** 플레이어 게임오브젝트에 `HypothermiaHallucination.cs` 부착. `fakeFootsteps` 배열은 비워둬도 돼 (나중에 내가 오디오 넣을게).
> 5. **이미 씬에 붙어있는 FogManager, FlashlightController, FootstepSound, SortieManager는 절대 건드리지 마.** 코드만 업데이트된 거라 알아서 작동해.
>
> 작업이 끝나면 씬을 저장(Ctrl+S)하고, 다음에 뭘 할지 알려줘!

---

## ✅ 체크리스트 (전부 완료되면 게임 플레이 가능)

- [ ] git pull 완료
- [ ] 씬 저장 후 커밋/푸시 (씬 동기화)
- [ ] SaveManager 씬 배치
- [ ] SubtitleManager 씬 배치 + UI 텍스트 연결
- [ ] GameDirector 씬 배치 + UI Image + 플레이어 참조 연결
- [ ] HypothermiaHallucination 플레이어에 부착
- [ ] 플레이 테스트: 별장 밖으로 나가서 자막/안개/손전등/환각 확인
