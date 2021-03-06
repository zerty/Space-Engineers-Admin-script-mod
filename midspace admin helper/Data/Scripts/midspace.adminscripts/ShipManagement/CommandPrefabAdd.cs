﻿namespace midspace.adminscripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class CommandPrefabAdd : ChatCommand
    {
        public CommandPrefabAdd()
            : base(ChatCommandSecurity.Admin, "addprefab", new[] { "/addprefab" })
        {
        }

        public override void Help(bool brief)
        {
            MyAPIGateway.Utilities.ShowMessage("/addprefab <#>", "Add the specified <#> prefab. Spawns the specified a ship 2m directly in front of player.");
        }

        public override bool Invoke(string messageText)
        {
            if (messageText.StartsWith("/addprefab ", StringComparison.InvariantCultureIgnoreCase))
            {
                var match = Regex.Match(messageText, @"/addprefab\s{1,}(?<Key>.+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var prefabName = match.Groups["Key"].Value;
                    var prefabKvp = MyDefinitionManager.Static.GetPrefabDefinitions().FirstOrDefault(kvp => kvp.Key.Equals(prefabName, StringComparison.InvariantCultureIgnoreCase));
                    MyPrefabDefinition prefab = null;

                    if (prefabKvp.Value != null)
                    {
                        prefab = prefabKvp.Value;
                    }

                    int index;
                    if (prefabName.Substring(0, 1) == "#" && Int32.TryParse(prefabName.Substring(1), out index) && index > 0 && index <= CommandListPrefabs.PrefabCache.Count)
                    {
                        prefab = CommandListPrefabs.PrefabCache[index - 1];
                    }

                    if (prefab != null)
                    {
                        if (prefab.CubeGrids == null)
                        {
                            MyDefinitionManager.Static.ReloadPrefabsFromFile(prefab.PrefabPath);
                            prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefab.Id.SubtypeName);
                        }

                        if (prefab.CubeGrids.Count() == 0)
                            return true;

                        var worldMatrix = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.WorldMatrix;
                        
                        // Use the cubeGrid BoundingBox to determine distance to place.
                        Vector3I min = Vector3I.MaxValue;
                        Vector3I max = Vector3I.MinValue;
                        foreach (var b in prefab.CubeGrids[0].CubeBlocks)
                        {
                            min = Vector3I.Min(b.Min, min);
                            max = Vector3I.Max(b.Min, max);
                        }
                        var size = new Vector3(max - min);
                        var distance = (Math.Sqrt(size.LengthSquared()) * prefab.CubeGrids[0].GridSizeEnum.ToGridLength() / 2) + 2;
                        var position = worldMatrix.Translation + worldMatrix.Forward * distance; // offset the position out in front of player by 2m.
                        var offset = position - prefab.CubeGrids[0].PositionAndOrientation.Value.Position;
                        var tempList = new List<MyObjectBuilder_EntityBase>();

                        // We SHOULD NOT make any changes directly to the prefab, we need to make a Value copy using Clone(), and modify that instead.
                        foreach (var grid in prefab.CubeGrids)
                        {
                            var gridBuilder = (MyObjectBuilder_CubeGrid)grid.Clone();
                            gridBuilder.PositionAndOrientation = new MyPositionAndOrientation(grid.PositionAndOrientation.Value.Position + offset, grid.PositionAndOrientation.Value.Forward, grid.PositionAndOrientation.Value.Up);
                            tempList.Add(gridBuilder);
                        }

                        tempList.CreateAndSyncEntities();
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
