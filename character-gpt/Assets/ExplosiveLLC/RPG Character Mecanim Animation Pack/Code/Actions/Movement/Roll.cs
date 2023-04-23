using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Roll : MovementActionHandler<RollType>
    {
        public Roll(RPGCharacterMovementController movement) : base(movement)
        {
        }

        public override bool CanStartAction(RPGCharacterController controller)
        { return controller.canAction && !controller.IsActive("Relax"); }

        protected override void _StartAction(RPGCharacterController controller, RollType rollType)
        {
            controller.Roll(rollType);
            movement.currentState = CharacterState.Roll;
		}

        public override bool IsActive()
        { return movement.currentState != null && (CharacterState)movement.currentState == CharacterState.Roll; }
    }
}