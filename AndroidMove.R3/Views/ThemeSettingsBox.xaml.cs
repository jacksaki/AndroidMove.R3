using AndroidMove.R3.ViewModels;

namespace AndroidMove.R3.Views
{
    public partial class ThemeSettingsBox
    {
        public ThemeSettingsBox()
        {
            InitializeComponent();
            DataContext = App.GetService<ThemeSettingsViewModel>();
        }
    }
}
