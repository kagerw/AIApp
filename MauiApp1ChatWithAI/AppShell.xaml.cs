using MauiApp1ChatWithAI.Views;

namespace MauiApp1ChatWithAI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Views.DevMenuPage), typeof(Views.DevMenuPage));
            Routing.RegisterRoute(nameof(Views.ThreadCreatePage), typeof(Views.ThreadCreatePage));
            Routing.RegisterRoute(nameof(Views.ThreadEditPage), typeof(Views.ThreadEditPage));
        }
    }
}
