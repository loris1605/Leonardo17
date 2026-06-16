using ReactiveUI;
using System.Reactive;

namespace Contracts
{
    public interface IConnectionViewModel : IRoutableViewModel
    {
        IObservable<Unit> ConnectionToLogin { get; }
    }
}
