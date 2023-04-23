using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Reload : InstantActionHandler<EmptyContext>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        {
            return !controller.isRelaxed &&
                   (controller.rightWeapon == Weapon.TwoHandCrossbow ||
                    controller.rightWeapon == Weapon.Rifle ||
                    controller.rightWeapon == Weapon.RightPistol ||
                    controller.leftWeapon == Weapon.LeftPistol);
        }

        protected override void _StartAction(RPGCharacterController controller, EmptyContext context)
        { controller.Reload(); }
    }
}