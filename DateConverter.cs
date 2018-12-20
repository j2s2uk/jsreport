using System;
using System.Windows.Data;

namespace JSReport {
    [ValueConversion(typeof(DateTime), typeof(string))]
    class DateConverter : IValueConverter {
        /// <summary>
        /// Take the DateTime and return the String representation
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            DateTime date = (DateTime)value;
            if (date.Ticks == 0) {
                return "";
            } else {
                return date.ToLongDateString();
            }
        }

        /// <summary>
        /// Take a string and attempt to turn it into a valid DateTime
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string dateStr = value.ToString().Trim();
            if (dateStr == "") {
                return new DateTime(0);
            } else {
                DateTime date;
                if (DateTime.TryParse(dateStr, out date)) {
                    return date;
                }
                return value;
            }
        }
    }
}
