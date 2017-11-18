using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyberblast.SharePoint.Server.ModelFramework
{
    public static class SiteProvisioner
    {
        /// <summary>
        /// Zeichen gültig für URL Suffix / PathSegment
        /// </summary>
        private static System.Collections.Generic.List<char> validSegmentChars = new System.Collections.Generic.List<char>("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._".ToCharArray());

        /// <summary>
        /// Url Suffix anhand Site NAme vorschlagen lassen. Entfernt ungültige Zeichen. Keine Validierung ob vollständige URL gültig oder bereits belegt.
        /// </summary>
        /// <param name="siteName">Name der anzulegenden Site</param>
        /// <returns>Bereinigter Vorschlag für URL Suffix</returns>
        public static string ProposeUrlSuffix(string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
                return null;

            string urlSnippet = string.Empty;
            foreach (char iChar in siteName)
            {
                if (validSegmentChars.Contains(iChar))
                    urlSnippet += iChar;
            }
            return urlSnippet;
        }

        /// <summary>
        /// Prüft, ob sich an übergebener Adresse eine SharePoint Website (SPWeb) befindet
        /// </summary>
        /// <param name="url">Exakte URL zum SPWeb</param>
        /// <returns>true wenn sich hinter url ein SPWeb befindet, sonst false</returns>
        private static bool WebExists(string url)
        {
            bool exists = false;
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    Uri uri = new Uri(url);
                    using (SPSite site = new SPSite(url))
                    using (SPWeb web = site.OpenWeb(uri.AbsolutePath, true))
                    {
                        exists = web.Exists;
                    }
                });
            }
            catch { }
            return exists;
        }

        /// <summary>
        /// Ermittelt für eine zu übergebende Url (baseUrl) und ein zusätzliches path segment (path) für die anzulegende Subsite ein valides path segment. 
        /// Hängt eine Zahl an, wenn sich an übergebenem path bereits ein SPWeb befindet. Passt auch den Site Name entsprechend an (angehangene Zahl).
        /// </summary>
        /// <param name="baseUrl">Pfad unter welchem das neue SPWeb angelegt werden soll.</param>
        /// <param name="path">Segment, welches der baseUrl angehangen werden soll.</param>
        /// <param name="maxAttempts">Maximale anzahl an versuchen. = Maximum der anzuhängenden Zahl an path wenn URL bereits belegt ist.</param>
        /// <param name="siteName">Name der anzulegenden Site</param>
        /// <returns>Vollständige Uri die tatsächlich verwendet werden kann.</returns>
        private static Uri GetFreeUriAndName(Uri baseUrl, string path, int maxAttempts, ref string siteName)
        {
            Uri resultingUri;
            string originalSiteName = siteName;

            int counter = 1;
            resultingUri = new Uri(baseUrl, path);
            bool uriOccupied = WebExists(resultingUri.ToString());
            while (counter <= maxAttempts && uriOccupied)
            {
                counter++;
                resultingUri = new Uri(baseUrl, string.Concat(path, counter.ToString()));
                uriOccupied = WebExists(resultingUri.ToString());
                siteName = string.Format("{0} {1}", originalSiteName, counter);
            }

            return uriOccupied ? null : resultingUri;
        }

        /// <summary>
        /// erstellt eine neue Website (SPWeb)
        /// </summary>
        /// <param name="parentUrl">Url des übergeordneten Webs</param>
        /// <param name="pathSegment">Url Suffix, welches den Pfad relativ zum übergeordneten Web beschreibt</param>
        /// <param name="siteName">name der anzulegenden Site</param>
        /// <param name="localeCID">Sprache in welcher die neue Site angelegt werden soll</param>
        /// <param name="webTemplate">Schlüssel des Site Templates / Site Definition</param>
        /// <param name="errorMessage">Gibt die Nachricht eines ggf. aufgetretenen Fehler zurück.</param>
        /// <returns>Die Url des angelegten Web. null bei Misserfolg.</returns>
        public static string CreateWeb(Uri parentUrl, string pathSegment, string siteName, uint localeCID, string webTemplate, out string errorMessage)
        {
            errorMessage = string.Empty;
            string newWebUrl = null;
            Uri targetUri = GetFreeUriAndName(parentUrl, pathSegment, 100, ref siteName);
            if (targetUri == null)
                errorMessage = "Es konnte keine freie Ziel-Url ermittelt werden.";
            else
            {
                using (SPSite site = new SPSite(parentUrl.ToString()))
                {
                    try
                    {
                        Uri siteRootUri = new Uri(site.Url.EndsWith("/") ? site.Url : string.Concat(site.Url, "/"));
                        Uri siteRelativeUri = siteRootUri.MakeRelativeUri(targetUri);
                        SPWeb newWeb = site.AllWebs.Add(siteRelativeUri.ToString(), siteName, null, localeCID, webTemplate, false, false);
                        if (newWeb.Exists)
                            newWebUrl = newWeb.Url;
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    }
                }
            }
            return newWebUrl;
        }

        public delegate void WebCreatedHandler(ref SPWeb web);

        /// <summary>
        /// erstellt eine neue Website (SPWeb)
        /// </summary>
        /// <param name="parentUrl">Url des übergeordneten Webs</param>
        /// <param name="pathSegment">Url Suffix, welches den Pfad relativ zum übergeordneten Web beschreibt</param>
        /// <param name="siteName">name der anzulegenden Site</param>
        /// <param name="localeCID">Sprache in welcher die neue Site angelegt werden soll</param>
        /// <param name="webTemplate">Schlüssel des Site Templates / Site Definition</param>
        /// <param name="errorMessage">Gibt die Nachricht eines ggf. aufgetretenen Fehler zurück.</param>
        /// <param name="onCreated">Delegate, der direkt am erzeugten SPWeb Objekt ausgeführt wird.</param>
        /// <returns>Die Url des angelegten Web. null bei Misserfolg.</returns>
        public static string CreateWeb(Uri parentUrl, string pathSegment, string siteName, uint localeCID, string webTemplate, out string errorMessage, WebCreatedHandler onCreated)
        {
            errorMessage = string.Empty;
            string newWebUrl = null;
            Uri targetUri = GetFreeUriAndName(parentUrl, pathSegment, 100, ref siteName);
            if (targetUri == null)
                errorMessage = "Es konnte keine freie Ziel-Url ermittelt werden.";
            else
            {
                using (SPSite site = new SPSite(parentUrl.ToString()))
                {
                    try
                    {
                        Uri siteRootUri = new Uri(site.Url.EndsWith("/") ? site.Url : string.Concat(site.Url, "/"));
                        Uri siteRelativeUri = siteRootUri.MakeRelativeUri(targetUri);
                        SPWeb newWeb = site.AllWebs.Add(siteRelativeUri.ToString(), siteName, null, localeCID, webTemplate, false, false);
                        if (newWeb.Exists)
                            newWebUrl = newWeb.Url;
                        if (onCreated != null) 
                            onCreated(ref newWeb);
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    }
                }
            }
            return newWebUrl;
        }

    }
}
