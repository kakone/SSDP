using System.IO;
using System.Text;

namespace UPnP
{
    /// <summary>
    /// Implements a TextWriter for writing information to a string
    /// </summary>
    public class StringWriterUTF8 : StringWriter
    {
        /// <summary>
        /// Gets the Encoding in which the output is written
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;
    }
}
