using System;
using System.Collections.Specialized;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public enum ToolBarType { None, Summary, Full }
    public class ListView
    {
        public string ViewName;
        public virtual StringCollection ViewFields { get; set; }
        public virtual string Query { get; set; }
        public uint RowLimit = 30;
        public bool Paged = true;
        public bool MakeDefaultView = false;
        private bool _DefaultView = false;
        public bool DefaultView
        {
            set 
            { 
                _DefaultView = value;
                if(value) MakeDefaultView = value;
            }
            get
            {
                return _DefaultView;
            }
        }
        public bool Hidden = false;
        public bool AllItemsWithoutFolders = false;
        public Microsoft.SharePoint.SPViewCollection.SPViewType ViewType = Microsoft.SharePoint.SPViewCollection.SPViewType.Html;
        public int ViewStyle = -1;
        public ToolBarType ToolbarType = ToolBarType.Full;
        /// <summary>
        /// Show Checkboxes
        /// </summary>
        public bool TabularView = false;
    }
}
