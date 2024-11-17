using MauiApp1ChatWithAI.ViewModels;

namespace MauiApp1ChatWithAI.Views;

public partial class ThreadDetailsPage : ContentPage
{
    public ThreadDetailsPage(ThreadDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}