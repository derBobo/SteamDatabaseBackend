﻿/*
 * Copyright (c) 2013-present, SteamDB. All rights reserved.
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace SteamDatabaseBackend
{
    internal class ImportantCommand : Command
    {
        public ImportantCommand()
        {
            Trigger = "important";
            IsAdminCommand = true;
        }

        public override async Task OnCommand(CommandArguments command)
        {
            if (command.CommandType != ECommandType.IRC || !IRC.IsRecipientChannel(command.Recipient))
            {
                command.Reply("This command is only available in channels.");

                return;
            }

            var channel = command.Recipient;

            var s = command.Message.Split(' ');
            var count = s.Length;

            if (count > 0)
            {
                uint id;
                switch (s[0])
                {
                    case "reload":
                        await Application.ReloadImportant(command);
                        PICSTokens.Reload(command);
                        FileDownloader.ReloadFileList();

                        return;

                    case "add":
                        if (count < 3)
                        {
                            break;
                        }

                        if (!uint.TryParse(s[2], out id))
                        {
                            break;
                        }

                        switch (s[1])
                        {
                            case "app":
                                var exists = Application.ImportantApps.TryGetValue(id, out var channels);

                                if (exists && channels.Contains(channel))
                                {
                                    command.Reply("App {0}{1}{2} ({3}) is already important in {4}{5}{6}.", Colors.BLUE, id, Colors.NORMAL, Steam.GetAppName(id), Colors.BLUE, channel, Colors.NORMAL);
                                }
                                else
                                {
                                    if (exists)
                                    {
                                        Application.ImportantApps[id].Add(channel);
                                    }
                                    else
                                    {
                                        Application.ImportantApps.Add(id, new List<string> { channel });
                                    }

                                    await using (var db = await Database.GetConnectionAsync())
                                    {
                                        await db.ExecuteAsync("INSERT INTO `ImportantApps` (`AppID`, `Channel`) VALUES (@AppID, @Channel)", new { AppID = id, Channel = channel });
                                    }

                                    command.Reply("Marked app {0}{1}{2} ({3}) as important in {4}{5}{6}.", Colors.BLUE, id, Colors.NORMAL, Steam.GetAppName(id), Colors.BLUE, channel, Colors.NORMAL);
                                }

                                return;

                            case "sub":
                                if (Application.ImportantSubs.ContainsKey(id))
                                {
                                    command.Reply("Package {0}{1}{2} ({3}) is already important.", Colors.BLUE, id, Colors.NORMAL, Steam.GetPackageName(id));
                                }
                                else
                                {
                                    Application.ImportantSubs.Add(id, 1);

                                    await using (var db = await Database.GetConnectionAsync())
                                    {
                                        await db.ExecuteAsync("INSERT INTO `ImportantSubs` (`SubID`) VALUES (@SubID)", new { SubID = id });
                                    }

                                    command.Reply("Marked package {0}{1}{2} ({3}) as important.", Colors.BLUE, id, Colors.NORMAL, Steam.GetPackageName(id));
                                }

                                return;
                        }

                        break;

                    case "remove":
                        if (count < 3)
                        {
                            break;
                        }

                        if (!uint.TryParse(s[2], out id))
                        {
                            break;
                        }

                        switch (s[1])
                        {
                            case "app":
                                if (!Application.ImportantApps.TryGetValue(id, out var channels) || !channels.Contains(channel))
                                {
                                    command.Reply("App {0}{1}{2} ({3}) is not important in {4}{5}{6}.", Colors.BLUE, id, Colors.NORMAL, Steam.GetAppName(id), Colors.BLUE, channel, Colors.NORMAL);
                                }
                                else
                                {
                                    if (channels.Count > 1)
                                    {
                                        Application.ImportantApps[id].Remove(channel);
                                    }
                                    else
                                    {
                                        Application.ImportantApps.Remove(id);
                                    }

                                    await using (var db = await Database.GetConnectionAsync())
                                    {
                                        await db.ExecuteAsync("DELETE FROM `ImportantApps` WHERE `AppID` = @AppID AND `Channel` = @Channel", new { AppID = id, Channel = channel });
                                    }

                                    command.Reply("Removed app {0}{1}{2} ({3}) from the important list in {4}{5}{6}.", Colors.BLUE, id, Colors.NORMAL, Steam.GetAppName(id), Colors.BLUE, channel, Colors.NORMAL);
                                }

                                return;

                            case "sub":
                                if (!Application.ImportantSubs.ContainsKey(id))
                                {
                                    command.Reply("Package {0}{1}{2} ({3}) is not important.", Colors.BLUE, id, Colors.NORMAL, Steam.GetPackageName(id));
                                }
                                else
                                {
                                    Application.ImportantSubs.Remove(id);

                                    await using (var db = await Database.GetConnectionAsync())
                                    {
                                        await db.ExecuteAsync("DELETE FROM `ImportantSubs` WHERE `SubID` = @SubID", new { SubID = id });
                                    }

                                    command.Reply("Removed package {0}{1}{2} ({3}) from the important list.", Colors.BLUE, id, Colors.NORMAL, Steam.GetPackageName(id));
                                }

                                return;
                        }

                        break;
                }
            }

            command.Reply("Usage:{0} important reload {1}or{2} important <add/remove> <app/sub> <id>", Colors.OLIVE, Colors.NORMAL, Colors.OLIVE);
        }
    }
}
