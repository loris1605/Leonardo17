using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IServiziViewModel : IRoutableViewModel
    {
        IObservable<Unit> ServiziToMenu { get; }
    }
}
