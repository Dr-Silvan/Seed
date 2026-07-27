Seed v0.1

Seed는 끊고 싶은 습관으로부터 경과한 시간을 식물의 성장으로 보여주는 Windows 앱입니다.

## 포함된 기능

- 실시간 경과 일수·시간 표시
- 3/7/14/30/60/90/180/365일 성장 단계
- 코드로 그린 벡터 식물과 흔들림·개화·시듦 애니메이션
- 실패 원인과 메모 기록, 자동 재시작
- 365일 캘린더
- 가장 흔한 실패 원인과 개선 제안
- 60초 호흡, 동기 문구, 음악·영상·명상 링크
- 화면 좌하단 투명 식물 위젯
- Windows 로그인 시 식물 위젯 자동 실행
- 메인 창을 닫아도 트레이에서 백그라운드 실행
- 트레이 아이콘 더블클릭으로 다시 열기, 우클릭으로 완전 종료
- Seed 전용 앱·트레이 아이콘
- 사용자 로컬 JSON 저장

백그라운드 실행

식물 위젯이 켜진 상태에서 메인 창의 × 버튼을 누르면 Seed는 종료되지 않습니다.
작업 표시줄 오른쪽 알림 영역의 Seed 아이콘을 더블클릭하면 메인 창이 다시 열립니다.
완전히 종료하려면 Seed 아이콘을 우클릭한 뒤 "완전 종료"를 선택하세요.

## 개발 실행

```powershell
dotnet run
```

## Windows EXE 만들기

```powershell
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DebugType=None -o "dist\Seed-v0.1"
```

완성된 실행 파일은 `dist\Seed-v0.1\Seed.exe`입니다.

## 애니메이션 확장

현재 식물은 `Controls/PlantView.cs`가 직접 렌더링합니다. 추후 Rive/Lottie 같은
외부 애니메이션을 도입할 때 이 컨트롤만 동일한 `Level` 인터페이스를 가진 새 컨트롤로
교체하면 나머지 기록·성장·캘린더 로직은 유지됩니다.
