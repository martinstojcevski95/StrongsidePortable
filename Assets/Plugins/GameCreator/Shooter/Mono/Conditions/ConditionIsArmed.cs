namespace GameCreator.Shooter
{
	using GameCreator.Core;
    using UnityEngine;

    [AddComponentMenu("")]
	public class ConditionIsArmed : ICondition
	{
        public enum ArmedType
        {
            IsArmed,
            IsUnarmed
        }

        public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Player);
        public ArmedType type = ArmedType.IsArmed;

		public override bool Check(GameObject target)
		{
            CharacterShooter charShooter = this.shooter.GetComponent<CharacterShooter>(target);
            if (charShooter == null) return this.type != ArmedType.IsArmed;

            switch (this.type)
            {
                case ArmedType.IsArmed: return charShooter.currentWeapon != null;
                case ArmedType.IsUnarmed: return charShooter.currentWeapon == null;
            }

            return false;
        }
        
		#if UNITY_EDITOR

        public static new string NAME = "Shooter/Is Armed";
        private const string NODE_TITLE = "Character {0} {1}";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.shooter,
                UnityEditor.ObjectNames.NicifyVariableName(this.type.ToString())
            );
        }

        #endif
    }
}
