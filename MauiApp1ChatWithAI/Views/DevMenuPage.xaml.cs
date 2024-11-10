using MauiApp1ChatWithAI.ViewModels;

namespace MauiApp1ChatWithAI.Views;

public partial class DevMenuPage : ContentPage
{
    private readonly DevMenuViewModel _viewModel;

    public DevMenuPage(DevMenuViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}