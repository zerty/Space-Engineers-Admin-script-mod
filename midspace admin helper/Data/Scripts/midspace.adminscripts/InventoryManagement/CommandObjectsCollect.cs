﻿namespace midspace.adminscripts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.ModAPI;
    using VRageMath;
    using midspace.adminscripts.Messages.Sync;

    public class CommandObjectsCollect : ChatCommand
    {
        private Queue<Action> _workQueue = new Queue<Action>();

        public CommandObjectsCollect()
            : base(ChatCommandSecurity.Admin, "collectobjects", new[] { "/collectobjects" })
        {
        }

        public override void Help(bool brief)
        {
            MyAPIGateway.Utilities.ShowMessage("/collectobjects <range>", "Collects any floating objects in <range> of the player to player's location.");
        }

        public override bool Invoke(string messageText)
        {
            if (messageText.StartsWith("/collectobjects ", StringComparison.InvariantCultureIgnoreCase))
            {
                var match = Regex.Match(messageText, @"/collectobjects\s{1,}(?<R>[+-]?((\d+(\.\d*)?)|(\.\d+)))", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var range = double.Parse(match.Groups["R"].Value, CultureInfo.InvariantCulture);

                    Vector3D destination;

                    if (MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.Parent == null)
                    {
                        var worldMatrix = MyAPIGateway.Session.Player.Controller.ControlledEntity.GetHeadMatrix(true, true, false);
                        destination = worldMatrix.Translation + worldMatrix.Forward * 1.5f; // Spawn item 1.5m in front of player for safety.
                    }
                    else
                    {
                        var worldMatrix = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.WorldMatrix;
                        destination = worldMatrix.Translation + worldMatrix.Forward * 1.5f + worldMatrix.Up * 0.5f; // Spawn item 1.5m in front of player in cockpit for safety.
                    }

                    var sphere = new BoundingSphereD(destination, range);
                    var floatingList = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
                    //floatingList = floatingList.Where(e => (e is Sandbox.ModAPI.IMyFloatingObject) || (e is Sandbox.ModAPI.IMyCharacter)).ToList();
                    floatingList = floatingList.Where(e => (e is Sandbox.ModAPI.IMyFloatingObject)).ToList();

                    _workQueue.Clear();

                    foreach (var item in floatingList)
                    {
                        // Check for null physics and IsPhantom, to prevent picking up primitives.
                        if (item.Physics != null && !item.Physics.IsPhantom)
                        {
                            if (item is Sandbox.ModAPI.IMyCharacter)
                            {
                                var character = item.GetObjectBuilder() as MyObjectBuilder_Character;
                                if (!character.Health.HasValue || character.Health.Value > 0) // ignore living players
                                {
                                    // TODO: not working currently. It causes body duplicates?

                                    //item.Physics.ClearSpeed();
                                    //_workQueue.Enqueue(delegate() { item.SetPosition(destination); });
                                }
                            }
                            else if (item is IMyFloatingObject)
                            {
                                // Need to queue the objects, and relocate them over a number of frames, otherwise if they 
                                // are all moved simultaneously to the same point in space, they will become stuck.
                                _workQueue.Enqueue(delegate()
                                {
                                    //item.SyncObject.UpdatePosition(); // causes Null exception.

                                    if (MyAPIGateway.Multiplayer.MultiplayerActive)
                                    {
                                        ConnectionHelper.SendMessageToAll(new MessageSyncEntityPosition() { EntityId = item.EntityId, Position = destination });
                                    }
                                    else if (item.Physics != null)
                                    {
                                        item.Physics.ClearSpeed();
                                        item.SetPosition(destination); // Doesn't sync to the server.
                                    }
                                });
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public override void UpdateBeforeSimulation100()
        {
            if (_workQueue.Count > 0)
            {
                var action = _workQueue.Dequeue();
                action.Invoke();
            }
        }
    }
}