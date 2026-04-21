<div align="center">

# Glow Me Up

**손그림을 스캔해 AI 아이돌 스타일 이미지로 변환하는 체험형 프로그램**

<br/>

<img src="https://img.shields.io/badge/Engine-Unity-black?style=for-the-badge" />
<img src="https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge" />
<img src="https://img.shields.io/badge/Library-OpenCvSharp-5C3EE8?style=for-the-badge" />

</div>

---

## 프로젝트 소개

**Glow Me Up**은 사용자가 직접 그린 캐릭터 이미지를 스캐너로 촬영한 뒤,  
AI를 통해 아이돌 콘셉트의 결과 이미지로 변환해주는 체험형 프로그램입니다.

사용자는 메인 화면의 안내에 따라 체험을 시작하고, 스캔한 이미지를 확인한 뒤  
닉네임을 입력하여 결과를 생성할 수 있습니다.  
완성된 결과물은 현장 대형 스크린과 QR 기반 모바일 홈페이지를 통해 함께 확인할 수 있도록 구성하였습니다.

- 웹 페이지 GitHub: https://github.com/newstartak/GlowMeUpWeb

---

## 주요 화면

<table>
  <tr>
    <th>메인 화면</th>
    <th>카메라 화면</th>
  </tr>
  <tr>
    <td align="center">
      프로그램 진행 단계와 체험 흐름을 안내하는 시작 화면
      <br/><br/>
      <img width="260" alt="메인 화면" src="https://github.com/user-attachments/assets/978dde07-6643-45d7-a5ec-c47092c0458a" />
    </td>
    <td align="center">
      사용자가 그린 그림을 스캐너로 확인하고 촬영하는 화면
      <br/><br/>
      <img width="260" alt="카메라 화면" src="https://github.com/user-attachments/assets/e959b1a3-bd24-4b72-8140-afa69210a0e9" />
    </td>
  </tr>
  <tr>
    <th>키보드 입력 화면</th>
    <th>결과 안내 화면</th>
  </tr>
  <tr>
    <td align="center">
      가상 키보드를 통해 닉네임을 입력하는 화면
      <br/><br/>
      <img width="260" alt="키보드 입력 화면" src="https://github.com/user-attachments/assets/2736070b-4c50-44c1-9a8e-9bc1d854fa29" />
    </td>
    <td align="center">
      결과 확인 및 QR 스캔 동선을 안내하는 완료 화면
      <br/><br/>
      <img width="260" alt="결과 안내 화면" src="https://github.com/user-attachments/assets/f8c4b280-9bb7-4feb-af4e-8ad90d893632" />
    </td>
  </tr>
  <tr>
    <th colspan="2">실제 운영 현장</th>
  </tr>
  <tr>
    <td colspan="2" align="center">
      그라운드시소 싱가포르 현장에서 태블릿과 스캐너를 함께 배치하여 사용자가 직접 그림을 그리고,
      스캔 후 AI 결과를 확인할 수 있도록 운영한 모습
      <br/><br/>
      <img width="500" alt="실제 운영 현장" src="https://github.com/user-attachments/assets/0debc773-f734-441a-8455-f39a0039cf64" />
    </td>
  </tr>
</table>

---

## 담당 역할

### 유니티 기반 응용 프로그램 개발
- 프로그램 초기화 및 무거운 작업 구간에 비동기 로직 적용
- 닉네임 입력을 위한 가상 키보드 기능 구현
- 체험형 키오스크 환경에 맞춘 전체 사용자 흐름 및 화면 전환 로직 구현

### 스캐너 화면 표시
- OpenCvSharp를 활용하여 스캐너 입력 화면을 프로그램 내에 표시
- 사용자가 스캔 대상을 직관적으로 확인할 수 있도록 미리보기 화면 구성

### HTTP 서버 이미지 전달
- 스캐너로 촬영한 이미지를 HTTP POST 방식으로 AI 서버에 전송
- AI 변환 프로세스와 연결될 수 있도록 이미지 전달 흐름 구현

---

## 기술 스택

- Unity
- C#
- OpenCvSharp

---

## 구현 포인트

### 스캐너 영상 성능 최적화
- Unity 기본 제공 API인 WebCamTexture의 경우 캡처되는 영상의 확장자나 미디어 프레임워크의 변경이 불가능해 고화질의 스캐너 영상을 받아오는데 적합하지 않았습니다.
- OpenCvSharp를 활용해 영상 확장자 변경, 캡처 API를 DirectShow로 변경해 최적화하였습니다.

### 스캐너 리소스 재사용 구조 적용
- 스캐너 인스턴스를 매번 완전히 종료하고 다시 생성하는 방식 대신, 백그라운드에 유지한 뒤 필요한 시점에 다시 활성화하는 구조로 설계하였습니다.
- 이를 통해 초기화 시간을 줄이고 전체 체험 흐름이 끊기지 않도록 최적화하였습니다.

### 비동기 처리 기반 사용자 경험 개선
- 프로그램 초기화, 이미지 처리, 서버 통신 등 시간이 걸리는 작업에 비동기 로직을 적용하였습니다.
- 메인 흐름이 멈추지 않도록 구성하여 체험 과정이 자연스럽게 이어지도록 구현하였습니다.

### 키오스크 환경 대응 입력 UX 구현
- 물리 키보드 없이도 독립적으로 동작할 수 있도록 가상 키보드를 구현하였습니다.
- 전시 및 체험형 환경에 적합한 입력 방식을 구성하여 사용 편의성을 높였습니다.
