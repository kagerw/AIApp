using MauiApp1ChatWithAI.Views;

namespace MauiApp1ChatWithAI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(TestPage), typeof(TestPage));
        }
    }
}
