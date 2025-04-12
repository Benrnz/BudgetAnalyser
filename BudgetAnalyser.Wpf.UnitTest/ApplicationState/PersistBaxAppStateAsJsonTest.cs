using System.Windows;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using Moq;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Wpf.UnitTest.ApplicationState;

[TestClass]
public class PersistBaxAppStateAsJsonTest
{
    private Mock<IUserMessageBox> mockUserMessageBox;
    private PersistBaxAppStateAsJsonTestHarness subject;

    [TestMethod]
    public void Load_ShouldDeserializeDataCorrectly()
    {
        // Arrange
        this.subject.SerialisedData = """
                                      {
                                          "LastBaxFile": "TestFile.bax",
                                          "ShellWindowState": {
                                              "Size": { "X": 800, "Y": 600 },
                                              "TopLeft": { "X": 100, "Y": 100 }
                                          }
                                      }
                                      """;

        // Act
        var result = this.subject.Load().ToList();

        // Assert
        Assert.AreEqual(2, result.Count, "There should be two deserialized objects.");
        var mainState = result.OfType<MainApplicationState>().Single();
        var shellState = result.OfType<ShellPersistentState>().Single();

        Assert.AreEqual("TestFile.bax", mainState.BudgetAnalyserDataStorageKey, "The LastBaxFile value should match.");
        Assert.AreEqual(new Point(800, 600), shellState.Size, "The window size should match.");
        Assert.AreEqual(new Point(100, 100), shellState.TopLeft, "The window position should match.");
    }

    [TestMethod]
    public void Load_ShouldHandleCorruptFileGracefully()
    {
        // Arrange
        this.subject.SerialisedData = "Invalid JSON";

        // Act
        var result = this.subject.Load().ToList();

        // Assert
        Assert.AreEqual(0, result.Count, "No objects should be deserialized from invalid JSON.");
        this.mockUserMessageBox.Verify(m => m.Show(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Unable to load previously used application preferences"))), Times.Once,
            "An error message should be shown to the user.");
    }


    [TestMethod]
    public void Persist_ShouldSerializeDataCorrectly()
    {
        // Arrange
        var modelsToPersist = new List<IPersistentApplicationStateObject>
        {
            new MainApplicationState { BudgetAnalyserDataStorageKey = "TestFile.bax" },
            new ShellPersistentState { Size = new Point(800, 600), TopLeft = new Point(100, 100) }
        };

        // Act
        this.subject.Persist(modelsToPersist);

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(this.subject.SerialisedData), "Serialized data should not be empty.");
        Assert.IsTrue(this.subject.SerialisedData.Contains("TestFile.bax"), "Serialized data should contain the LastBaxFile value.");
        Assert.IsTrue(this.subject.SerialisedData.Contains("800"), "Serialized data should contain the window size width.");
        Assert.IsTrue(this.subject.SerialisedData.Contains("600"), "Serialized data should contain the window size height.");
    }

    [TestInitialize]
    public void TestInitialize()
    {
        this.mockUserMessageBox = new Mock<IUserMessageBox>();
        this.subject = new PersistBaxAppStateAsJsonTestHarness(this.mockUserMessageBox.Object);
    }
}
