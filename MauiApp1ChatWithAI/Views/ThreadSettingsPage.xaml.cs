using MauiApp1ChatWithAI.ViewModels;

namespace MauiApp1ChatWithAI.Views;

public partial class ThreadCreatePage : ContentPage
{
    public ThreadCreatePage(ThreadCreateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}