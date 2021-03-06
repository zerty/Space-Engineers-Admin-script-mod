﻿namespace midspace.adminscripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Sandbox.ModAPI;
    using VRage.ModAPI;

    public class CommandShipDelete : ChatCommand
    {
        public CommandShipDelete()
            : base(ChatCommandSecurity.Admin, "deleteship", new[] { "/deleteship" })
        {
        }

        public override void Help(bool brief)
        {
            MyAPIGateway.Utilities.ShowMessage("/deleteship <#>", "Deletes the specified <#> ship.");
        }

        public override bool Invoke(string messageText)
        {
            if (messageText.Equals("/deleteship", StringComparison.InvariantCultureIgnoreCase))
            {
                var entity = Support.FindLookAtEntity(MyAPIGateway.Session.ControlledObject, true, false, false, false, false);
                if (entity != null)
                {
                    var shipEntity = entity as Sandbox.ModAPI.IMyCubeGrid;
                    if (shipEntity != null)
                    {
                        DeleteShip(entity);
                        return true;
                    }
                }

                MyAPIGateway.Utilities.ShowMessage("deleteship", "No ship targeted.");
                return true;
            }

            var match = Regex.Match(messageText, @"/deleteship\s{1,}(?<Key>.+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var shipName = match.Groups["Key"].Value;

                var currentShipList = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(currentShipList, e => e is Sandbox.ModAPI.IMyCubeGrid && e.DisplayName.Equals(shipName, StringComparison.InvariantCultureIgnoreCase));

                if (currentShipList.Count == 1)
                {
                    DeleteShip(currentShipList.First());
                    return true;
                }
                else if (currentShipList.Count == 0)
                {
                    int index;
                    if (shipName.Substring(0, 1) == "#" && Int32.TryParse(shipName.Substring(1), out index) && index > 0 && index <= CommandListShips.ShipCache.Count && CommandListShips.ShipCache[index - 1] != null)
                    {
                        DeleteShip(CommandListShips.ShipCache[index - 1]);
                        CommandListShips.ShipCache[index - 1] = null;
                        return true;
                    }
                }
                else if (currentShipList.Count > 1)
                {
                    MyAPIGateway.Utilities.ShowMessage("deleteship", "{0} Ships match that name.", currentShipList.Count);
                    return true;
                }

                MyAPIGateway.Utilities.ShowMessage("deleteship", "Ship name not found.");
                return true;
            }

            return false;
        }

        private void DeleteShip(IMyEntity shipEntity)
        {
            var grids = shipEntity.GetAttachedGrids();

            foreach (var cubeGrid in grids)
            {
                // ejects any player prior to deleting the grid.
                cubeGrid.EjectControllingPlayers();

                var name = cubeGrid.DisplayName;

                // This will Delete the entity and sync to all.
                // Using this, also works with player ejection in the same Tick.
           
                cubeGrid.SyncObject.SendCloseRequest();

                MyAPIGateway.Utilities.ShowMessage("ship", "'{0}' deleted.", name);
            }
        }
    }
}
