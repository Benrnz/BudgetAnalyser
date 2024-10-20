using System.ComponentModel;
using System.Windows;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Interaction logic for NewWindowContainer.xaml
    /// </summary>
    public partial class NewWindowContainer : Window
    {
        private INotifyPropertyChanged? notifyController;
        private IShowableController? showableController;

        public NewWindowContainer()
        {
            InitializeComponent();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            if (this.notifyController is not null)
            {
                this.notifyController.PropertyChanged -= OnControllerPropertyChanged;
            }
        }

        private void OnControllerPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (this.showableController is not null)
            {
                if (propertyChangedEventArgs.PropertyName == "Shown" && this.showableController.Shown == false)
                {
                    this.notifyController.PropertyChanged -= OnControllerPropertyChanged;
                    Close();
                }
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is not null && this.notifyController is not null)
            {
                this.notifyController.PropertyChanged -= OnControllerPropertyChanged;
            }

            this.notifyController = e.NewValue as INotifyPropertyChanged;
            if (this.notifyController is not null)
            {
                this.notifyController.PropertyChanged += OnControllerPropertyChanged;
            }

            this.showableController = e.NewValue as IShowableController;
        }
    }
}