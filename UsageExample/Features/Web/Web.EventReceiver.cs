using cyberblast.SharePoint.Server.ModelFramework.Provisioning;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using System;
using System.Runtime.InteropServices;
using UsageExample.Model;

namespace UsageExample.Features.Web {
    [Guid("9ed72747-4ecb-4fea-a849-04bc8ebe21ca")]
    public class WebEventReceiver : SPFeatureReceiver {
        public override void FeatureActivated(SPFeatureReceiverProperties properties) {
            SPWeb web = (SPWeb)properties.Feature.Parent;

            UsageExampleModel schema = new UsageExampleModel();
            schema.Build(web.Url);

            FeatureActivation activation = new FeatureActivation();
            activation.OnPublishingWebActivated += activation_OnPublishingWebActivated;
            activation.ActivatePublishingWeb(web.Url);
        }

        void activation_OnPublishingWebActivated(SPWeb web, PublishingSite pubSite, PublishingWeb pubWeb) {
            // do stuff requiring publishing feature activation here
            pubWeb.Navigation.InheritGlobal = true;
            pubWeb.Navigation.GlobalIncludePages = false;
            pubWeb.Navigation.GlobalIncludeSubSites = true;

            SPFile homePageFile = web.GetFile("default.aspx");
            pubWeb.DefaultPage = homePageFile;
            pubWeb.Update();
        }

        public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters) {
            SPWeb web = (SPWeb)properties.Feature.Parent;
            UsageExampleModel schema = new UsageExampleModel();
            schema.Build(web.Url);
        }        
    }
}
