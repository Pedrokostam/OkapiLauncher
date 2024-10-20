using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OkapiLauncher.Helpers
{
    class NullDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? NullTemplate { get; set; }
        public DataTemplate? NotNullTemplate { get; set; }
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                null => NullTemplate,
                _ => NotNullTemplate,
            };

        }
    }
}
