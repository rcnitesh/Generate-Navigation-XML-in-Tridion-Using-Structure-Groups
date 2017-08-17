/*
 * *****************************************************
 * SiteMap Generator using Structure Groups 
 * 
 * *****************************************************
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Publishing;
using Tridion.Logging;
using System.Xml.Linq;
using System.Linq;

namespace Tridion.Extensions.ContentManager.Templating
{
    [TcmTemplateTitle("GenerateNavigationXML")]
    class GenerateNavigationXML : ITemplate
    {
        #region VARIABLES
        private int IntMaxNavLevel = 2;
        private static String StrTitlePattern = CMSConstants.TitlePattern;
        Regex regex = new Regex(StrTitlePattern);
        Package Package;
        Engine Engine;
        private TemplatingLogger Logger = null;
        private const string StructureGroupMetaField_WebDavUrl = CMSConstants.WebDavUrl;
        private const string StructureGroupMetaField_AdditionalText = CMSConstants.AdditionalText;
        private PublicationTarget publicationTarget = null;
        private Page contextPage;       
        #endregion

        #region TRANSFORM CALL
        public void Transform(Engine engine, Package package)
        {
            try
            {
                // Initialize the Package, Engine & Logger variables
                Package = package;
                Engine = engine;
                /*
                 * Logger for TBB. 
                 * Logs are present in file: Tridion.ContentManager.Publisher.log
                 * Logs File Location: %TRIDION_HOME%/logs             
                 */
                Logger = TemplatingLogger.GetLogger(this.GetType());

                contextPage = engine.GetObject(package.GetByName(Package.PageName)) as Page;
                publicationTarget = engine.PublishingContext.PublicationTarget;
                Logger.Info("Page_NAME: " + contextPage.Id + "PATH: " + contextPage.Path);                

                IntMaxNavLevel = int.Parse(CMSConstants.IntMaxNavLevel);

                List<ListItem> SGs = (List<ListItem>)GetListStructureGroups(contextPage);
                
                package.PushItem("StructureGroupsCount", package.CreateStringItem(ContentType.Text, SGs.Count.ToString()));
                Logger.Info("StructureGroupsCount" + SGs.Count.ToString());

                ListItem rootSG = GetRootSG(SGs);
                XmlDocument navigationXml = GetNavigationXML(rootSG, SGs);

                Logger.Info("PUSHING_SITEMAP_XML_IN_PACKAGE: "+DateTime.Now.ToString());
                Logger.Info("DocumentElement:- " + navigationXml.DocumentElement);
                Package.PushItem("SiteMap_XML", Package.CreateXmlDocumentItem(ContentType.Xml, navigationXml));
            }
            catch(Exception ex)
            {
                Logger.Error("Error:: ", ex);                
            }
        }

        #endregion

        #region FUNCTIONS TO BUILD SITEMAP XML
        private string GetHTMLNavMenu(XmlElement elm)
        {
            string result = "";

            XmlNodeList nodes = elm.ChildNodes;

            foreach(XmlElement element in nodes)
            {
                Logger.Info(element.GetAttribute("url"));
                Logger.Info(element.GetAttribute("title"));
            }

            foreach (XmlElement element in nodes)
            {
                if (string.IsNullOrEmpty(result))
                {
                    result = "<ul>" + Environment.NewLine;
                }
                result += "\t<li><a href=\"" + element.GetAttribute("url") + "\">" + element.GetAttribute("title") + "</a></li>" + Environment.NewLine;
                result += GetHTMLNavMenu(element);
            }
            if (!string.IsNullOrEmpty(result))
            {
                result += "</ul>" + Environment.NewLine;
            }

            return result;
        }

        private XmlDocument BuildNavigationXML(ListItem currentSG, IList<ListItem> SGs, XmlDocument doc, XmlElement parent)
        {
            Logger.Info("Current SG: " + currentSG.Title);
            
            string currentSGTitle = currentSG.Title;

            if (includeRegEx(regex, ref currentSGTitle) || doc == null)
            {
                
                if (doc == null)
                {
                    //Create the Root Doc
                    doc = new XmlDocument();                    
                    parent = null;
                }

                StructureGroup objCurrentSG = (StructureGroup)Engine.GetObject(currentSG.Id);

                string currentSGURL = objCurrentSG.PublishLocationUrl;
                Logger.Info("ORIGINAL_URL: " + currentSGURL);
                
                if (objCurrentSG.PublishLocationUrl.EndsWith("/"))
                {
                    currentSGURL = objCurrentSG.PublishLocationUrl + CMSConstants.DefaultFileName; 
                }               
                else
                {
                    currentSGURL = objCurrentSG.PublishLocationUrl + "/" + CMSConstants.DefaultFileName; 
                }
                                

                string[] sgURLArr = currentSGURL.Split('/');
                int currentSGDepth = sgURLArr.Length;

                Logger.Info("CURRENT_SG_DEPTH: "+ currentSGDepth + ", URL: " + currentSGURL);

                // Structure Group XML Element: elmCurrentSG
                XmlElement elmCurrentSG = null;
                // Root Structure Group Node Element
                if (parent == null)
                {
                    elmCurrentSG = doc.CreateElement(CMSConstants.RootNodeNameText);
                }
                else
                {
                    elmCurrentSG = doc.CreateElement(CMSConstants.SGNodeNameText);
                }
                elmCurrentSG.SetAttribute("id", objCurrentSG.Id);
                elmCurrentSG.SetAttribute("title", currentSGTitle);
                //elmSG.SetAttribute("title", Regex.Replace(title, StrTitlePattern, ""));

                // Need to add index.html/default.html/index.aspx/default.aspx file in the URL by default.
                // objSG.PublishLocationUrl does not have index/default file name added. It only has folder path.
                elmCurrentSG.SetAttribute("url", currentSGURL);
                elmCurrentSG.SetAttribute("indexpage", GetStructureGroupDefaultPageId(currentSG));
                elmCurrentSG.SetAttribute("compTitle", GetStructureGroupDefaultPageComponentTitle(currentSG));

                setStrutureGroupMetadataInToXML(objCurrentSG, ref doc, ref elmCurrentSG);

                #region GET PAGES FOR CURRENT STRUCTURE GROUP
                // Get the pages for Curent Structure Group
                IList<Page> pages = GetPagesInSG(currentSG);
                Logger.Info("PAGES_COUNT" + pages.Count.ToString());
                StructureGroup engineObjectSG = (StructureGroup)Engine.GetObject(currentSG.Id);
                Logger.Info("Current_SG_URL" + currentSG.Url);
                // URL Sample: webdav/05%2E%20PierSeven/Root/020%2E%207%20Ways%20To%20Dine
                //string foldername = (currentSG.Url.Replace("%20", " ")).Replace("%2E",".").Substring(currentSG.Url.Replace("%20", " ").LastIndexOf("/") + 1);
                string foldername = (currentSG.Url.Replace("%20", " ").Replace("%2E", ".")).Substring((currentSG.Url.Replace("%20", " ").Replace("%2E", ".")).LastIndexOf("/") + 1);

                Logger.Info("FOLDER_NAME"+ foldername);
                Logger.Info("PARENT_IS_NULL: " + (parent == null).ToString());
                Logger.Info("INCLUDE_REG_EX_CALL" + includeRegEx(regex, ref foldername));
                if (includeRegEx(regex, ref foldername) || foldername.ToLower().Equals(CMSConstants.PublicationRootStructureGroupName.ToLower()))
                {   
                    foreach (Page page in pages)
                    {
                        currentSGTitle = page.Title;
                        Logger.Info("TITLE: " + currentSGTitle);
                        // DefaultFileName=-1 to not add the index.html page as a child node
                        // index.html will be added as an attribute of Structure Group node
                        if (page.PublishLocationUrl.ToLower().IndexOf(CMSConstants.DefaultFileName) == -1)
                        {
                            XmlElement elementPage = doc.CreateElement(CMSConstants.PageNodeNameText);

                            /* Set Page Attributes
                               Format:
                                      <node id="tcm:133-48133-64" title="005. About Us" url="/en/AboutUs.aspx" compTitle="About Pier 7"></node>
                             */
                            elementPage.SetAttribute("id", page.Id);
                            elementPage.SetAttribute("title", currentSGTitle);
                            //elementPage.SetAttribute("title", Regex.Replace(title, StrTitlePattern, ""));
                            elementPage.SetAttribute("url", page.PublishLocationUrl);
                            /* Set Page Metadata as Child Elements
                               Format:
                                        <node >
                                            <fdisplaymainmenu>Yes</fdisplaymainmenu>
                                            <fdisplayfooter>Yes</fdisplayfooter>
                                            <fsitemap>Yes</fsitemap>
                                            <fpreviousnext>Yes</fpreviousnext>
                                        </node>
                            */
                            if (page.MetadataSchema != null)
                            {
                                ItemFields PageMetaFields = new ItemFields(page.Metadata, page.MetadataSchema);
                                if (PageMetaFields != null)
                                {
                                    XmlElement childElement;
                                    foreach (ItemField metaField in PageMetaFields)
                                    {
                                        childElement = doc.CreateElement(metaField.Name);
                                        try
                                        {
                                            if (metaField is TextField)
                                            {
                                                childElement.InnerText = ((TextField)metaField).Value;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            //log the error
                                            Logger.Error("Exception In Reading Page Metadata", ex);
                                        }
                                        elementPage.AppendChild(childElement);
                                    }
                                }
                                else
                                {
                                    Logger.Info("PAGE:" + page.Id + CMSConstants.MetadataFieldNotFoundErrorText);                                    
                                }
                            }
                            else
                            {
                                Logger.Info("PAGE:" + page.Id + CMSConstants.MetadataSchemaNotFoundErrorText);                                
                            }

                            // Append Page Element to SG element
                            Logger.Info("APPENDING_PAGE_2_SG_NODE: " + elmCurrentSG.GetAttribute("title") + ", " + elementPage.GetAttribute("title"));
                            elmCurrentSG.AppendChild(elementPage);
                        }
                    }

                    if (currentSGDepth <= IntMaxNavLevel)
                    {
                        if (parent != null)
                        {
                            Logger.Info("APPEND_PARENT" + parent.Value);
                            parent.AppendChild(elmCurrentSG);
                        }
                        else
                        {
                            Logger.Info("APPEND_DOC" + doc.Name);
                            doc.AppendChild(elmCurrentSG);
                        }
                    }
                }
                #endregion

                #region GET CHILD STRUCTURE GROUPS OF CURRENT STRUCTURE GROUP
                // Check for Child SGs
                foreach (ListItem sg in SGs)
                {
                    //Logger.Info("Structure Group : " + sg.Id + ", " + sg.Title + ", Parent ID: " + sg.ParentId);

                    // 'sg' is a child of 'currentSG'
                    if (sg.ParentId == currentSG.Id)
                    {
                        //Recursive Calling of current method 
                        Logger.Info("RECURSIVE_CALL FOR: " + sg.Title);
                        BuildNavigationXML(sg, SGs, doc, elmCurrentSG);
                        Logger.Info("RECURSIVE CALL END FOR: " + sg.Title);
                    }
                }
                #endregion 
            }
            return doc;
        }
        
        private XmlDocument GetNavigationXML(ListItem rootSG, IList<ListItem> SGs)
        {
            XmlDocument navigationXmlDocument =  BuildNavigationXML(rootSG, SGs, null, null);
            return SortXmlDocumentNodes(ref navigationXmlDocument);
        }

        #endregion


        #region UTILITY FUNCTIONS

        private XmlDocument SortXmlDocumentNodes(ref XmlDocument navigationXmlDocument)
        {
            XDocument xDoc = ToXDocument(navigationXmlDocument);
            var sortedxDoc = new XDocument(new XElement("root",  from node in xDoc.Root.Elements()
                                                                 orderby node.Attribute("title").Value.Substring(0,3) ascending
                                                                 select node)
                                        );
            return ToXmlDocument(sortedxDoc);
        }

        private static XDocument ToXDocument(XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        private static XmlDocument ToXmlDocument(XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        private IList<ListItem> GetListStructureGroups(Page page)
        {
            Package.PushItem("GetListStructureGroupsEnterMessage", Package.CreateStringItem(ContentType.Text, "EnteredFunction"));
            Publication publication = GetPublication(page);
            OrganizationalItemsFilter sgTypeFilter = new OrganizationalItemsFilter(Engine.GetSession());
            //Filter sgFilter = new Filter();
            sgTypeFilter.ItemTypes = new[] { ItemType.StructureGroup };
            sgTypeFilter.BaseColumns = ListBaseColumns.Extended;
            sgTypeFilter.IncludeRelativeWebDavUrlColumn = true;
            sgTypeFilter.IncludePathColumn = true;

            XmlElement orgItemsSG = publication.GetListOrganizationalItems(sgTypeFilter);
            XmlNodeList itemElements = orgItemsSG.SelectNodes("*");
            
            List<ListItem> result = new List<ListItem>(itemElements.Count);
            foreach (XmlElement itemElement in itemElements)
            {
                ListItem sg = new ListItem(itemElement);
                result.Add(sg);

                /*Debugging Only*/
                //foreach(XmlAttribute attr in itemElement.Attributes)
                //{
                //    Logger.Info(">>Attribute: " + attr.Name +" , " + attr.Value);
                //}
            }
            //Sort the list on Title property
            result.Sort((a, b) => String.Compare(a.Title, b.Title));
            return result;
        }

        private IList<Page> GetPagesInSG(ListItem sg)
        {
            OrganizationalItemItemsFilter pageTypeFilter = new OrganizationalItemItemsFilter(Engine.GetSession());
            pageTypeFilter.ItemTypes = new[] { ItemType.Page };
            pageTypeFilter.BaseColumns = ListBaseColumns.Extended;
            pageTypeFilter.IncludeRelativeWebDavUrlColumn = true;
            
            StructureGroup structuregroup = Engine.GetObject(sg.Id) as StructureGroup;
            IEnumerable<RepositoryLocalObject> rlos = structuregroup.GetItems(pageTypeFilter);

            List<Page> pages = new List<Page>();
            foreach (RepositoryLocalObject localObject in rlos)
            {
                Page localPage = (Page)localObject;
                if (Boolean.Parse(CMSConstants.IncludePublishedPagesOnly))
                {
                    if (PublishEngine.IsPublished(localPage, publicationTarget, true))
                    {
                        pages.Add(localPage);
                    }
                }
                else
                {
                    pages.Add(localPage);
                }
            }
            return pages;
        }

        private Publication GetPublication(Page page)
        {
            Repository repository = page.ContextRepository;
            if (repository is Publication)
            {
                return (Publication)repository;
            }
            return null;
        }

        private ListItem GetRootSG(IList<ListItem> items)
        {
            foreach (ListItem item in items)
            {
                //Logger.Info(">> URL: "+item.Url);
                if (item.ParentId.PublicationId == -1)
                {
                    return item;
                }
            }
            Logger.Error("CANNOT READ ROOT STRUCTURE GROUP");
            return new ListItem(null);
        }

        private bool includeRegEx(Regex regex, ref string title)
        {
            if (regex == null) return true;
            MatchCollection matches = regex.Matches(title);
            if (matches.Count > 0)
            {
                // remove the prefix (using the first match)
                //title = title.Substring(matches[0].Groups[0].Length);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void setStrutureGroupMetadataInToXML(StructureGroup objSG, ref XmlDocument doc, ref XmlElement elmSG)
        {
            // Set Metadata for a Structure Group
            if (objSG.Metadata != null)
            {
                string webDavUrl = string.Empty;                
                ItemFields sgMetadataFields = new ItemFields(objSG.Metadata, objSG.MetadataSchema);
                if (sgMetadataFields != null)
                {
                    foreach (ItemField metadataField in sgMetadataFields)
                    {
                        XmlElement childElement;
                       
                            childElement = doc.CreateElement(metadataField.Name);
                            try
                            {
                                childElement.InnerText = ((TextField)metadataField).Value;
                            }
                            catch (Exception ex)
                            {
                                //log the error
                                Logger.Error("Exception In Reading Structure Group Metadata", ex);
                            }
                        elmSG.AppendChild(childElement);
                    }
                }
            }
        }

        private string GetStructureGroupDefaultPageId(ListItem sg)
        {
            IList<Page> pages = GetPagesInSG(sg);
            foreach (Page localPage in pages)
            {
                if (localPage.FileName.ToLower().Equals("index"))
                {   
                    return localPage.Id.ToString();
                }
            }
            return string.Empty;
        }

        private string GetStructureGroupDefaultPageComponentTitle(ListItem sg)
        {
            IList<Page> pages = GetPagesInSG(sg);
            foreach (Page localPage in pages)
            {
                if (localPage.FileName.ToLower().Equals("index"))
                {
                    Component comp = localPage.ComponentPresentations[0].Component;
                    ItemFields compFields = new ItemFields(comp.Content, comp.Schema);
                    if (compFields.Contains("ftitle"))
                    {
                        return ((TextField)compFields["ftitle"]).Value;
                    }
                }
            }
            return string.Empty;
        }

        private string GetPublicationPublishPath()
        {
            Repository repository = contextPage.ContextRepository;
            if (repository is Publication)
            {
                return ((Publication)repository).PublicationUrl;
            }
            else
            {
                return "/";
            }
        }
        #endregion
    }
}
