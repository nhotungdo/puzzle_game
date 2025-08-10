## Puzzle Game — C# Windows Forms

> Port từ bản HTML/JS sang C# (.NET 8, WinForms). Giao diện khớp canvas 600x540 của `index.html`.

### Yêu cầu hệ thống
- **Hệ điều hành**: Windows 10/11
- **.NET SDK**: `8.0` trở lên (`https://dotnet.microsoft.com/download`)
- Tùy chọn: Visual Studio 2022 / Rider / VS Code (C# Dev Kit)

### Cấu trúc dự án
```
PuzzleGame/
└─ puzzle_game/
   ├─ PuzzleGame.csproj
   ├─ Program.cs
   ├─ MainForm.cs
   ├─ README.md (tệp này)
   ├─ index.html (tham khảo)
   ├─ script.js (tham khảo)
   ├─ pieces.js (tham khảo)
   └─ styles.css (tham khảo)
```

### Chạy bằng CLI
1. Mở terminal tại `puzzle_game`.
2. Biên dịch:
   ```bash
   dotnet build
   ```
3. Chạy:
   ```bash
   dotnet run
   ```

### Chạy bằng Visual Studio
- Mở `puzzle_game/PuzzleGame.csproj`
- Chọn `Debug` → Run (F5)

### Điều khiển
- Trái/Phải: di chuyển ngang
- Lên: xoay khối
- Xuống: rơi nhanh (nhả phím để trở lại tốc độ thường)
- R: reset game

### Build phát hành
- Windows x64, không tự chứa:
  ```bash
  dotnet publish -c Release -r win-x64 --self-contained false
  ```
- Single-file tự chứa:
  ```bash
  dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishTrimmed=false
  ```
  Output tại `bin/Release/net8.0-windows/win-x64/publish`.

### Ghi chú
- Form cố định 600x540, căn giữa, double-buffering để vẽ mượt.
- Các tệp HTML/CSS/JS gốc giữ làm tham khảo.

### Đóng góp
- Tạo issue hoặc pull request trên GitHub.

### License
- Chưa khai báo. Thêm `LICENSE` nếu cần phân phối công khai.
