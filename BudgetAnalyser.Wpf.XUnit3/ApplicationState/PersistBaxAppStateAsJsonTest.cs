using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using NSubstitute;
using Rees.Wpf.Contracts;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.ApplicationState;

public class PersistBaxAppStateAsJsonTest
{
    private readonly IUserMessageBox mockUserMessageBox;
    private readonly PersistBaxAppStateAsJsonTestHarness subject;

    public PersistBaxAppStateAsJsonTest()
    {
        this.mockUserMessageBox = Substitute.For<IUserMessageBox>();
        this.subject = new PersistBaxAppStateAsJsonTestHarness(this.mockUserMessageBox);
    }

    [Fact]
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
        result.Count.ShouldBe(2, "There should be two deserialized objects.");

        var mainState = result.OfType<ApplicationEngineState>().Single();
        var shellState = result.OfType<ShellPersistentState>().Single();

        mainState.BudgetAnalyserDataStorageKey.ShouldBe("TestFile.bax", "The LastBaxFile value should match.");
        shellState.Size.ShouldBe(new Point(800, 600), "The window size should match.");
        shellState.TopLeft.ShouldBe(new Point(100, 100), "The window position should match.");
    }

    [Fact]
    public void Load_ShouldHandleCorruptFileGracefully()
    {
        // Arrange
        this.subject.SerialisedData = "Invalid JSON";

        // Act
        var result = this.subject.Load().ToList();

        // Assert
        result.ShouldBeEmpty();
        this.mockUserMessageBox.Received(1).Show(Arg.Any<Exception>(), Arg.Is<string>(s => s.Contains("Unable to load previously used application preferences")));
    }

    [Fact]
    public void Persist_ShouldSerializeDataCorrectly()
    {
        // Arrange
        var modelsToPersist = new List<IPersistentApplicationStateObject>
        {
            new ApplicationEngineState { BudgetAnalyserDataStorageKey = "TestFile.bax" },
            new ShellPersistentState { Size = new Point(800, 600), TopLeft = new Point(100, 100) }
        };

        // Act
        this.subject.Persist(modelsToPersist);

        // Assert
        this.subject.SerialisedData.ShouldNotBeNullOrEmpty();
        this.subject.SerialisedData.ShouldContain("TestFile.bax");
        this.subject.SerialisedData.ShouldContain("800");
        this.subject.SerialisedData.ShouldContain("600");
    }
}
