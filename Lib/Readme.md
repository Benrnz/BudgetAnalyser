This is a temporary folder to contain known working versions of the BudgetAnalyser.Engine and Storage libraries and their dependencies.
The reason for this is because as I peicemeal convert this application to only consume engine functionality via a REST service, I want to be sure the master branch has documented known working versions easily accessible.

Commit hash e88f733 on master branch.

For the time being the WPF app will continue to reference the code project directly.  But there will come a time when I will need to merge the .NET8 conversion back into master.  The WPF app at that time will temporarily reference the Lib folder for a hard reference to a known stable version of the engine libraries. Until a new branch can be created, built and tested to consume the service properly.  Then this folder can be deleted.
