namespace RideHailing.Mobile;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
	}

    // Sự kiện khi bấm vào thanh tìm kiếm
    private async void OnSearchTapped(object sender, EventArgs e)
    {
        // Chuyển sang trang đặt xe (MainPage cũ có bản đồ)
        // Chúng ta sẽ sửa lại trang đó sau để khớp với flow
        await Navigation.PushAsync(new MainPage()); 
    }

    private async void OnCarServiceClicked(object sender, EventArgs e)
    {
        // Chọn dịch vụ Ô tô -> Cũng vào trang bản đồ
        await Navigation.PushAsync(new MainPage());
    }
}