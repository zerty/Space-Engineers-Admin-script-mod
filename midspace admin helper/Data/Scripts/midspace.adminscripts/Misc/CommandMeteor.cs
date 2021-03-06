﻿namespace midspace.adminscripts
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class CommandMeteor : ChatCommand
    {
        private readonly string _defaultOreName;

        public CommandMeteor(string defaultOreName)
            : base(ChatCommandSecurity.Admin, "meteor", new[] { "/meteor" })
        {
            _defaultOreName = defaultOreName;
        }

        public override void Help(bool brief)
        {
            MyAPIGateway.Utilities.ShowMessage("/meteor", "Throws a meteor in the direction you face");
        }

        public override bool Invoke(string messageText)
        {
            MatrixD worldMatrix;
            Vector3D position;

            if (MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.Parent == null)
            {
                worldMatrix = MyAPIGateway.Session.Player.Controller.ControlledEntity.GetHeadMatrix(true, true, false); // dead center of player cross hairs.
                position = worldMatrix.Translation + worldMatrix.Forward * 1.5f; // Spawn item 1.5m in front of player for safety.
            }
            else
            {
                worldMatrix = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.WorldMatrix;
                position = worldMatrix.Translation + worldMatrix.Forward * 1.5f + worldMatrix.Up * 0.5f; // Spawn item 1.5m in front of player in cockpit for safety.
            }

            var meteorBuilder = new MyObjectBuilder_Meteor()
            {
                Item = new MyObjectBuilder_InventoryItem()
                {
                    Amount = 10000,
                    Content = new MyObjectBuilder_Ore() { SubtypeName = _defaultOreName }
                },
                PersistentFlags = MyPersistentEntityFlags2.InScene, // Very important
                PositionAndOrientation = new MyPositionAndOrientation()
                {
                    Position = position,
                    Forward = (Vector3)worldMatrix.Forward,
                    Up = (Vector3)worldMatrix.Up,
                },
                LinearVelocity = worldMatrix.Forward * 300,
                Integrity = 100,
            };

            meteorBuilder.CreateAndSyncEntity();
            return true;
        }
    }
}
