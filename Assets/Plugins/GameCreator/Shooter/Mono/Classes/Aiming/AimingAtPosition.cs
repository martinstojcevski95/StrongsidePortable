namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;

    public class AimingAtPosition : AimingAtTarget
    {
        public AimingAtPosition(CharacterShooter shooter) : base(shooter)
        {
            this.aimTo = AimTo.Position;
        }

        public override void Setup(params object[] parameters)
        {
            this.target = parameters[0] as TargetGameObject;
            base.UpdateAimToVectorFromTarget();
        }

        protected override void UpdateAimToVectorFromTarget()
        {
            return;
        }
    }
}