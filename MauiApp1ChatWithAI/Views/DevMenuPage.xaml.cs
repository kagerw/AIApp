using MauiApp1ChatWithAI.ViewModels;

namespace MauiApp1ChatWithAI.Views;

public partial class DevMenuPage : ContentPage
{
    public DevMenuPage(DevMenuViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}