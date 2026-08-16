# Gem Collector

Game thu thập gem góc nhìn thứ ba được xây dựng bằng Unity cho bài test Eight Unity Developer.

## Tổng Quan

Người chơi bắt đầu trong một scene landscape, xem đoạn camera intro ngắn bay quanh map, sau đó điều khiển nhân vật góc nhìn thứ ba bằng joystick ảo. Gem sẽ sinh ngẫu nhiên trên mặt sân theo thời gian. Người chơi di chuyển, leo qua các vật cản thấp, tấn công các gem ở gần để thu thập, và chiến thắng khi đạt đủ mốc điểm yêu cầu.

## Điều Khiển

- Joystick ảo: di chuyển nhân vật theo hướng nhìn của camera
- Vuốt trên vùng gameplay: xoay camera góc nhìn thứ ba
- Hỗ trợ multi-touch: giữ joystick bằng một ngón và xoay camera bằng ngón khác
- Nút Attack: chạy animation tấn công và thu thập gem gần nhất trong phạm vi đánh
- Nút Reset: tải lại scene hiện tại và xóa điểm đã lưu

## Yêu Cầu Đã Hoàn Thành

- Màn hình gameplay landscape
- Camera intro bay quanh map, sau đó blend mượt về sau lưng player
- Camera follow góc nhìn thứ ba, có thể xoay bằng thao tác vuốt
- Player di chuyển bằng joystick ảo theo hướng camera
- Điều khiển animation Idle và Run
- Giới hạn vị trí player trong phạm vi map
- Phát hiện vật cản thấp và chạy animation leo trèo
- Có hành động Attack trong khi vẫn cho phép di chuyển
- Gem sinh ngẫu nhiên theo thời gian
- Sử dụng object pooling cho gem
- Nhiều loại gem với điểm số và tỉ lệ spawn khác nhau
- Animation gem bay về icon UI khi thu thập
- Lưu điểm bằng `PlayerPrefs`
- Điều kiện thắng khi đạt mốc điểm `10`
- Hiển thị panel thắng và particle `ConfettiBlastRainbow`
- Có nút Start và Reset

## Ghi Chú Thiết Kế

- `GameManager` quản lý các trạng thái game: chờ bắt đầu, intro, đang chơi, thắng.
- `ScoreManager` quản lý điểm, số gem đã thu thập, điểm mục tiêu và sự kiện thắng.
- `SaveManager` bọc logic lưu dữ liệu bằng `PlayerPrefs`.
- `GemPool` tránh việc instantiate gem lặp lại trong lúc chơi.
- `GemFactory` chọn loại gem bằng weighted random.
- `Gem` dùng `MaterialPropertyBlock` để đổi màu và emission riêng cho từng instance mà không tạo thêm bản sao material runtime.
- `CameraController` bỏ qua các touch nằm trên UI, nên thao tác joystick không chặn việc xoay camera bằng ngón khác.
