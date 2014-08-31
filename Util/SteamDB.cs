/*
 * Copyright (c) 2013-2015, SteamDB. All rights reserved.
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */
using System;
using System.Collections.Generic;
using SteamKit2;
using SteamKit2.Internal;

namespace SteamDatabaseBackend
{
    static class SteamDB
    {
        private static bool ALLOW_FREE_LICENSES = false;
        public const string UNKNOWN_APP  = "SteamDB Unknown App";

        public static readonly string StringNeedToken = string.Format(" {0}(needs token){1}", Colors.RED, Colors.NORMAL);
        public static readonly string StringCheckmark = string.Format(" {0}✓{1}", Colors.GREEN, Colors.NORMAL);

        public static bool IsBusy()
        {
            return Application.ProcessorPool.InUseThreads > 15 || Application.SecondaryPool.InUseThreads > 10;
        }

        public static string GetBlogURL(string postID)
        {
            return new Uri(Settings.Current.BaseURL, string.Format("/blog/{0}/", postID)).AbsoluteUri;
        }

        public static string GetChangelistURL(uint changeNumber)
        {
            return new Uri(Settings.Current.BaseURL, string.Format("/changelist/{0}/", changeNumber)).AbsoluteUri;
        }

        public static string GetRawAppURL(uint appID)
        {
            return new Uri(Settings.Current.RawBaseURL, string.Format("/app/{0}.vdf", appID)).AbsoluteUri;
        }

        public static string GetRawPackageURL(uint subID)
        {
            return new Uri(Settings.Current.RawBaseURL, string.Format("/sub/{0}.vdf", subID)).AbsoluteUri;
        }

        public static string GetAppURL(uint appID)
        {
            return new Uri(Settings.Current.BaseURL, string.Format("/app/{0}/", appID)).AbsoluteUri;
        }

        public static string GetAppURL(uint appID, string section)
        {
            return new Uri(Settings.Current.BaseURL, string.Format("/app/{0}/{1}/", appID, section)).AbsoluteUri;
        }

        public static string GetDepotURL(uint appID, string section)
        {
            return new Uri(Settings.Current.BaseURL, string.Format("/depot/{0}/{1}/", appID, section)).AbsoluteUri;
        }

        public static string GetPackageURL(uint subID)
        {
            return new Uri(Settings.Current.BaseURL, string.Format("/sub/{0}/", subID)).AbsoluteUri;
        }

        public static string GetPackageURL(uint subID, string section)
        {
            return new Uri(Settings.Current.BaseURL, string.Format("/sub/{0}/{1}/", subID, section)).AbsoluteUri;
        }

        public static JobID RequestFreeLicense(List<uint> appids)
        {
            if (!ALLOW_FREE_LICENSES)
            {
                return null;
            }

            var clientMsg = new ClientMsgProtobuf<CMsgClientRequestFreeLicense>(EMsg.ClientRequestFreeLicense);
            clientMsg.SourceJobID = Steam.Instance.Client.GetNextJobID();

            clientMsg.Body.appids.AddRange(appids);

            Steam.Instance.Client.Send(clientMsg);

            return clientMsg.SourceJobID;
        }
    }
}
