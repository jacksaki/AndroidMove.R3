using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidMove.R3.Models
{
    public class BindableAndroidFile
    {
        public AndroidFile File { get; }
        public string Path => this.File.Path;
        public string FileName => this.File.FileName;
        public DateTime LastUpdateTime => this.File.LastUpdateTime;
        public BindableReactiveProperty<bool> IsSelected { get; } 
        public BindableAndroidFile(AndroidFile file)
        {
            File = file;
            this.IsSelected = new BindableReactiveProperty<bool>(false);
        }
    }
}
