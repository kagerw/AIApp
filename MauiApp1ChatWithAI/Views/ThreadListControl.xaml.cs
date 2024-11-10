using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithAI.ViewModels;

namespace MauiApp1ChatWithAI.Views;

public partial class ThreadListControl : ContentView
{
	public ThreadListControl()
	{
		InitializeComponent();
	}

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ChatThread selectedThread)
        {
            var vm = BindingContext as ThreadListViewModel;
            vm?.SelectThreadCommand.Execute(selectedThread);
        }
    }
}