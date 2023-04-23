using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class Swim : MovementActionHandler<EmptyContext>
    {
        public Swim(RPGCharacterMovementController movement) : base(movement)
        {
        }

        public override bool CanStartAction(RPGCharacterController controller)
        { return !IsActive(); }

        protected override void _StartAction(RPGCharacterController controller, EmptyContext context)
        {
            movement.currentState = CharacterState.Swim;
			controller.SetIKOff();
		}

        public override bool IsActive()
        { return movement.currentState != null && (CharacterState)movement.currentState == CharacterState.Swim; }
    }
}