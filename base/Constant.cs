using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tridion.Extensions.ContentManager.Templating
{
    public class CMSConstants
    {
        public class StructureGroupMetadataSchema
        {
            public const string IsVisibleInMenu = "isvisibleinmenu";
            public const string IsVisibleInSitemap = "isvisibleinsitemap";
            public const string IsVisibleInSubMenu = "isvisibleinsubmenu";
        }
        public const string RootElement = "Root";
        public const string CategoryRoot = "Category";
        public const string KeyWordRoot = "Keyword";
        public const string ParentKeyWordAttribute = "ParentKeyword";
        public const string KeyWordDescription = "Description";
        public const string KeyWordKey = "Key";
        public const string KeyWordTitle = "Title";
        public const string KeyWordValue = "KeywordValue";
        public const string KeyMetaNode = "Meta";
        public const string CategoryTCMURI = "CategoryTCMURI";
        public const string KeyIsAbstract = "IsAbstract";
        public const string RelatedKeywords = "RelatedKeywords";
        public const string ChildKeywords = "ChildKeywords";
        public const string CategoryName = "CategoryName";
        public const string CategoryXMLName = "CategoryXMLName";
        public const string CategoryDescription = "Description";
        public const string WebDavUrl = "WebDavUrl";
        public const string AdditionalText = "AdditionalText";
        public const string TitlePattern = @"^[0-9]+\.";
        public const string RootNodeNameText = "root";
        public const string SGNodeNameText = "node";
        public const string PageNodeNameText = "node";
        public const string DefaultFileName = "index.html";
        public const string IntMaxNavLevel = "5";
        public const string MetadataFieldNotFoundErrorText = "Does not have any Metadata Fields";
        public const string MetadataSchemaNotFoundErrorText = "Does not have any Metadata Schema associated with it";
        public const string IncludePublishedPagesOnly = "false";
        public const string PublicationRootStructureGroupName = "root";
    }
}
