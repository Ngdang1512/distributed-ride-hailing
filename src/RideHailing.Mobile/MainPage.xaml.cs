using System.Net.Http.Json;
using System.Text.Json;
// Đã xóa các thư viện Map để tránh lỗi

namespace RideHailing.Mobile;

public partial class MainPage : ContentPage
{
    // LƯU Ý: Dùng 10.0.2.2 để gọi về localhost của máy tính từ máy ảo Android
    const string BaseUrl = "http://10.0.2.2:5213/api/Booking"; 
    
    HttpClient client = new HttpClient();
    string currentTripId = "";
    string currentRegion = "";

    public MainPage()
    {
        InitializeComponent();
        
        // ĐÃ XÓA DÒNG map.MoveToRegion(...) GÂY LỖI Ở ĐÂY
    }

    // 1. Xử lý nút ĐẶT XE
    private async void OnBookBtnClicked(object sender, EventArgs e)
    {
        // Lấy tọa độ từ ô nhập liệu
        if (!double.TryParse(txtLat.Text, out double lat) || !double.TryParse(txtLng.Text, out double lng))
        {
            await DisplayAlert("Lỗi", "Vui lòng nhập tọa độ hợp lệ!", "OK");
            return;
        }

        lblStatus.Text = "🔄 Đang tìm tài xế...";
        btnBook.IsEnabled = false;

        try
        {
            // Gửi yêu cầu lên Server
            var payload = new { lat = lat, lng = lng, pickupLocation = "Mobile App Booking" };
            var response = await client.PostAsJsonAsync($"{BaseUrl}/book", payload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                string driverName = result.GetProperty("driver").GetString();
                currentTripId = result.GetProperty("tripId").GetString();
                currentRegion = result.GetProperty("region").GetString();

                lblStatus.Text = $"✅ Tài xế: {driverName}";
                lblStatus.TextColor = Colors.Green;

                // Đổi nút sang Trả Khách
                btnBook.IsVisible = false;
                btnFinish.IsVisible = true;
            }
            else
            {
                lblStatus.Text = "❌ Không tìm thấy tài xế!";
                lblStatus.TextColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi Kết Nối", $"Không gọi được Server: {ex.Message}", "OK");
            lblStatus.Text = "❌ Lỗi hệ thống";
        }
        finally
        {
            btnBook.IsEnabled = true;
        }
    }

    // 2. Xử lý nút TRẢ KHÁCH
    private async void OnFinishBtnClicked(object sender, EventArgs e)
    {
        try
        {
            var payload = new 
            { 
                tripId = currentTripId, 
                region = currentRegion,
                endLat = double.Parse(txtLat.Text),
                endLng = double.Parse(txtLng.Text) 
            };

            var response = await client.PostAsJsonAsync($"{BaseUrl}/finish", payload);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Thành công", "Chuyến đi đã hoàn thành! Cảm ơn quý khách.", "OK");
                
                // Reset giao diện
                lblStatus.Text = "Sẵn sàng gọi xe...";
                lblStatus.TextColor = Colors.Gray;
                btnBook.IsVisible = true;
                btnFinish.IsVisible = false;
                currentTripId = "";
            }
        }
        catch
        {
            await DisplayAlert("Lỗi", "Không thể kết thúc chuyến đi.", "OK");
        }
    }
}