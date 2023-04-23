using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Turn : InstantActionHandler<TurnType>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        { return controller.canMove && controller.canAction; }

        protected override void _StartAction(RPGCharacterController controller, TurnType turnType)
        { controller.Turn(turnType); }
    }
}