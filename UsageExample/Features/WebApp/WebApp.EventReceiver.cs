using System;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace UsageExample.Features.WebApp {
    [Guid("e1760f67-c53f-45bf-af21-997029b9b48e")]
    public class WebAppEventReceiver : SPFeatureReceiver {
        public override void FeatureActivated(SPFeatureReceiverProperties properties) {
            // This approach makes it possible to deactivate/activate the web application feature to trigger an update of all included features
            SPWebApplication webapp = properties.Feature.Parent as SPWebApplication;
            FeatureUpgrade.UpgradeFeatures(ref webapp, new Guid("24ff6b4b-1ffe-48e5-8cf7-35b7e8c76c5b"));
        }
    }
}
