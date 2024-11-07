using MauiApp1ChatWithAI.ViewModels;

namespace MauiApp1ChatWithAI.Views;

public partial class TestPage : ContentPage
{
    public TestPage(TestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void Entry_Completed(object sender, EventArgs e)
    {
        var viewModel = BindingContext as TestViewModel;
        viewModel?.SendMessageCommand.Execute(null);
    }
}