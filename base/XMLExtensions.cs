using System.Xml;
using System.Xml.Linq;

namespace Tridion.Extensions.ContentManager.Templating
{
    /// <summary>
    /// Extension methods for XmlDocument
    /// </summary>
    public static class XMLExtensions
    {
        #region Public Methods
        /// <summary>
        /// Converts XElement to XmlDocument
        /// </summary>
        /// <param name="element">XElement to convert</param>
        /// <returns>Converted XmlDocument</returns>
        public static XmlDocument ToXmlDocument(this XElement element)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(element.CreateReader());
            return xmlDocument;
        }

        /// <summary>
        /// Converts  to XmlDocument to XElement
        /// </summary>
        /// <param name="document">Document to convert</param>
        /// <returns>Converted XElement</returns>
        public static XElement ToXElement(this XmlDocument document)
        {
            using (var nodeReader = new XmlNodeReader(document))
            {
                nodeReader.MoveToContent();
                return XElement.Load(nodeReader);
            }
        } 
        #endregion
    }
}
