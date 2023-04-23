using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Talk : BaseActionHandler<int>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        {
            return controller.canAction && controller.isRelaxed && !controller.isSitting
				&& !controller.isTalking && !active;
        }

        public override bool CanEndAction(RPGCharacterController controller)
        { return active; }

        protected override void _StartAction(RPGCharacterController controller, int context)
        {
            controller.isTalking = true;
            controller.StartConversation();
        }

        protected override void _EndAction(RPGCharacterController controller)
        {
            if (!controller.isTalking) { return; }
            controller.isTalking = false;
            controller.EndConversation();
        }
    }
}