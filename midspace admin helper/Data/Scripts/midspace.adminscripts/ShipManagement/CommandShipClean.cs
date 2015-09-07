namespace midspace.adminscripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Sandbox.ModAPI;
    using VRage.ModAPI;

    public class CommandShipClean : ChatCommand
    {
        private List<string> methods = new List<string> (){"unpowered", "nobeacon", "deadowner", "atlarge", "istrash"};
        public CommandShipClean()
            : base(ChatCommandSecurity.Admin, "cleanships", new[] { "/cleanship" }) 
        {
        }

        public override void Help(bool brief)
        {
            MyAPIGateway.Utilities.ShowMessage("/cleanships <Condition>", "Clean the ships under one of the following condition : unpowered, nobeacon, deadowner, atlarge, istrash.");
        }

        public override bool Invoke(string messageText)
        {

            var match = Regex.Match(messageText, @"/cleanships\s{1,}(?<Key>.+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var type = match.Groups["Key"].Value;
                var typeid = methods.FindIndex( x => x == type);
                
                if (typeid == -1)
                {
                    MyAPIGateway.Utilities.ShowMessage("cleanship", "wrong method id: {}", type);
                    return false;
                }
                
                var currentShipList = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(currentShipList, e => e is Sandbox.ModAPI.IMyCubeGrid );

                switch (typeid)
                {
                    case 0://unpowered
                        break;
                    case 1://nobeacon
                        break;
                    case 2: //deadowners
                        break;
                    case 3: //atlarge
                        break;
                    case 4: //istrash
                        break;
                    default: // error method
                        MyAPIGateway.Utilities.ShowMessage("cleanship", "Method not implemented or missing");
                        break;
                }
                /*if (currentShipList.Count == 1)
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
                    MyAPIGateway.Utilities.ShowMessage("cleanship", "{0} Ships match that name.", currentShipList.Count);
                    return true;
                }

                MyAPIGateway.Utilities.ShowMessage("cleanship", "Ship name not found.");
                */
                return true;
            }

            return false;
        }

        private bool GetPower (IMyEntity shipEntity)
        {
            var grids = shipEntity.GetAttachedGrids();
            return false;

        }

        private bool AtLarge (IMyEntity shipEntity)
        {
            return false;
        }

        private bool nobeacon (IMyEntity shipEntity)
        {
            return false;
        }

        private bool deadowner(IMyEntity shipEntity)
        {
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
