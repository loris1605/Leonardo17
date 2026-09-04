using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels;

namespace Cassa.ViewModels
{
    public interface ISchedaContoViewModel : IRoutableViewModel
    {
        // Define any properties or methods that the SchedaContoViewModel should implement
    }

    public class SchedaContoViewModel : ViewModelBase, ISchedaContoViewModel
    {

    }
}
