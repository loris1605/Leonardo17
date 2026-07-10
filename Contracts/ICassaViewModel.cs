using ReactiveUI;
using System.Reactive;

namespace Contracts
{
    public interface ICassaViewModel : IRoutableViewModel
    {
        IObservable<Unit> CassaToMenu { get; }
    }
}
