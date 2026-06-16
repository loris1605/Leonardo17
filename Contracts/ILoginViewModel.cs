using ReactiveUI;
using System.Reactive;

namespace Contracts
{
    public interface ILoginViewModel : IRoutableViewModel
    {
        IObservable<Unit> LoginSuccesso { get; }
    }
}
