using AndroidMove.R3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidMove.R3.ViewModels
{
    public class FileListWindowViewModel : ViewModelBase
    {
        public FileListWindowViewModel(AndroidDevice device) : base()
        {
            Device = device;
        }
        public AndroidDevice Device { get; }
    }
}
