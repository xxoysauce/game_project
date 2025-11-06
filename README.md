# 🐰 Hello_World!: LLM 기반 다이나믹 대화 시스템 (Unity/OpenAI)


## 😈 문제 상황

<p align="center"><img width="868" height="874" alt="KakaoTalk_Photo_2025-11-06-20-43-38" src="https://github.com/user-attachments/assets/be4bf3d2-471e-4aec-9c14-91b60d788292" /></p>

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
├─ village scene             
├─ Player
├─ NPC1_Happy
├─ NPC2_Crying
├─ NPC3_Happy
├─ NPC4_Crying
├─ NPC5_Idle
├─ NavMeshRoot                
├─ Canvas
│  └─ talkPanel               
│      ├─ NameText
│      ├─ BodyText
│      └─ Buttons
│         ├─ NextBtn
│         ├─ OptionA
│         │   └─ Text (TMP)
│         └─ OptionB
│             └─ Text (TMP)
├─ DialogueSystem             
└─ ApiManager               
 ```


## 💡 핵심 아키텍처 및 LLM 연동 전략


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




## 🏠구현 기능

- NPC — NPCWander로 자유롭게 이동

- 대화 시 NPC / 플레이어 정지

- 플레이어 방향키로 이동

- 사과 프리펩과 충돌 시 +1 / NPC 퀘스트 완료 시 긍정의 말 출력

- E키 접근 시 대화창 등장 / Q키 선택 시 대화창 종료 및 작별인사 출력

- NPC 대사 → 다음 버튼 → 선택지 표시(이지선다)

- 선택 시 OpenAI 응답 표시 (LLM 대화 연결)

- 시간의 흐름에 따라 바뀌는 SKYBOX 적용

- NPC마다 다른 프롬프팅으로 퀘스트를 많이 주는 NPC, 상담을 요청하는 NPC, 퀴즈를 내는 NPC 등으로 구성


----
### 🌳구현 화면

<p align="center"><img width="1440" height="755" alt="스크린샷 2025-11-06 14 40 15" src="https://github.com/user-attachments/assets/f148f993-54c2-46d1-a86b-33f9655c1b47" /></p>

<p align="center"><img width="1438" height="742" alt="스크린샷 2025-11-06 14 41 23" src="https://github.com/user-attachments/assets/9cff98cb-0970-4a9f-bdc9-da83e43917f4" /></p>

- 시작씬

<p align="center"><img width="1440" height="747" alt="스크린샷 2025-11-06 14 41 39" src="https://github.com/user-attachments/assets/995a13ea-776a-435f-88d7-8d95907ce1b4" /></p>

 - 플레이어 구동 및 NPC AI manegement 사용해 자유 이동

<p align="center"><img width="1439" height="757" alt="스크린샷 2025-11-06 14 41 57" src="https://github.com/user-attachments/assets/4f01014f-3966-4214-8bee-6724482cbcc6" /></p>

- 바닥에 떨어져 있는 사과 프리펩들


<p align="center"><img width="689" height="656" alt="quest" src="https://github.com/user-attachments/assets/67ff00a9-cadd-4a75-b905-33f89a478bfc" /></p>

- 퀘스트 로그

  
<p align="center"><img width="1282" height="515" alt="스크린샷 2025-11-06 14 38 07" src="https://github.com/user-attachments/assets/260a1e9b-5794-47b7-88c9-e0174ce03c31" /></p>
<p align="center"><img width="1265" height="584" alt="스크린샷 2025-11-06 14 38 18" src="https://github.com/user-attachments/assets/5b3770a4-5f3a-4f53-aeeb-c06c739ddb1e" /></p>
<p align="center"><img width="734" height="464" alt="스크린샷 2025-11-06 15 11 47" src="https://github.com/user-attachments/assets/1379259e-34ff-422c-af35-c6c82936017e" /></p>

- 대화창 및 API 통신 로그



  <p align="center"><img width="638" height="637" alt="스크린샷 2025-11-06 16 31 50" src="https://github.com/user-attachments/assets/4f84d0c1-a6c6-4255-8c81-16afd82e217a" />></p>

- 인스펙터 내에서 수정할 수 있는 플레이어의 프롬프트

  <p align="center"><img width="570" height="488" alt="스크린샷 2025-11-06 20 26 56" src="https://github.com/user-attachments/assets/ce769c79-447e-42e0-8625-cfdfa5ba2a92" /></p>

- 인스펙터 내에서 수정할 수 있는 각각 NPC 들의 프롬프트

<p align="center"><img width="664" height="268" alt="스크린샷 2025-11-06 15 12 40" src="https://github.com/user-attachments/assets/f4ef0534-9858-42b2-bad4-106b891ec28b" /></p>

- 히스토리 초기화 및 저장
