using MauiApp1ChatWithAI.Views;

namespace MauiApp1ChatWithAI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(TestPage), typeof(TestPage));
            Routing.RegisterRoute(nameof(Views.DevMenuPage), typeof(Views.DevMenuPage));
            Routing.RegisterRoute(nameof(Views.ThreadSettingsPage), typeof(Views.ThreadSettingsPage));
        }
    }
}
