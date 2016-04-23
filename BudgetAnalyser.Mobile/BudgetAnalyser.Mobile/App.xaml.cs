using Xamarin.Forms;

namespace BudgetAnalyser.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
               
            // The root page of your application
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
