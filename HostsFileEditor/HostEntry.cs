using System;
using System.Text;

namespace HostsFileEditor
{
    public class HostEntry
    {
        private string ipToRedirectTo = string.Empty;
        private string urlToIntercept = string.Empty;

        public HostEntry(string ip, string url)
        {
            ipToRedirectTo = ip;
            urlToIntercept = url;
        }

        public string IPToRedirectTo { get { return ipToRedirectTo; } }

        public string UrlToIntercept { get { return urlToIntercept; } }

        public override string ToString()
        {
            int separatorCount = 2;
            string separator = String.Empty;
            for (int i = 0; i < separatorCount; i++) { separator += " "; }

            StringBuilder sb = new StringBuilder();
            sb.Append(ipToRedirectTo);
            sb.Append(separator);
            sb.Append(urlToIntercept);
            return sb.ToString();
        }
    }
}