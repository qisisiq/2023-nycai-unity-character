using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims.Actions
{
    public class ClimbLadder : MovementActionHandler<EmptyContext>
    {
        public ClimbLadder(RPGCharacterMovementController movement) : base(movement)
        {
        }

        public override bool CanStartAction(RPGCharacterController controller)
        { return !IsActive() && controller.isNearLadder; }

        protected override void _StartAction(RPGCharacterController controller, EmptyContext context)
        {
            var ladder = controller.ladder;
            var superCharacterController = movement.GetComponent<SuperCharacterController>();

            var ladderTop = new Vector3(ladder.transform.position.x, ladder.bounds.max.y, ladder.transform.position.z);
            var ladderBottom = new Vector3(ladder.transform.position.x, ladder.bounds.min.y, ladder.transform.position.z);
            var distanceFromTop = (controller.transform.position - ladderTop).magnitude;
            var distanceFromBottom = (controller.transform.position - ladderBottom).magnitude;

            // If the top of the ladder is below the character's head, climb onto the top of the ladder.
            if (distanceFromTop < distanceFromBottom) {
                movement.ClimbLadder(false);
                controller.ClimbLadder(ClimbType.MountTop);
                movement.currentState = CharacterState.ClimbLadder;
            }
			else if (distanceFromBottom < distanceFromTop) {
                movement.ClimbLadder(true);
                controller.ClimbLadder(ClimbType.MountBottom);
                movement.currentState = CharacterState.ClimbLadder;
            }
        }

        public override bool IsActive()
        { return movement.currentState != null && (CharacterState)movement.currentState == CharacterState.ClimbLadder; }
    }
}