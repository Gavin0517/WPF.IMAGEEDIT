using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFPhotoEditorTool.Models;

namespace WPFPhotoEditorTool.ViewModels
{
    public class EditMenu: ViewModelBase
    {

        private string  name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private string toolTip;
        public string ToolTip
        {
            get { return toolTip; }
            set
            {
                toolTip = value;
                OnPropertyChanged("ToolTip");
            }
        }


        private string source;
        public string Source
        {
            get { return source; }
            set
            {
                source = value;
                OnPropertyChanged("Source");
            }
        }

        public string SourceUrl { get; set; }
        public string CheckedSourceUrl { get; set; }

        public DrawEnum DrawEnum { get; set; }

    }
}
