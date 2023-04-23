using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Emote : BaseActionHandler<EmoteType>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        {
            return controller.canAction && controller.isRelaxed && !controller.isSitting
				&& !controller.isTalking && !active;
        }

        public override bool CanEndAction(RPGCharacterController controller)
        { return active; }

        protected override void _StartAction(RPGCharacterController controller, EmoteType emoteType)
        {
            switch (emoteType) {

                // Sit, Sleep and Talk all stay "on", until turned off.
                case EmoteType.Sit:
                    controller.isSitting = true;
                    controller.Sit();
                    break;
                case EmoteType.Laydown:
                    controller.isSitting = true;
                    controller.Laydown();
                    break;
                // Drink, Bow, Yes, No, Pickup, and Activate run once and exit immediately.
                case EmoteType.Drink:
                    controller.Drink();
                    EndAction(controller);
                    break;
                case EmoteType.Bow1:
                case EmoteType.Bow2:
                    controller.Bow();
                    EndAction(controller);
                    break;
                case EmoteType.Yes:
                    controller.Yes();
                    EndAction(controller);
                    break;
                case EmoteType.No:
                    controller.No();
                    EndAction(controller);
                    break;
				case EmoteType.Pickup:
                    controller.Pickup();
					EndAction(controller);
					break;
                case EmoteType.Activate:
                    controller.Activate();
					EndAction(controller);
					break;
            }
        }

        protected override void _EndAction(RPGCharacterController controller)
        {
            if (controller.isSitting) {
                controller.isSitting = false;
                controller.Stand();
            }
            if (controller.isTalking) {
                controller.isTalking = false;
                controller.EndConversation();
            }
        }
    }
}