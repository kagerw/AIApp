using MauiApp1ChatWithAI.ViewModels;

namespace MauiApp1ChatWithAI.Views;

public partial class ThreadEditPage : ContentPage
{
	public ThreadEditPage(ThreadEditViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;
	}
}