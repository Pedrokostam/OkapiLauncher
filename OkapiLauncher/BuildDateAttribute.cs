using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkapiLauncher
{
    /// <summary>
    /// Stores build date in UTC
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    internal class BuildDateAttribute : Attribute
    {
        public BuildDateAttribute(string value)
        {
            DateTime = DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }
        /// <summary>
        /// Build date in UTC
        /// </summary>
        public DateTime DateTime { get; }
    }
}
