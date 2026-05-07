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
1. git pull (오늘 학교에서 추가한 SaveManager.cs 스크립트 받기)
2. Unity 열고 씬 덮어쓰지 말기
3. Ctrl+S 로 씬 저장 후 git add . -> git commit -> git push
4. 이후로는 씬 파일도 항상 커밋에 포함!

---

## 오늘 학교에서 구현한 것

### 1. SaveManager.cs (신규 파일)
경로: Assets/Scripts/SaveManager.cs

저장 내용:
- 씬에 배치된 모든 가구 (위치, 회전, 크기, ItemType)
- 생존 상태 (upgradeLevel, materialCount, maxTime, coldMultiplier)
- 출격 횟수 (SortieCount)

저장 위치: Application.persistentDataPath/save.json

집에서 씬에 붙이는 법:
1. 빈 게임오브젝트 생성, 이름: SaveManager
2. SaveManager.cs 드래그해서 붙이기
3. Inspector 비워도 됨 (자동 탐색)
4. SortieManager.cs의 OnSortieReturn() 메서드 끝에 아래 한 줄 추가:
   if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();

### 2. SortieManager.cs 수정
- public int SortieCount { get; set; } 프로퍼티 추가됨

---

## 현재 전체 스크립트 현황 (30개 전부 완성)

PlayerController, PlayerInventory, FurnitureDrop, FurniturePlacement
SurvivalTimer, SortieManager, ItemSpawner, FurnitureItem, ItemDatabase
AtmosphereManager, FogManager, CabinComfort, ZoneAudio, TensionAudio
WeightHUD, InteractionPrompt, CabinCompass, JournalUI, PauseMenu
MainMenuManager, MenuCameraOrbitDriver, FootstepSound
FlashlightController, SnowParticleKill, CameraBob
GameProgress, FurniturePackFixer, FurniturePostprocessor
SaveManager (오늘 추가!)

---

## 씬에서 아직 해야 할 것 (집에서 할 일)

1. SaveManager 오브젝트 씬에 배치 + 자동 저장 연동
2. PauseMenu UI 패널 Canvas 안에 배치 (ESC 일시정지 화면)
3. JournalUI 패널 Canvas 안에 배치 (Tab 메모지 화면)
4. ZoneAudio, TensionAudio 에 오디오 클립 연결 (BGM, 눈보라, 심박수)
5. FrostOverlay 이미지 연결 (체온 낮을 때 서리 효과)

---

## 다음 작업 우선순위

1순위: SaveManager 씬 배치 + SortieManager 자동 저장 연동
2순위: PauseMenu UI 씬 배치
3순위: JournalUI 씬 배치
4순위: 오디오 에셋 연결

---

## Git 주의사항

작업 전: git pull 먼저!
작업 후: Ctrl+S (씬 저장) -> git add . -> git commit -> git push
씬 파일(.unity)이 커밋 목록에 포함되어 있는지 반드시 확인!

---

## 집에서 Antigravity 켤 때 할 말

'DEV_HANDOFF.md 파일 내용 분석해서 숙지해줘.
 그 다음 SaveManager 씬 배치 및 자동 저장 연동 작업부터 이어서 진행해줘.'

---
이 파일은 작업 세션이 끝날 때마다 업데이트 예정.
