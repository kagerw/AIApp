
using MauiApp1ChatWithAI.ViewModels;

namespace MauiApp1ChatWithAI.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    public MainPage(MainViewModel viewModel, ThreadListViewModel threadListViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        SidebarPanel.BindingContext = threadListViewModel;
    }   

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel?.Dispose();
    }
}