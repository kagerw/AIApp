using MauiApp1ChatWithAI.ViewModels;

namespace MauiApp1ChatWithAI.Views;

public partial class TestPage : ContentPage
{
    public TestPage(TestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}