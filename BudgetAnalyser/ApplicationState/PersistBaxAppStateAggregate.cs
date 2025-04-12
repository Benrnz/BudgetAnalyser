using BudgetAnalyser.Engine.Persistence;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.ApplicationState;

public class PersistBaxAppStateAggregate(IUserMessageBox userMessageBox) : IPersistApplicationState
{
    private readonly IPersistApplicationState[] persistApplicationStateRepos = [new PersistBaxAppStateAsXaml(userMessageBox), new PersistBaxAppStateAsJson(userMessageBox)];

    public IEnumerable<IPersistentApplicationStateObject> Load()
    {
        IEnumerable<IPersistentApplicationStateObject>? models = null;
        foreach (var persistApplicationStateRepo in this.persistApplicationStateRepos)
        {
            var result = persistApplicationStateRepo.Load();
            if (persistApplicationStateRepo is PersistBaxAppStateAsJson)
            {
                models = result;
            }
        }

        return models;
    }

    public void Persist(IEnumerable<IPersistentApplicationStateObject> modelsToPersist)
    {
        foreach (var persistApplicationStateRepo in this.persistApplicationStateRepos)
        {
            persistApplicationStateRepo.Persist(modelsToPersist);
        }
    }
}
