using CommunityToolkit.Maui;
using Microsoft.Maui.Controls;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GetCIDByVN;

public partial class MainPage : ContentPage
{
	int count = 0;
	


    public MainPage()
	{
		InitializeComponent();
        //var builder = MauiApp.CreateBuilder().UseMauiApp<App>().UseMauiCommunityToolkit();
        GetCID_VN_ViewMode vm = new GetCID_VN_ViewMode();
		vm.GetVerifyImage(null);
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

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
		Task.Run(() =>
		{
			Label label = sender as Label;
			int cp = this.TbVerify.CursorPosition;
            this.TbVerify.Text =this.TbVerify.Text + label.Text;
        });
    }

	
}

