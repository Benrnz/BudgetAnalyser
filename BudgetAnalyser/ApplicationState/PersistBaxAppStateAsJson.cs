using System.Diagnostics;
using System.IO;
using System.Text.Json;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.ApplicationState;

/// <summary>
///     An implementation of <see cref="IPersistApplicationState" /> that saves the user meta-data as Json to a file on the local disk.
/// </summary>
public class PersistBaxAppStateAsJson : IPersistApplicationState
{
    private const string FileName = "BudgetAnalyserAppState.json";
    private readonly IUserMessageBox userMessageBox;

    private string doNotUseFullFileName = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PersistBaxAppStateAsJson" /> class.
    /// </summary>
    /// <param name="userMessageBox">A service to show the user a message box.</param>
    /// <exception cref="System.ArgumentNullException">userMessageBox cannot be null.</exception>
    public PersistBaxAppStateAsJson(IUserMessageBox userMessageBox)
    {
        this.userMessageBox = userMessageBox ?? throw new ArgumentNullException(nameof(userMessageBox));
    }

    /// <summary>
    ///     Gets the full name of the file to save the data into. The file will be overwritten. By default, this will save to the application folder with the name BudgetAnalyserAppState.xml.
    /// </summary>
    protected virtual string FullFileName
    {
        get
        {
            if (string.IsNullOrEmpty(this.doNotUseFullFileName))
            {
                this.doNotUseFullFileName = DefaultFileName();
            }

            return this.doNotUseFullFileName;
        }
    }

    /// <summary>
    ///     Load the user state from the Json file on the local disk.
    /// </summary>
    /// <returns>
    ///     An array of data objects that are self identifying. This array will need to be processed or broadcast to the components that consume this data.
    /// </returns>
    /// <exception cref="BadApplicationStateFileFormatException">
    ///     This will be thrown if the file is invalid.
    /// </exception>
    public IEnumerable<IPersistentApplicationStateObject> Load()
    {
        if (!File.Exists(FullFileName))
        {
            return new List<IPersistentApplicationStateObject>();
        }

        try
        {
            var serialised = JsonSerializer.Deserialize<BaxApplicationStateDto>(ReadAppStateFileAsText()) ??
                   throw new BadApplicationStateFileFormatException($"The file used to store application state ({FullFileName}) is not in the correct format. It may have been tampered with.");
            var models = new List<IPersistentApplicationStateObject> {
                new MainApplicationState { BudgetAnalyserDataStorageKey = serialised.LastBaxFile },
                new ShellPersistentState { Size = serialised.ShellWindowState.Size, TopLeft = serialised.ShellWindowState.TopLeft }
            };
            return models;
        }
        catch (IOException ex)
        {
            return HandleCorruptFileGracefully(ex);
        }
        catch (JsonException ex)
        {
            return HandleCorruptFileGracefully(ex);
        }
    }

    /// <summary>
    ///     Persist the user data to the Json file on the local disk.
    /// </summary>
    /// <param name="modelsToPersist">
    ///     All components in the App that implement <see cref="IPersistentApplicationStateObject" /> so the implementation can go get the data to persist.
    /// </param>
    public void Persist(IEnumerable<IPersistentApplicationStateObject> modelsToPersist)
    {
        // Persisting as a list of objects will cause the JSON Serializer to properly serialise each as their own type.
        var data = new BaxApplicationStateDto(modelsToPersist.ToArray());
        using var stream = CreateWritableStream();

        try
        {
            JsonSerializer.Serialize(stream, data, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (IOException ex)
        {
            this.userMessageBox.Show(ex, "Unable to save application preferences. Please check the file is not read-only or in use by another process.");
        }
    }

    protected virtual Stream CreateWritableStream()
    {
        return File.Open(FullFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
    }

    protected virtual string DefaultFileName()
    {
        var location = Path.GetDirectoryName(GetType().Assembly.Location);
        Debug.Assert(location is not null);
        return Path.Combine(location, FileName);
    }

    protected virtual string ReadAppStateFileAsText()
    {
        return File.ReadAllText(FullFileName);
    }

    private IEnumerable<IPersistentApplicationStateObject> HandleCorruptFileGracefully(Exception ex)
    {
        this.userMessageBox.Show(ex, $"Unable to load previously used application preferences. Preferences have been returned to default settings.\n\n{ex.Message}");
        return new List<IPersistentApplicationStateObject>();
    }
}
