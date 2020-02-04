using NetworkModel;
using NWaves.Blueprints.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NWaves.Blueprints.Converters
{
    class FilterNodeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var node = values[0] as NodeViewModel;
            var filters = values[1] as IEnumerable<FilterNodeViewModel>;

            return filters.First(f => f.NetworkNode == node);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
