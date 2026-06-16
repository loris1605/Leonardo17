using ReactiveUI;
using System.Reactive;

namespace Contracts
{
    public interface ISociViewModel : IRoutableViewModel
    {
        IObservable<Unit> SociToMenu { get; }
    }
}
