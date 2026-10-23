using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Automation;
using Shouldly;

namespace BudgetAnalyser.InstallTest;

/// <summary>
///     End to end test of a clean new install: clone the repository into a fresh empty temp folder, build the solution, run its unit tests, launch the
///     app, then close it using the main window's close (X) button and check it exits with code 0.
///     This test is slow and needs network access, so it is skipped unless the BUDGETANALYSER_INSTALL_TEST environment variable is set to 1.
/// </summary>
public class CleanInstallTest(ITestOutputHelper output)
{
    private const string EnableVariable = "BUDGETANALYSER_INSTALL_TEST";
    private const string RepoUrlVariable = "BUDGETANALYSER_INSTALL_TEST_REPO";
    private const string BranchVariable = "BUDGETANALYSER_INSTALL_TEST_BRANCH";
    private const string DefaultRepoUrl = "https://github.com/Benrnz/BudgetAnalyser.git";
    private const string DefaultBranch = "master";
    private const string SolutionFile = "BudgetAnalyser.sln";
    private const string AppProcessName = "BudgetAnalyser";
    private const string Configuration = "Debug";

    private static readonly TimeSpan CloneTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan BuildTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan TestTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LaunchTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan SettleTime = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ExitTimeout = TimeSpan.FromSeconds(30);

    [Fact(Timeout = 40 * 60 * 1000)]
    public async Task CleanClone_ShouldBuildTestRunAndExitWithCodeZero()
    {
        Assert.SkipUnless(Environment.GetEnvironmentVariable(EnableVariable) == "1", $"Set {EnableVariable}=1 to run the clean install test.");

        var cancellationToken = TestContext.Current.CancellationToken;
        var repoUrl = Environment.GetEnvironmentVariable(RepoUrlVariable) ?? DefaultRepoUrl;
        var branch = Environment.GetEnvironmentVariable(BranchVariable) ?? DefaultBranch;
        var workingFolder = Path.Combine(Path.GetTempPath(), "BudgetAnalyserInstallTest_" + Guid.NewGuid().ToString("N"));
        var repoFolder = Path.Combine(workingFolder, "BudgetAnalyser");

        Directory.CreateDirectory(workingFolder);
        Directory.EnumerateFileSystemEntries(workingFolder).ShouldBeEmpty();
        output.WriteLine($"Cloning {repoUrl} ({branch}) into {repoFolder}");

        try
        {
            await RunToCompletion("git", ["clone", "--depth", "1", "--branch", branch, repoUrl, repoFolder], workingFolder, CloneTimeout, cancellationToken);
            await RunToCompletion("dotnet", ["build", SolutionFile, "--configuration", Configuration, "--disable-build-servers"], repoFolder, BuildTimeout, cancellationToken);
            await RunToCompletion("dotnet", ["test", SolutionFile, "--no-build", "--configuration", Configuration], repoFolder, TestTimeout, cancellationToken);

            // dotnet run doesn't accept a solution file, so run the WPF project the solution starts.
            var launchTime = DateTime.Now;
            using var dotnetRun = Start(
                "dotnet",
                ["run", "--project", Path.Combine("BudgetAnalyser", "BudgetAnalyser.Wpf.csproj"), "--no-build", "--configuration", Configuration],
                repoFolder);
            Process? app = null;
            try
            {
                app = await WaitForMainWindow(dotnetRun, launchTime, cancellationToken);
                _ = app.Handle; // Hold a handle to the process so its exit code can still be read after it ends.
                output.WriteLine($"App is running with process id {app.Id} and its main window is showing. Waiting {SettleTime.TotalSeconds:N0} seconds.");

                await Task.Delay(SettleTime, cancellationToken);
                app.HasExited.ShouldBeFalse("The app exited on its own after showing its main window.");

                CloseApp(app);
                (await WaitForExit(app, ExitTimeout, cancellationToken)).ShouldBeTrue($"The app did not exit within {ExitTimeout.TotalSeconds:N0} seconds of being closed.");
                output.WriteLine($"App exited with code {app.ExitCode}.");
                app.ExitCode.ShouldBe(0, "The app's exit code");

                (await WaitForExit(dotnetRun, ExitTimeout, cancellationToken)).ShouldBeTrue("dotnet run did not exit after the app exited.");
                dotnetRun.ExitCode.ShouldBe(0, "dotnet run's exit code");
            }
            finally
            {
                KillIfRunning(app);
                KillIfRunning(dotnetRun);
                app?.Dispose();
            }
        }
        finally
        {
            DeleteFolder(workingFolder);
        }
    }

    private void CloseApp(Process app)
    {
        if (TryClickCloseButton(app.MainWindowHandle))
        {
            output.WriteLine("Clicked the main window's close (X) button.");
            return;
        }

        output.WriteLine($"Could not find the main window's close (X) button, closing the main window of process id {app.Id} instead.");
        app.CloseMainWindow().ShouldBeTrue("Unable to send a close message to the app's main window.");
    }

    private static void DeleteFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        try
        {
            // Git marks its object files read only, which stops Directory.Delete removing them.
            foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(folder, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Not worth failing the test over, the temp folder will be cleaned up by the OS eventually.
        }
    }

    private static bool IsInheritedTestRunnerVariable(string name)
    {
        // Don't let child processes inherit the enable flag, otherwise the cloned repo's own dotnet test would recursively run this test again.
        // MSBuild and test runner variables set for this test host can also confuse the child dotnet build and test.
        return name.Equals(EnableVariable, StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("MSBuild", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("VSTEST_", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("TESTINGPLATFORM_", StringComparison.OrdinalIgnoreCase);
    }

    private static void KillIfRunning(Process? process)
    {
        try
        {
            if (process is { HasExited: false })
            {
                process.Kill(true);
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
        {
            // Already exited.
        }
    }

    private async Task RunToCompletion(string fileName, string[] arguments, string workingDirectory, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var process = Start(fileName, arguments, workingDirectory);
        try
        {
            (await WaitForExit(process, timeout, cancellationToken)).ShouldBeTrue($"'{fileName} {arguments[0]}' did not finish within {timeout.TotalMinutes:N0} minutes.");
            process.WaitForExit(); // Flushes the remaining redirected output.
            process.ExitCode.ShouldBe(0, $"'{fileName} {arguments[0]}' failed, see the test output for details.");
        }
        finally
        {
            KillIfRunning(process);
        }
    }

    private Process Start(string fileName, string[] arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var name in startInfo.Environment.Keys.Where(IsInheritedTestRunnerVariable).ToList())
        {
            startInfo.Environment.Remove(name);
        }

        output.WriteLine($"> {fileName} {string.Join(' ', arguments)}");
        var process = new Process { StartInfo = startInfo };
        process.OutputDataReceived += (_, e) => WriteProcessOutput(e.Data);
        process.ErrorDataReceived += (_, e) => WriteProcessOutput(e.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static bool TryClickCloseButton(IntPtr windowHandle)
    {
        try
        {
            var window = AutomationElement.FromHandle(windowHandle);
            var titleBar = window.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TitleBar));
            var closeButton = titleBar?.FindFirst(
                TreeScope.Children,
                new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "Close")));
            if (closeButton is null || !closeButton.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern))
            {
                return false;
            }

            ((InvokePattern)pattern).Invoke();
            return true;
        }
        catch (Exception ex) when (ex is ElementNotAvailableException or InvalidOperationException or ArgumentException)
        {
            return false;
        }
    }

    private async Task<Process> WaitForMainWindow(Process dotnetRun, DateTime launchTime, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < LaunchTimeout)
        {
            if (dotnetRun.HasExited)
            {
                Assert.Fail($"dotnet run exited with code {dotnetRun.ExitCode} before the app's main window was shown.");
            }

            // dotnet run starts the app as a child process. Ignore any instance that was already running before the test launched it.
            foreach (var candidate in Process.GetProcessesByName(AppProcessName))
            {
                if (StartedAfter(candidate, launchTime) && candidate.MainWindowHandle != IntPtr.Zero)
                {
                    return candidate;
                }

                candidate.Dispose();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }

        Assert.Fail($"The app's main window was not shown within {LaunchTimeout.TotalMinutes:N0} minutes of running it.");
        return null;
    }

    private static bool StartedAfter(Process process, DateTime time)
    {
        try
        {
            return process.StartTime >= time;
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
        {
            return false;
        }
    }

    private static async Task<bool> WaitForExit(Process process, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
            return true;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private void WriteProcessOutput(string? line)
    {
        if (line is not null)
        {
            output.WriteLine(line);
        }
    }
}
