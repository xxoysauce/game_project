# 🐰 Hello_World!: LLM 기반 다이나믹 대화 시스템 (Unity/OpenAI)


## 😈 문제 상황
- 덮어쓰기: 유니티는 Assets 폴더 내 C# 파일이 바뀌면 자동으로 컴파일을 시도하지만, 덮어쓰기 과정에서 컴파일 캐시 (Library 폴더) 자체가 꼬여버림.

- 복구 불가: Library 폴더나 씬 파일 내부의 연결 데이터는 수동으로 YAML 파일을 열어 UID를 맞추지 않는 한 복구가 거의 불가능


## 🌟 프로젝트 소개


이 프로젝트는 **Generative AI (OpenAI Chat API)** 를 게임 시스템에 통합하여, 플레이어의 상황과 맥락에 따라 실시간으로 반응하는 **' NPC 대화 시스템'** 을 구축한 Unity 기반 프로토타입입니다.

NPC 단순한 스크립트 재생을 넘어, 대화 히스토리, 퀘스트 상태, 캐릭터 페르소나를 모두 고려하여 인간적인 상호작용을 제공하며, AI가 직접 게임의 로직을 주도하도록 설계되었습니다.

참고: 현재 프로젝트 파일은 소실되었으나, 모든 핵심 코드 및 실행 결과는 스크린샷으로 기록되어 있습니다. 본 문서는 기록된 스크린샷을 기반으로 작성되었습니다.

## 🧩 프로젝트 목표 및 기대 효과

- 대화 확장성: 스크립트 작성 없이 LLM을 통해 모든 상황(퀘스트, 아이템 증정, 잡담)에 실시간으로 대응하는 NPC 구현.

- AI 주도형 게임 로직: AI가 단순히 대화만 하는 것이 아니라, 게임 상태를 인지하고 직접 퀘스트를 부여하고 완료를 판별하는' NPC를 구현.

## 🛠️ 기술 스택 및 환경

- **게임 엔진** : Unity (C#)

- **LLM 통합** : OpenAI Chat Completions API (gpt-3.5-turbo)

- **네트워크** : C# HttpClient (비동기 처리) / Unity UnityWebRequest

- **데이터 처리** : System.Text.Json (JSON 직렬화/역직렬화)

- **프롬프팅 기법** : Few-shot Prompting, JSON Schema Enforcement

## 파일 구조(Scene -> Hierarchy)

  ```
IslandScene
├─ Main Camera
├─ Directional Light
├─ village scene              ← 배경/지형 프리팹
├─ Player
├─ NPC1_Happy
├─ NPC2_Crying
├─ NPC3_Happy
├─ NPC4_Crying
├─ NPC5_Idle
├─ NavMeshRoot                ← NavMesh Surface 있는 오브젝트
├─ Canvas
│  └─ talkPanel               ← 여기서 실제 대화 UI 나옴
│      ├─ NameText
│      ├─ BodyText
│      └─ Buttons
│         ├─ NextBtn
│         ├─ OptionA
│         │   └─ Text (TMP)
│         └─ OptionB
│             └─ Text (TMP)
├─ DialogueSystem             ← DialogueManager 붙어 있는 오브젝트였을 때 이름
└─ ApiManager                 ← OpenAIConnector 붙여둔 빈 오브젝트
 ```


## 💡 핵심 아키텍처 및 LLM 연동 전략

본 프로젝트의 가장 큰 기술적 성과는 LLM을 단순한 챗봇이 아닌, 게임 내 액션과 로직을 결정하는 엔진으로 활용한 점입니다.

**1. JSON Schema 기반 구조화된 응답 강제** 

LLM이 NPC 대사(npc)뿐만 아니라 플레이어에게 제공할 다음 **선택지 2개(option_a, option_b)** 까지 단일 JSON 형식으로만 출력하도록 system prompt에 강제했습니다.


**요청 JSON 구조** :

  ```
{
    "model": "gpt-3.5-turbo",
    "messages": [
        {"role": "system", "content": "..."} // NPC 페르소나 및 JSON 형식 강제
        // ... conversationHistory ...
        {"role": "user", "content": "..."} // 플레이어의 현재 선택 또는 행동
    ]
}

```

**2. LLM과 게임 상태 동기화 (State-Awareness)**

OpenAIConnector는 API를 호출하기 직전, QuestManager의 데이터를 읽어 현재 게임 상태를 프롬프트에 동적으로 삽입합니다.

- 퀘스트가 비활성화 상태일 때 (QuestManager.questIsActive == false), LLM은 "사과 3개 모으기" 퀘스트를 강제로 부여합니다.

**3. 대화 맥락 유지 (Conversation History Management)**

NPC가 플레이어의 이전 질문이나 선택을 잊지 않도록, 모든 API 호출 시 conversationHistory 리스트의 내용을 통째로 전송하여 LLM의 **'기억(Context Window)'** 을 유지했습니다.


## 구현 기능



NPC — NPCWander로 자유롭게 이동

E키 접근 시 대화창 등장

NPC 대사 → 다음 버튼 → 선택지 표시

선택 시 OpenAI 응답 표시 (LLM 대화 연결)
