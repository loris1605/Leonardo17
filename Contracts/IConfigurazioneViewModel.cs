using ReactiveUI;
using System.Reactive;

namespace Contracts
{
    public interface IConfigurazioneViewModel : IRoutableViewModel
    {
        IObservable<Unit> ConfigurazioneToMenu { get; }
    }
}
