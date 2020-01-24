using System.IO;
using System.Reflection;

namespace PaatyDSM
{
    /// <summary>
    /// Load a text file embedded resource.
    /// </summary>
    public static class LoadResource
    {
        /// <summary>
        /// Load the ReleaseNotes file.
        /// </summary>
        public static string ReadReleaseNotes()
        {
            try
            {
                var assembly = typeof(LoadResource).GetTypeInfo().Assembly;
                Stream stream = assembly.GetManifestResourceStream("SharedItems.ReleaseNotes.txt");

                string text = "";
                using (var reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
                return text;
            }
            catch
            {
                return "";
            }
        }
    }
}