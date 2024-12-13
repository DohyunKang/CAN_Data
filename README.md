### CAN 데이터 디코딩 프로그램 (CAB 500 센서 데이터 처리)

#### 📋 프로젝트 개요
이 프로그램은 **PCAN USB 모듈**을 사용하여 **CAB 500 센서**로부터 CAN 데이터를 수신하고, 해당 데이터를 디코딩하여 **전류값**으로 변환한 후 UI에 출력합니다.

---

### ⚙️ 주요 기능

1. **CAN 초기화**:
   - PCAN USB 모듈을 `Baudrate: 500Kbps`로 초기화.
   - CAN 데이터 수신 준비.

2. **전류값 디코딩**:
   - 센서로부터 수신한 데이터 중 상위 4바이트를 Big-Endian에서 Little-Endian으로 변환.
   - 변환된 값을 기반으로 **스케일링**하여 전류값 계산.

3. **실시간 데이터 업데이트**:
   - 디코딩된 데이터를 UI의 **ListBox**와 **그래프**에 표시.
   - 스크롤은 자동으로 최신 데이터로 이동.

4. **CAN 메시지 디스플레이**:
   - 수신된 CAN 메시지를 ID, 데이터 길이, 데이터, 주기와 함께 UI에 표시.

---

### 📚 함수 설명

#### 1. `InitializeCAN()`
CAN 통신 초기화:
- **입력**: 없음
- **출력**: 초기화 상태 메시지
- **설명**: PCAN 모듈을 초기화하여 CAN 통신 준비를 수행.

#### 2. `DecodeCurrentValue(TPCANMsg message)`
CAN 데이터 디코딩:
- **입력**: `TPCANMsg` (수신된 CAN 메시지)
- **출력**: 디코딩된 전류값 (단위: A)
- **설명**:
  - 센서 ID가 `0x3C2`일 경우 데이터 디코딩.
  - 상위 4바이트를 Little-Endian으로 변환하여 전류값을 계산.
  - 스케일링 값: 0.1mA -> A.

#### 3. `UpdateCurrentValue(double currentValue)`
전류값 UI 업데이트:
- **입력**: 디코딩된 전류값
- **출력**: 없음
- **설명**:
  - ListBox와 그래프에 실시간으로 값 추가.
  - 100개 이상 항목이 쌓일 경우 가장 오래된 값 제거.

#### 4. `UpdateCANData(TPCANMsg message, int period)`
CAN 메시지 UI 업데이트:
- **입력**: `TPCANMsg` (수신된 메시지), 데이터 주기 (ms)
- **출력**: 없음
- **설명**:
  - 수신된 CAN 데이터를 ListBox에 표시.
  - 데이터 항목이 100개를 초과하면 오래된 값 삭제.

#### 5. `ReadCANData()`
CAN 데이터 읽기:
- **입력**: 없음
- **출력**: 없음
- **설명**:
  - PCAN 모듈로부터 데이터를 읽어 `DecodeCurrentValue()`와 `UpdateCANData()` 호출.

#### 6. `button1_Click()`
시작 버튼 이벤트:
- **입력**: 없음
- **출력**: 없음
- **설명**:
  - CAN 초기화 및 데이터 읽기 스레드 시작.

#### 7. `button2_Click()`
종료 버튼 이벤트:
- **입력**: 없음
- **출력**: 없음
- **설명**:
  - 데이터 읽기 스레드 종료 및 CAN 통신 종료.

---

### 📊 UI 구성
1. **전류값 ListBox**: 디코딩된 전류값을 실시간으로 표시.
2. **CAN 메시지 ListBox**: 수신된 CAN 데이터를 실시간으로 표시.
3. **그래프**: 전류값을 실시간으로 시각화.

---

### 🛠️ 설치 및 실행 방법
1. **PCAN 드라이버 설치**:
   - PEAK-System의 PCAN 드라이버 설치.
   - [드라이버 다운로드](https://www.peak-system.com/)

2. **CAB 500 센서 연결**:
   - 센서를 PCAN USB 모듈에 연결.
   - 센서 설정값 확인(ID: `0x3C2`).

3. **프로그램 실행**:
   - 프로그램 실행 후 **Start** 버튼 클릭.
   - 데이터를 확인한 후 **Stop** 버튼으로 종료.

---

### 🔍 예제 출력
#### CAN 메시지 출력 예시
```
ID: 3C2, Len: 8, Data: 80-00-00-00-00-00-00-00, Period: 100 ms
```

#### 전류값 출력 예시
```
Raw Value: 2147483648, Offset Applied: 0, Scaled Value: 0.00 A
```

---

### 🖼️ UI 예시
<img width = 700 src = "https://github.com/user-attachments/assets/2ce3394e-267b-4616-a5a0-5776c68e5c50">

---

### 🖼️ 센서 연결
<img width = 500 src = "https://github.com/user-attachments/assets/a386d56d-e5ce-4f6c-bafe-c252db7c2b61">

---


### 🖼️ PCAN USB 모듈
<img width = 500 src = "https://github.com/user-attachments/assets/ceeaa16a-ec33-4bae-bfc7-e73a629d4ba5">

---

### 매뉴얼 & 데이터시트
[cab_500-c_sp5_public_datasheet.pdf](https://github.com/user-attachments/files/18122805/cab_500-c_sp5_public_datasheet.pdf)

[user_guide_-_cab_500-v1.pdf](https://github.com/user-attachments/files/18122807/user_guide_-_cab_500-v1.pdf)

[PCAN-USB-Pro_UserMan_eng.pdf](https://github.com/user-attachments/files/18122811/PCAN-USB-Pro_UserMan_eng.pdf)
