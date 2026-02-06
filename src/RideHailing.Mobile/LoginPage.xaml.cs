namespace RideHailing.Mobile;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string phone = txtPhone.Text;
        if (string.IsNullOrEmpty(phone) || phone.Length < 9)
        {
            await DisplayAlert("Lỗi", "Vui lòng nhập số điện thoại hợp lệ", "OK");
            return;
        }

        // TODO: Gửi OTP xác thực ở đây (Làm sau)
        
        // Tạm thời chuyển thẳng vào trang chủ
        await Shell.Current.GoToAsync("//HomePage");
    }
}