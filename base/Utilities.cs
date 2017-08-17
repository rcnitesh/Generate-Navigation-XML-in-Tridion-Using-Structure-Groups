using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;

namespace Tridion.Extensions.ContentManager.Templating
{

    /// <summary>
    /// Enum representing field types
    /// </summary>
    public enum FieldType
    {
        DateField,
        ExternalLinkField,
        EmbeddedSchemaField,
        MultimediaLinkField,
        ComponentLinkField,
        NumberField,
        MultiLineTextField,
        SingleLineTextField,
        XHtmlField,
        KeywordField,
        TextField
    }

    /// <summary>
    /// Contains utitility methods for xml parsing.
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Gets the type of the specified field in an enumeration
        /// </summary>
        public static FieldType GetFieldType(ItemField field)
        {
            return GetFieldType(field, false);
        }

        /// <summary>
        /// Gets the type of the specified field in an enumeration
        /// </summary>
        /// <param name="limitToBase">confines the result to the base type of the field such as returning a component link field type
        /// for a multimedia field</param>
        public static FieldType GetFieldType(ItemField field, bool limitToBase)
        {
            FieldType res;

            if (limitToBase)
            {
                if (field is EmbeddedSchemaField)
                {
                    res = FieldType.EmbeddedSchemaField;
                }
                else if (field is NumberField)
                {
                    res = FieldType.NumberField;
                }
                else if (field is ComponentLinkField)
                {
                    res = FieldType.ComponentLinkField;
                }
                else if (field is KeywordField)
                {
                    res = FieldType.KeywordField;
                }
                else if (field is DateField)
                {
                    res = FieldType.DateField;
                }
                else
                {
                    res = FieldType.TextField;
                }
            }
            else
            {
                if (field is ExternalLinkField)
                {
                    res = FieldType.ExternalLinkField;
                }
                else if (field is EmbeddedSchemaField)
                {
                    res = FieldType.EmbeddedSchemaField;
                }
                else if (field is MultimediaLinkField)
                {
                    res = FieldType.MultimediaLinkField;
                }
                else if (field is KeywordField)
                {
                    res = FieldType.KeywordField;
                }
                else if (field is ComponentLinkField)
                {
                    res = FieldType.ComponentLinkField;
                }
                else if (field is XhtmlField)
                {
                    res = FieldType.XHtmlField;
                }
                else if (field is DateField)
                {
                    res = FieldType.DateField;
                }
                else if (field is MultiLineTextField)
                {
                    res = FieldType.MultiLineTextField;
                }
                else if (field is NumberField)
                {
                    res = FieldType.NumberField;
                }
                else
                {
                    res = FieldType.SingleLineTextField;
                }
            }

            return res;
        }

        /// <summary>
        /// Converts a string to a memory stream
        /// </summary>
        public static Stream ConvertStringToStream(string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Removes namespaces from xmldocument
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static XmlDocument RemoveAllNamespaces(XmlDocument doc)
        {
            XElement xDoc = doc.ToXElement();
            XElement docElement = RemoveAllNamespaces(xDoc);
            return docElement.ToXmlDocument();
        }

        /// <summary>
        /// Removes Xlink related attributes from node
        /// </summary>
        /// <param name="node"></param>
        public static void RemoveXLinkAttributes(XmlNode node)
        {
            if (node.Attributes.Count < 1)
            {
                return;
            }

            for (int i = node.Attributes.Count - 1; i >= 0; i--)
            {
                if ((node.Attributes[i].NamespaceURI == Constants.XlinkNamespace)
                    || (node.Attributes[i].Value == Constants.XlinkNamespace))
                {
                    node.Attributes.Remove(node.Attributes[i]);
                }
            }
        }

        /// <summary>
        /// Adds attribute to the XmlNode
        /// </summary>
        /// <param name="node">XmlNode to be modified</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <param name="attributeValue">Value of the attribute</param>
        public static void AddAttribute(XmlNode node, string attributeName, string attributeValue)
        {
            AddAttribute(node, attributeName, null, attributeValue);
        }

        /// <summary>
        /// Adds attribute to the XmlNode with specified namespace
        /// </summary>
        /// <param name="node">XmlNode to be modified</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <param name="attNamespace">Namespace of the attribute</param>
        /// <param name="attributeValue">Value of the attribute</param>
        public static void AddAttribute(XmlNode node, string attributeName, string attNamespace, string attributeValue)
        {
            XmlAttribute newAttribute = null;
            if (string.IsNullOrEmpty(attNamespace))
            {
                newAttribute = node.OwnerDocument.CreateAttribute(attributeName);
            }
            else
            {
                newAttribute = node.OwnerDocument.CreateAttribute(attributeName, attNamespace);
            }

            newAttribute.Value = attributeValue;
            node.Attributes.Append(newAttribute);
        }

        #region Protected Methods

        /// <summary>
        /// Constructs Regex based upon the parameter
        /// </summary>
        /// <param name="parameter">Parameter for generating Regex</param>
        /// <returns></returns>
        protected static Regex GetRegexForParameter(string parameter)
        {
            Regex result = null;
            if (!String.IsNullOrEmpty(parameter))
            {
                string pattern = parameter.Substring(parameter.IndexOf("("));
                pattern = pattern.Substring(1, pattern.Length - 2);

                // Logger.debug("GetRegexForParameter: " + pattern);
                result = new Regex(pattern);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="cssClass"></param>
        /// <returns></returns>
        protected static string GetOpeningTag(string tag, string cssClass)
        {
            string menu = "<" + tag;
            if (!String.IsNullOrEmpty(cssClass))
            {
                menu += " class='" + cssClass + "'";
            }

            menu += ">" + Environment.NewLine;
            return menu;
        }

        /// <summary>
        /// Encodes passed values
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded value</returns>
        protected static string Encode(string value)
        {
            return System.Web.HttpUtility.HtmlEncode(value);
        }

        /// <summary>
        /// Return a list of objects of the requested type from the XML.
        /// </summary>
        /// <remarks>
        /// This method goes back to the database to retrieve more information. So it is NOT just
        /// a fast and convenient way to get a type safe list from the XML.
        /// </remarks>
        /// <typeparam name="T">The type of object to return, like Publication, User, Group, OrganizationalItem</typeparam>
        /// <param name="listElement">The XML from which to construct the list of objects</param>
        /// <returns>a list of objects of the requested type from the XML</returns>
        protected IList<T> GetObjectsFromXmlList<T>(Engine engine, XmlElement listElement) where T : IdentifiableObject
        {
            XmlNodeList itemElements = listElement.SelectNodes("*");
            List<T> result = new List<T>(itemElements.Count);
            foreach (XmlElement itemElement in itemElements)
            {
                result.Add(GetObjectFromXmlElement<T>(engine, itemElement));
            }

            result.Sort(delegate(T item1, T item2)
            {
                return item1.Title.CompareTo(item2.Title);
            });

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="engine"></param>
        /// <param name="itemElement"></param>
        /// <returns></returns>
        protected T GetObjectFromXmlElement<T>(Engine engine, XmlElement itemElement) where T : IdentifiableObject
        {
            return (T)engine.GetObject(itemElement.GetAttribute("ID"));
        }

        /// <summary>
        /// Returns the root structure group from the list of structure groups specified.
        /// </summary>
        /// <exception cref="InvalidDataException">when there is no root structure group in the list</exception>
        /// <param name="items">The list of structure groups to search.</param>
        /// <returns>the root structure group from the list of structure groups specified</returns>
        protected ListItem GetRootSG(IList<ListItem> items)
        {
            foreach (ListItem item in items)
            {
                if (item.ParentId.PublicationId == -1)
                {
                    return item;
                }
            }

            throw new InvalidDataException("Could not find root structure group");
        }

        /// <summary>
        /// Returns the root structure group for the specified item
        /// </summary>
        /// <param name="item">Any item which resides in a publication</param>
        /// <returns>The Root Structure Group in the publication</returns>
        protected StructureGroup GetRootSG(RepositoryLocalObject item)
        {
            Repository pub = item.OwningRepository;

            return this.GetRootSG(pub);
        }

        /// <summary>
        /// Returns the root structure group for the specified publication
        /// </summary>
        /// <returns>The Root Structure Group in the publication</returns>
        /// <remarks>copied and modified code from Repository.RootFolder :)</remarks>
        protected StructureGroup GetRootSG(Repository publication)
        {
            Filter filter = new Filter();
            filter.Conditions["ItemType"] = ItemType.StructureGroup;

            IList<RepositoryLocalObject> items = publication.GetItems(filter);

            if (items.Count == 0)
            {
                return null;
            }
            else
            {
                return (StructureGroup)items[0];
            }
        }

        protected Component GetComponentValue(string fieldNAme, ItemFields fields)
        {
            if (fields.Contains(fieldNAme))
            {
                ComponentLinkField field = fields[fieldNAme] as ComponentLinkField;
                return field.Value;
            }

            return null;
        }

        protected IList<Component> GetComponentValues(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                ComponentLinkField field = (ComponentLinkField)fields[fieldName];
                return (field.Values.Count > 0) ? field.Values : null;
            }
            else
            {
                return null;
            }
        }

        protected IList<DateTime> GetDateValues(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                DateField field = (DateField)fields[fieldName];
                return (field.Values.Count > 0) ? field.Values : null;
            }
            else
            {
                return null;
            }
        }

        protected IList<Keyword> GetKeywordValues(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                KeywordField field = (KeywordField)fields[fieldName];
                return (field.Values.Count > 0) ? field.Values : null;
            }
            else
            {
                return null;
            }
        }

        protected IList<double> GetNumberValues(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                NumberField field = (NumberField)fields[fieldName];
                return (field.Values.Count > 0) ? field.Values : null;
            }
            else
            {
                return null;
            }
        }

        protected IList<string> GetStringValues(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                TextField field = (TextField)fields[fieldName];
                return (field.Values.Count > 0) ? field.Values : null;
            }
            else
            {
                return null;
            }
        }

        protected string GetSingleStringValue(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                TextField field = fields[fieldName] as TextField;
                if (field != null)
                {
                    return field.Value;
                }
            }

            return null;
        }
        #endregion

        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement element = new XElement(xmlDocument.Name.LocalName,
                    xmlDocument.Attributes().Select(att => RemoveAttributeNamespaces(att)));
                element.Value = xmlDocument.Value;
                return element;
            }

            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)), xmlDocument.Attributes().Select(att => RemoveAttributeNamespaces(att)));
        }

        private static XAttribute RemoveAttributeNamespaces(XAttribute att)
        {
            if (att.IsNamespaceDeclaration)
            {
                return null;
            }

            XAttribute attribute = new XAttribute(att.Name, att.Value);

            return attribute;
        }
        public static Category GetCategoryByTitle(Publication objPublication, string categoryTitle)
        {
            Category objCategory = null;
            Filter categoryFilter = new Filter();
            foreach (Category category in objPublication.GetCategories(categoryFilter))
            {
                if (categoryTitle.ToUpper().Equals(category.Title.ToUpper()))
                {
                    objCategory = category;
                    break;
                }
            }

            return objCategory;
        }

        public static Category GetCategoryByID(Publication objPublication, string categoryTCMID)
        {
            Filter categoryFilter = new Filter();
            categoryFilter.Conditions["ID"] = categoryTCMID;
            // TODO: Refactor deprecated method GetCategories
            foreach (Category category in objPublication.GetCategories(categoryFilter))
            {
                if (categoryTCMID.ToUpper().Equals(category.Id.ToString().ToUpper()))
                {
                    return category;
                }
            }
            return null;
        }
    }    
}