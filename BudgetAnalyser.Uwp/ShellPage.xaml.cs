using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BudgetAnalyser.Uwp
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            InitializeComponent();
        }

        public ShellController ViewModel => StaticBinder.UiContext.ShellController;

        private async void OnLoading(FrameworkElement sender, object args)
        {
            await ViewModel.InitialiseAsync();
        }
    }
}