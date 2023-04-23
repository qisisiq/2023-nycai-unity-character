namespace RPGCharacterAnims.Actions
{
    public class AttackCast : BaseActionHandler<AttackCastContext>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        {
			return !controller.isRelaxed && !active && controller.maintainingGround
				  && controller.canAction && controller.hasCastableWeapon;
		}

        public override bool CanEndAction(RPGCharacterController controller)
        { return controller.isCasting && active; }

        protected override void _StartAction(RPGCharacterController controller, AttackCastContext context)
        {  controller.StartCast(context.Type, context.Side); }

        protected override void _EndAction(RPGCharacterController controller)
        { controller.EndCast(); }
    }
}