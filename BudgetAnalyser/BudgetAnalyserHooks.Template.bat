@REM  BudgetAnalyserHooks.bat [Category:Save|Exit] [Origin:Object-Type-Name] [Location:BudgetAnalyser Exe Folder] [Sender:Object-Type-Name]
@REM  Example: BudgetAnalyserHooks.bat Exit ApplicationDatabaseService C:\GitRepositories\BudgetAnalyserProject\Trunk\BudgetAnalyser\bin\Debug\ BatchFileApplicationHookSubscriber
@ECHO %0 %1 %2 %3 %4

@IF %2=="Exit" GOTO Commit
@ECHO Not the Exit event... exiting.
@GOTO End

:Commit 
REM Add your code here.

:End
@ECHO Exiting...

