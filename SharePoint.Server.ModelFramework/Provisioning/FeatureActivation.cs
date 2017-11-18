using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using System.Collections.Generic;
using System.Timers;

namespace cyberblast.SharePoint.Server.ModelFramework.Provisioning {
    public class FeatureActivation
    {
        const int PUBLISHING_MAX_DELAY = 60000;
        const int POLL_SPEED = 1000;
        public delegate void PublishingWebActivatedHandler(SPWeb web, PublishingSite pubSite, PublishingWeb pubWeb);
        public delegate void PublishingActivatedHandler(string webUrl);
        public event PublishingWebActivatedHandler OnPublishingWebActivated = null;//delegate(SPWeb web, PublishingSite pubSite, PublishingWeb pubWeb) { };
        public event PublishingActivatedHandler OnPublishingActivated = null;//delegate(string webUrl) { };

        public static void ActivateSiteCollectionFeature(string webUrl, string featureId, bool force = false)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(webUrl))
                {
                    Guid id = new Guid(featureId);
                    SPFeatureCollection featureCollection = site.Features;
                    if (force || featureCollection[id] == null)
                    {
                        if (featureCollection[id] != null)
                        {
                            try
                            {
                                site.AllowUnsafeUpdates = true;
                                featureCollection.Remove(id, force);
                            }
                            catch { }
                        }
                        site.AllowUnsafeUpdates = true;
                        featureCollection.Add(id, force);
                        site.AllowUnsafeUpdates = false;
                    }
                }
            });
        }

        public static void ActivateWebFeature(string webUrl, string featureId, bool force = false)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(webUrl))
                using (SPWeb newWeb = site.OpenWeb())
                {
                    Guid id = new Guid(featureId);
                    SPFeatureCollection featureCollection = newWeb.Features;
                    if (force || featureCollection[id] == null)
                    {
                        if (featureCollection[id] != null)
                        {
                            try
                            {
                                newWeb.AllowUnsafeUpdates = true;
                                featureCollection.Remove(id, force);
                                newWeb.Update();
                            }
                            catch { }
                        }

                        newWeb.AllowUnsafeUpdates = true;
                        featureCollection.Add(id, force);
                        newWeb.Update();
                        newWeb.AllowUnsafeUpdates = false;
                    }
                }
            });
        }

        public void ActivatePublishingWeb(string webUrl)
        {
            // WebPublishing
            ActivateWebFeature(webUrl, "94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb", true);
            PollPublishingWebActivated(webUrl);
        }
        public void WaitForPublishingWeb(string webUrl)
        {
            PollPublishingWebActivated(webUrl);
        }        
        
        public static void RetractWebPartFile(ref SPSite site, string filename)
        {
            List<SPFile> FilesToDelete = new List<SPFile>();
            SPList webPartGallery = site.RootWeb.GetCatalog(SPListTemplateType.WebPartCatalog);
            SPQuery query = new SPQuery();
            query.Query = string.Format("<Where><Eq><FieldRef Name=\"FileLeafRef\" /><Value Type=\"Text\">{0}</Value></Eq></Where>", filename); ;
            SPListItemCollection items = webPartGallery.GetItems(query);
            if (items.Count > 0)
            {
                items[0].Delete();
            }
        }

        public void DefineDefaultHome(SPWeb web, PublishingSite pubSite, PublishingWeb pubWeb)
        {
            pubWeb.DefaultPage = web.GetFile("default.aspx");
            pubWeb.Update();
        }

        private void PollPublishingWebActivated(string webUrl, int delay = 0)
        {
            bool poll = false;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                //TraceLogger.TraceInfo("Checking for PublishingWeb at '{0}' (attempt {1})", webUrl, attempt);
                using (SPSite site = new SPSite(webUrl))
                using (SPWeb newWeb = site.OpenWeb())
                {
                    if (PublishingWeb.IsPublishingWeb(newWeb))
                    {
                        PublishingSite pubSite = new PublishingSite(site);
                        var pubWeb = PublishingWeb.GetPublishingWeb(newWeb);
                        SPList pages = null;
                        try
                        {
                            pages = pubWeb.PagesList;
                        }
                        catch { }
                        if (pages != null)
                        {
                            pages.EnableModeration = false;
                            pages.EnableVersioning = true;
                            pages.EnableMinorVersions = false;
                            pages.MajorVersionLimit = 5;
                            pages.ForceCheckout = false;
                            pages.Update();
                        }

                        if( OnPublishingWebActivated != null) OnPublishingWebActivated(newWeb, pubSite, pubWeb);
                        if (OnPublishingActivated != null) OnPublishingActivated(webUrl);
                    }
                    else
                    {
                        //TraceLogger.TraceInfo("Web at '{0}' IS NOT a publishing Web (yet)!", webUrl);
                        poll = delay < PUBLISHING_MAX_DELAY;
                    }
                }
            });

            if (poll)
            {
                Timer timer = new System.Timers.Timer(POLL_SPEED);
                timer.AutoReset = false;
                timer.Elapsed += delegate
                {
                    PollPublishingWebActivated(webUrl, delay += POLL_SPEED);
                };
                timer.Enabled = true;
            }
        }
        
    }
}
