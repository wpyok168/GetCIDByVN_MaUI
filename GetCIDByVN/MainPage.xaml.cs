using CommunityToolkit.Maui;

namespace GetCIDByVN;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
		//var builder = MauiApp.CreateBuilder().UseMauiApp<App>().UseMauiCommunityToolkit();
        GetCID_VN_ViewMode vm = new GetCID_VN_ViewMode();

        this.BindingContext = vm;

    }

	private void OnCounterClicked(object sender, EventArgs e)
	{
        DisplayAlert("Alert", "You have been alerted", "OK");
        count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}

