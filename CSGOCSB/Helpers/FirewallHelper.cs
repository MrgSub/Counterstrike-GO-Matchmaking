﻿using CSGOCSB.Model;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CSGOCSB.HttpHelpers
{
    public static class FirewallHelper
    {
        public static async Task DecideToBlockOrUnblockAsync(ServerModel steamServer)
        {
            if(!ApplicationData.CurrentBlockedServers.Contains(steamServer))
            {
                BlockServer(steamServer);
            }
            else
            {
                await UnblockServerAsync(steamServer);
            }
        }

        public static void BlockServer(ServerModel steamServer)
        {
            var steamServerCommandString = String.Format(@"netsh advfirewall firewall add rule name=""[CS:GO] {0} Competitive Server"" description=""[CS:GO] Blocking the competitive servers for {0}"" dir=out action=block protocol=any remoteip={1}", steamServer.Country, steamServer.RemoteIpRange);
            FirewallExecution(steamServerCommandString);
            steamServer.BlockStatus = true;
            steamServer.BlockedColourBrush = new SolidColorBrush(Colors.Red);
            steamServer.Ping = "Blocked";
            ApplicationData.CurrentBlockedServers.Add(steamServer);
        }

        public static async Task UnblockServerAsync(ServerModel steamServer)
        {
            var steamServerCommandString = String.Format(@"netsh advfirewall firewall delete rule name=""[CS:GO] {0} Competitive Server""", steamServer.Country);
            FirewallExecution(steamServerCommandString);
            ApplicationData.CurrentBlockedServers.Remove(steamServer);
            if (!ApplicationData.IsApplicationClosing)
            {
                steamServer.Ping = "Pinging";
                await NetworkHelper.PingAsync(steamServer);
            }
        }

        public static void BlockServerWithoutModelAlterations(ServerModel steamServer)
        {
            var steamServerCommandString = String.Format(@"netsh advfirewall firewall add rule name=""[CS:GO] {0} Competitive Server"" description=""[CS:GO] Blocking the competitive servers for {0}"" dir=out action=block protocol=any remoteip={1}", steamServer.Country, steamServer.RemoteIpRange);
            FirewallExecution(steamServerCommandString);
            steamServer.BlockStatus = true;
            ApplicationData.CurrentBlockedServers.Add(steamServer);
        }

        public static void UnblockServerWithoutModelAlterations(ServerModel steamServer)
        {
            var steamServerCommandString = String.Format(@"netsh advfirewall firewall delete rule name=""[CS:GO] {0} Competitive Server""", steamServer.Country);
            FirewallExecution(steamServerCommandString);
            ApplicationData.CurrentBlockedServers.Remove(steamServer);
        }

        private static void FirewallExecution(string firewallString)
        {
            var cmdPromptProcess = Process.Start(new ProcessStartInfo(Environment.GetEnvironmentVariable("windir") + "\\System32\\cmd.exe", "/c " + firewallString)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
            cmdPromptProcess.WaitForExit();
            cmdPromptProcess.Close();
        }
    }
}
