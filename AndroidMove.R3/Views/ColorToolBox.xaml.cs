using AndroidMove.R3.ViewModels;

namespace AndroidMove.R3.Views
{
    public partial class ColorToolBox
    {
        public ColorToolBox()
        {
            InitializeComponent();
            this.DataContext = App.GetService<ColorToolBoxViewModel>();
        }
    }
}
