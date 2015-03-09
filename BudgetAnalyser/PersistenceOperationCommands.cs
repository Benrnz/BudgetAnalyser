using System.Windows.Input;
using BudgetAnalyser.Engine;
using GalaSoft.MvvmLight.CommandWpf;

namespace BudgetAnalyser
{
    [AutoRegisterWithIoC]
    public static class PersistenceOperationCommands
    {
        private static readonly ICommand DoNotUseSaveDatabaseCommand = new RelayCommand(() => PersistenceOperations.OnSaveDatabaseCommandExecute(), () => PersistenceOperations.HasUnsavedChanges);
        private static readonly ICommand DoNotUseValidateModelsCommand = new RelayCommand(() => PersistenceOperations.OnValidateModelsCommandExecute());
        private static readonly ICommand DoNotUseLoadDatabaseCommand = new RelayCommand(() => PersistenceOperations.OnLoadDatabaseCommandExecute());
        private static readonly ICommand DoNotUseLoadDemoDatabaseCommand = new RelayCommand(() => PersistenceOperations.OnLoadDemoDatabaseCommandExecute());
        

        [PropertyInjection]
        public static PersistenceOperations PersistenceOperations { get; set; }

        // These properties use fields rather than new'ing up instance for each call because this is a shared static class consumed by many forms.
        public static ICommand SaveDatabaseCommand
        {
            get { return DoNotUseSaveDatabaseCommand; }
        }

        public static ICommand ValidateModelCommand
        {
            get { return DoNotUseValidateModelsCommand; }
        }

        public static ICommand LoadDatabaseCommand
        {
            get
            {
                return DoNotUseLoadDatabaseCommand;
            }
        }

        public static ICommand LoadDemoDatabaseCommand
        {
            get
            {
                return DoNotUseLoadDemoDatabaseCommand;
            }
        }
    }
}