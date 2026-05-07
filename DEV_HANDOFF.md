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

### 0단계: Git 동기화 + 안전 세이브포인트 만들기

**순서가 매우 중요합니다. 반드시 아래 순서대로!**

```
1. Unity를 닫은 상태에서 시작
2. 혹시 모르니 씬 파일 백업: Assets/Scenes/SampleScene.unity → 바탕화면에 복사
3. git pull  (학교에서 만든 스크립트가 내려옴, 씬은 안 건드림)
4. Unity 열기 → 씬이 자동 로드됨 → 씬이 정상인지 눈으로 확인!
5. Ctrl+S로 씬 저장
6. ⭐ 안전 세이브포인트 만들기:
   git add .
   git commit -m "안전 세이브포인트: 최신씬 + 최신스크립트 통합 완료"
   git push
7. 이 커밋 해시를 메모해두기!
```
> ⚠️ **이 세이브포인트 = 최신 씬 + 최신 스크립트가 합쳐진 깨끗한 상태입니다.**
> 이후 인수인계 작업(1~4단계) 중 문제가 생기면:
> `git reset --hard [위 해시]` → 씬과 스크립트 모두 이 깨끗한 시점으로 복원됩니다.
> 
> 학교에서 씬은 일절 커밋하지 않았으므로, pull로 씬이 변하지 않습니다.
> 만약 pull 후 씬이 이상하면 바탕화면 백업을 되돌려놓으면 됩니다.

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

> 안녕! 나 지금 git pull → Unity 열기 → Ctrl+S → git commit+push 까지 이미 끝냈어 (안전 세이브포인트 완료). `DEV_HANDOFF.md` 파일을 분석해서 맥락을 완전히 숙지해 줘.
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

---

## 🎬 집에서 구현할 기능: 엔딩 시스템 "새벽이 온다"

### 컨셉: 2막 구조
게임이 "생존"과 "탈출" 두 막으로 나뉩니다.

### 1막: 생존 (현재 게임 루프)
- 플레이어가 출격을 반복하며 가구를 모아 별장 안락도를 채움
- CabinComfort가 **80% 이상** 달성되면 2막 전환 트리거

### 2막 전환 연출
안락도 80% 도달 시:
1. 자막: *"...라디오에서 잡음이 들려온다"*
2. 잠시 뒤 자막: *"여기는 구조대... 좌표를 보내라. 반복한다, 좌표를..."*
3. 자막: *"...언덕 위 통신 중계기에 가면 좌표를 보낼 수 있을지도 모른다."*
4. 맵 **가장 먼 곳**에 통신 중계기 오브젝트 스폰 (빛나는 이펙트로 위치 힌트)

### 2막: 마지막 출격 (최종 미션)
이 출격은 특별 사양을 강제 적용:
- `coldMultiplier` → 최대치 강제
- 블리자드 웨이브 간격 → 극단적으로 짧게 (15초 간격)
- 환각 시스템 → 평소보다 일찍 발동
- **핵심:** 체온이 0이 되어도 즉사하지 않고, 환각 상태로 약간의 시간을 더 버틸 수 있음 (라스트 찬스)

### 엔딩 연출 (중계기 도착 후 상호작용 시)
1. 조작 마비 (GameDirector 방식 재활용)
2. 자막: *"...신호가 발사됐다..."*
3. FogManager 안개 밀도 → 서서히 0으로 (블리자드가 멎음)
4. 환경광 색상: 차가운 파란색 → 따뜻한 주황색 (새벽)
5. 자막: *"...멀리서 헬기 소리가 들려온다..."*
6. 카메라가 천천히 아래로 기울며 (쓰러지는 연출)
7. 화면 서서히 하얗게 암전 (검은색이 아님 — 빛 속으로 의식을 잃는 느낌)
8. **PROLOGUE END** 타이틀 표시

### 구현 시 필요한 스크립트
- **EndingManager.cs** (신규): 2막 전환 감지, 중계기 스폰, 최종 출격 조건 세팅, 엔딩 연출 코루틴
- **CabinComfort.cs** (수정): 안락도 80% 도달 이벤트 콜백 추가
- **SurvivalTimer.cs** (수정): 2막에서 체온 0 도달 시 즉사 대신 "라스트 찬스" 모드 진입
- **FogManager.cs** (수정): 엔딩 시 안개 해제 메서드 추가
- **AtmosphereManager.cs** (수정): 엔딩 시 조명 색상 전환 메서드 추가

### 씬에서 필요한 것
- 통신 중계기 프리팹 (간단한 안테나 모양 + Point Light)
- 헬기 소리 오디오 클립
- 신호탄 발사 효과음
- "PROLOGUE END" 표시용 UI 텍스트

### 연출 의도
> 쓰러지는 이유: 긴장이 풀리며 동사 직전까지 갔던 몸이 무너지는 것.
> 구조는 됐지만 의식을 잃는 연출 = "프롤로그"로서 다음 이야기(본편)를 암시.
> 플레이어에게 "이 사람은 과연 살아남은 걸까?" 라는 여운을 남김.


---

## 🚀 추가할 기능 (집에서 구현)

### 엔딩 시스템: "새벽이 온다 — 마지막 출격"

**2막 구조 엔딩:**

**1막(생존):** 기존 게임 루프. 가구를 모아 별장 안락도(CabinComfort)를 80% 이상 채운다.

**2막(탈출) 트리거 조건:** 안락도 80% 달성 시 자동 발동.
- 라디오 교신 자막: *"여기는 구조대... 좌표를 보내라. 반복한다, 좌표를..."*
- 후속 자막: *"...언덕 위 통신 중계기에 가면 좌표를 보낼 수 있을지도 모른다."*
- 맵 **가장 먼 곳**에 통신 중계기 오브젝트 스폰

**마지막 출격 특별 사양:**
- coldMultiplier 최대치 강제 적용
- 블리자드 웨이브 간격 극단적으로 짧게 (15초 간격)
- 체온 0이 되어도 즉사하지 않고 환각 상태로 잠시 버틸 수 있음 (라스트 찬스)
- 중계기에 도착 → E키 상호작용 → 신호 발사

**엔딩 연출 순서:**
1. 자막: *"..발사됐다...제발 빨리"*
2. 블리자드 서서히 멎음 (FogManager 안개 밀도 → 0)
3. 화면 색감이 차가운 파란색 → 따뜻한 주황색 (새벽)
4. 자막: *"...멀리서 헬기 소리가 들려온다..."*
5. 캐릭터가 눈밭에 쓰러지며 암전 → **PROLOGUE END**
6. 쓰러지는 이유: 긴장이 풀리며 동사 직전까지 갔던 몸이 무너짐. 구조는 됐지만 의식을 잃음 = 본편 암시.
