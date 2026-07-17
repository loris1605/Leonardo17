using ReactiveUI;
using System.Reactive;

namespace Contracts
{
    public interface IMenuViewModel : IRoutableViewModel
    {
        IObservable<Unit> MenuToLogin { get; }
        IObservable<Unit> MenuToSoci { get; }
        IObservable<Unit> MenuToConnection { get; }
        IObservable<Unit> MenuToConfigurazione { get; }
        IObservable<int> MenuToCassa { get; }
    }
}
