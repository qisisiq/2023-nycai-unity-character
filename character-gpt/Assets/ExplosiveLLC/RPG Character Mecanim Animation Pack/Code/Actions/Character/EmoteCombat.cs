using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class EmoteCombat : InstantActionHandler<EmoteType>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        { return controller.canAction && !controller.isRelaxed; }

        protected override void _StartAction(RPGCharacterController controller, EmoteType emoteType)
        {
            switch (emoteType) {
                case EmoteType.Pickup:
                    controller.Pickup();
                    break;
                case EmoteType.Activate:
                    controller.Activate();
                    break;
                case EmoteType.Boost:
                    controller.Boost();
                    break;
            }
        }
    }
}