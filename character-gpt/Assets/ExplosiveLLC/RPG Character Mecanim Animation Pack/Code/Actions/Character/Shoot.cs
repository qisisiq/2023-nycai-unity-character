using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Shoot : InstantActionHandler<EmptyContext>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        { return controller.canAction; }

        protected override void _StartAction(RPGCharacterController controller, EmptyContext context)
        {
            var attackNumber = 1;
            if (controller.rightWeapon == Weapon.Rifle && controller.isHipShooting) { attackNumber = 2; }
            controller.Shoot(attackNumber);
        }
    }
}