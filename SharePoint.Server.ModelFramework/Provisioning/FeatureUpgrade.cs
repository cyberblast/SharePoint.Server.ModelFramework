using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;

namespace UsageExample {
    public static class FeatureUpgrade
    {
        public static void UpgradeFeatures(ref SPWebApplication webApp, Guid featureId)
        {
            SPFeatureQueryResultCollection features = null;
            try
            {
                features = webApp.QueryFeatures(featureId, true);
            }
            catch {// not contained 
            } 
            if (features != null)
            {
                IEnumerator<SPFeature> featureEnumerator = features.GetEnumerator();
                featureEnumerator.Reset();
                while (featureEnumerator.MoveNext())
                {
                    try
                    {
                        SPFeature feature = featureEnumerator.Current;
                        feature.Upgrade(false);
                    }
                    catch (Microsoft.SharePoint.SPFeatureIsOrphanedException oEx) { }
                }
            }
        }

        public static void UpgradeFeatures(ref SPSite site, Guid featureId)
        {
            SPFeatureQueryResultCollection features = null;
            try
            {
                features = site.QueryFeatures(featureId, true);
            }
            catch {// not contained 
            }

            if (features != null) {
                IEnumerator<SPFeature> featureEnumerator = features.GetEnumerator();
                featureEnumerator.Reset();
                while (featureEnumerator.MoveNext()) {
                    SPFeature feature = featureEnumerator.Current;
                    feature.Upgrade(false);
                }
            }
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
        public static void RetractWebPartFile(ref SPWeb rootWeb, string filename)
        {
            List<SPFile> FilesToDelete = new List<SPFile>();
            SPList webPartGallery = rootWeb.GetCatalog(SPListTemplateType.WebPartCatalog);
            SPQuery query = new SPQuery();
            query.Query = string.Format("<Where><Eq><FieldRef Name=\"FileLeafRef\" /><Value Type=\"Text\">{0}</Value></Eq></Where>", filename); ;
            SPListItemCollection items = webPartGallery.GetItems(query);
            if (items.Count > 0)
            {
                items[0].Delete();
            }
        }
    }
}
