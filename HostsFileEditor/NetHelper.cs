using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace HostsFileEditor
{
    public static class NetHelper
    {
        public static bool ValidateIP(string ipString)
        {
            if (!ipString.Contains(":"))
            {
                // Likely IPv4 address
                if (ipString.Count(c => c == '.') != 3) return false;
                IPAddress address;
                return IPAddress.TryParse(ipString, out address);
            }
            else
            {
                // Might be IPv6 address
                IPAddress address;
                return IPAddress.TryParse(ipString, out address);
            }
        }

        public static List<HostEntry> ParseHosts(string content)
        {
            List<HostEntry> hosts = new List<HostEntry>();

            // Separate lines
            string[] lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (!line.StartsWith("#")) // Check if the line is a comment
                {
                    char[] charsToTrim = { ' ', '\t' };
                    string mainLine = line.Trim(charsToTrim);

                    // Remove extra spaces
                    mainLine = Regex.Replace(mainLine, @"\s+", " ");
                    Debug.WriteLine(mainLine);

                    // Split the string
                    string[] objects = mainLine.Split(' ');
                    Debug.WriteLine(objects.Length);

                    if (objects.Length % 2 == 0) // Check if count is a multiple of 2
                    {
                        for (int i = 0; i < objects.Length; i += 2)
                        {
                            string ipAddr = objects[i];
                            string hostName = objects[i + 1];

                            Debug.WriteLine(ipAddr);
                            Debug.WriteLine(hostName);
                            Debug.WriteLine("---");

                            if (ValidateIP(ipAddr))
                            {
                                hosts.Add(new HostEntry(ipAddr, hostName));
                            }
                        }
                    }
                }
            }
            return hosts;
        }

        public static string ReformatHosts(string content)
        {
            StringBuilder sb = new StringBuilder();

            // Separate lines
            string[] lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (!line.StartsWith("#")) // Check if the line is a comment
                {
                    char[] charsToTrim = { ' ', '\t' };
                    string mainLine = line.Trim(charsToTrim);

                    // Remove extra spaces
                    mainLine = Regex.Replace(mainLine, @"\s+", " ");
                    //Debug.WriteLine(mainLine);

                    // Split the string
                    string[] objects = mainLine.Split(' ');
                    //Debug.WriteLine(objects.Length);

                    if (objects.Length % 2 == 0) // Check if count is a multiple of 2
                    {
                        for (int i = 0; i < objects.Length; i += 2)
                        {
                            string ipAddr = objects[i];
                            string hostName = objects[i + 1];
                            sb.AppendLine(new HostEntry(ipAddr, hostName).ToString());
                        }
                    }
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }
    }
}