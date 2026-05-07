# Snow-Crawler: Prologue -- Dev Handoff

AI가 이 파일을 읽으면 개발 맥락을 즉시 파악하고 작업을 이어갈 수 있습니다.

## 마지막 업데이트
- 날짜: 2026-05-07 (수), 작업 위치: 학교 PC

---

## [필독] 씬 파일 동기화 문제

집 PC의 SampleScene.unity 파일이 GitHub에 커밋되지 않은 상태입니다.
집 PC 씬 = 최신 버전 (테스트 가구 삭제됨, ItemSpawner 있음)
학교 PC 씬 = 오래된 버전 (테스트 가구 살아있음, ItemSpawner 없음)

### 집에 도착하면 가장 먼저 할 것
1. git pull (오늘 학교에서 수정한 스크립트 받기)
2. Unity 열고 씬 덮어쓰지 말기
3. Ctrl+S 로 씬 저장 후 git add . -> git commit -> git push
4. 이후로는 씬 파일도 항상 커밋에 포함!

---

## 오늘 학교에서 구현한 것 (100% 코드 기반 안전 작업)

### 1. SaveManager.cs (신규 파일) + SortieManager 자동저장 연동 완료!
- 씬에 배치된 모든 가구, 생존 상태, 출격 횟수 저장 로직.
- **SortieManager.cs 스크립트 수정 완료:** 별장에 귀환하면 자동으로 저장되도록 코드 구현 끝남!

### 2. FootstepSound.cs 고도화 (바닥 재질별 발소리 분리)
- 기존 단일 발소리에서 **눈밭(Snow)과 나무바닥(Wood)**으로 발소리를 분리하는 코드 작성 완료.
- 발 밑으로 레이저(Raycast)를 쏴서 밟고 있는 바닥의 Tag를 검사하도록 구현.
- 유니티 ProjectSettings에 **`Wood` 태그 자동 추가 완료!** (집에 가면 태그가 생겨있음)

### 3. SubtitleManager.cs (독백 자막 시스템 신규 추가)
- **영화 같은 페이드인/아웃 연출**과 **대기열(Queue)**을 지원하는 자막 매니저 생성.
- **이미 연동 완료된 상황:**
  1. 첫 출격 시: "눈보라가 거세다... 필요한 가구만 빠르게 챙겨야 해."
  2. 3번째 출격 시: "점점 더 추워지는 기분이야... 너무 오래 밖을 맴돌면 위험해."
  3. 얼어 죽기 직전(체온 25% 이하): "너무 춥다... 손발의 감각이 사라지고 있어..." (긴급 자막 덮어쓰기 적용)
  4. 가방 꽉 찼는데 주울 때: "가방이 꽉 차서 더 이상 주울 수 없어..."
- **집에서 씬에 세팅하는 법:**
  1. 빈 게임오브젝트 생성 후 `SubtitleManager.cs` 부착.
  2. Canvas 밑에 화면 아래쪽에 적당히 TextMeshPro-Text(UI) 생성 후 폰트 설정.
  3. SubtitleManager의 `subtitleText` 칸에 그 텍스트를 드래그해서 넣으면 끝!

---

## 씬에서 아직 해야 할 것 (집에서 할 일)

1. **SaveManager 및 SubtitleManager 오브젝트 씬에 배치**
   - 빈 게임오브젝트 2개 만들고 각각 스크립트 부착. SubtitleManager는 UI 텍스트 연결!
2. **PauseMenu UI & Journal UI 배치**
   - Canvas 안에 패널 배치 (ESC 일시정지, Tab 메모지)
3. **오디오 에셋 연결 (BGM, 눈보라, 심박수)**
   - `ZoneAudio`, `TensionAudio`에 다운받은 클립 넣기
4. **FrostOverlay 이미지 연결**
   - 체온 낮을 때 서리 효과 이미지 할당
5. **발소리(FootstepSound) 세팅 완료하기**
   - 별장 마룻바닥(Floor) 오브젝트들의 Tag를 `Wood`로 변경.
   - 플레이어 오브젝트에 있는 `FootstepSound` 스크립트 빈칸(`snowClips`, `woodClips`)에 다운받은 소리 파일 드래그 앤 드롭.

---

## 다음 작업 우선순위

1순위: 씬을 열자마자 바로 저장(Ctrl+S) 후 커밋해서 씬 동기화부터 완료하기
2순위: SaveManager, SubtitleManager 씬 배치 및 UI 연결
3순위: UI 씬 배치 (PauseMenu, JournalUI)
4순위: 발소리 및 배경음악 에셋 연결

---

## Git 주의사항

작업 전: git pull 먼저!
작업 후: Ctrl+S (씬 저장) -> git add . -> git commit -> git push
씬 파일(.unity)이 커밋 목록에 포함되어 있는지 반드시 확인!

---

## 집에서 Antigravity 켤 때 할 말

'DEV_HANDOFF.md 파일 내용 분석해서 숙지해줘.
 그 다음 1순위(씬 저장 동기화)부터 순서대로 작업 이어서 진행해줘.'
