using MauiApp1ChatWithAI.ViewModels;

namespace MauiApp1ChatWithAI.Views;

public partial class ThreadSettingsPage : ContentPage
{
    public ThreadSettingsPage(ThreadSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}