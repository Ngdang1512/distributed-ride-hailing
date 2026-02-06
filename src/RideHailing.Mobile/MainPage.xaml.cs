using System.Net.Http.Json;
using System.Text.Json;

namespace RideHailing.Mobile;

public partial class MainPage : ContentPage
{
    // Cấu hình API (Server Backend)
    const string BaseUrl = "http://10.0.2.2:5213/api/Booking";
    
    HttpClient client = new HttpClient();
    string currentTripId = "";
    string currentRegionCode = "";

    public MainPage()
    {
        InitializeComponent();
        // Tự động kích hoạt lấy vị trí khi mở trang (nếu muốn)
        // OnGetLocationClicked(this, EventArgs.Empty);
    }

    // --- 1. LOGIC LẤY GPS (QUAN TRỌNG) ---
    private async void OnGetLocationClicked(object sender, EventArgs e)
    {
        lblStatus.Text = "Đang định vị...";
        
        try
        {
            // Kiểm tra quyền
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Cần quyền", "Ứng dụng cần quyền vị trí để hoạt động.", "OK");
                    lblStatus.Text = "❌ Không có quyền GPS";
                    return;
                }
            }

            // Lấy tọa độ (Timeout 10s)
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                // Lưu vào ô ẩn
                txtLat.Text = location.Latitude.ToString();
                txtLng.Text = location.Longitude.ToString();

                // Hiển thị đẹp
                lblAddress.Text = $"{location.Latitude:0.0000}, {location.Longitude:0.0000}";
                
                // Logic phân vùng HN/HCM
                if (location.Latitude > 17)
                {
                    lblRegion.Text = "📍 Khu vực: Hà Nội (Miền Bắc)";
                    currentRegionCode = "HN";
                }
                else
                {
                    lblRegion.Text = "📍 Khu vực: TP.HCM (Miền Nam)";
                    currentRegionCode = "HCM";
                }

                lblStatus.Text = "✅ Đã xác định vị trí";
            }
            else
            {
                lblStatus.Text = "⚠️ Không lấy được GPS (Thử set trên Emulator)";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi GPS", ex.Message, "OK");
        }
    }

    // --- 2. LOGIC ĐẶT XE (GỌI SERVER) ---
    private async void OnBookBtnClicked(object sender, EventArgs e)
    {
        if (txtLat.Text == "0" || string.IsNullOrEmpty(txtLat.Text))
        {
            await DisplayAlert("Chưa có vị trí", "Vui lòng bấm nút định vị trước!", "OK");
            return;
        }

        double lat = double.Parse(txtLat.Text);
        double lng = double.Parse(txtLng.Text);

        lblStatus.Text = "🔄 Đang tìm tài xế gần nhất...";
        btnBook.IsEnabled = false;

        try
        {
            var payload = new { lat = lat, lng = lng, pickupLocation = "Vị trí GPS Mobile" };
            var response = await client.PostAsJsonAsync($"{BaseUrl}/book", payload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                string driverName = result.GetProperty("driver").GetString();
                currentTripId = result.GetProperty("tripId").GetString();

                lblStatus.Text = $"✅ Tài xế đang đến: {driverName}";
                lblStatus.TextColor = Colors.Green;
                
                await DisplayAlert("Thành công", $"Tài xế {driverName} đã nhận chuyến!", "OK");

                btnBook.IsVisible = false;
                btnFinish.IsVisible = true;
            }
            else
            {
                lblStatus.Text = "❌ Không tìm thấy xe!";
                lblStatus.TextColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi Server", "Không kết nối được Backend. Kiểm tra 'dotnet run --urls' chưa?", "OK");
        }
        finally
        {
            btnBook.IsEnabled = true;
        }
    }

    // --- 3. LOGIC TRẢ KHÁCH ---
    private async void OnFinishBtnClicked(object sender, EventArgs e)
    {
        try
        {
            var payload = new 
            { 
                tripId = currentTripId, 
                region = currentRegionCode, // Gửi đúng vùng để Server biết tìm DB nào
                endLat = double.Parse(txtLat.Text),
                endLng = double.Parse(txtLng.Text) 
            };

            var response = await client.PostAsJsonAsync($"{BaseUrl}/finish", payload);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Cảm ơn", "Chuyến đi hoàn tất!", "OK");
                // Reset
                lblStatus.Text = "Sẵn sàng";
                lblStatus.TextColor = Colors.Gray;
                btnBook.IsVisible = true;
                btnFinish.IsVisible = false;
            }
        }
        catch
        {
            await DisplayAlert("Lỗi", "Không thể hoàn thành chuyến.", "OK");
        }
    }

    // Nút Back
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}