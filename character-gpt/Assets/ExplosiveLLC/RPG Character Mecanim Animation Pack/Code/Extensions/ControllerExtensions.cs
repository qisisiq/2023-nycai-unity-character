using UnityEngine;

namespace RPGCharacterAnims.Extensions
{
    public static class ControllerExtensions
    {
        public static void DebugController(this RPGCharacterController controller)
        {
            Debug.Log("CONTROLLER SETTINGS---------------------------");
			Debug.Log($"AnimationSpeed:{controller.animationSpeed}   headLook:{controller.headLook}   " +
				$"isHeadlook:{controller.isHeadlook}    ladder:{controller.ladder}    cliff:{controller.cliff}  " +
				$"canAction:{controller.canAction}    canFace:{controller.canFace}    canMove:{controller.canMove}");
			Debug.Log($"canStrafe:{controller.canStrafe}   acquiringGround:{controller.acquiringGround}    " +
				$"maintainingGround:{controller.maintainingGround}    isAiming:{controller.isAiming}    isAttacking:{controller.isAttacking}    " +
				$"isBlocking:{controller.isBlocking}    isCasting:{controller.isCasting}    isClimbing:{controller.isClimbing}");
			Debug.Log($"isCrouching:{controller.isCrouching}    isCrawling:{controller.isCrawling}    isDead:{controller.isDead}    " +
				$"isFacing:{controller.isFacing}    isFalling:{controller.isFalling}    isHipShooting:{controller.isHipShooting}    " +
				$"isIdle:{controller.isIdle}    isInjured:{controller.isInjured}");
			Debug.Log($"Aiming:{controller.isAiming}    isMoving:{controller.isMoving}    isNavigating:{controller.isNavigating}    " +
				$"isNearCliff:{controller.isNearCliff}    isNearLadder:{controller.isNearLadder}    isRelaxed:{controller.isRelaxed}    " +
				$"isRolling:{controller.isRolling}    isKnockback:{controller.isKnockback}");
			Debug.Log($"isKnockdown:{controller.isKnockdown}    isSitting:{controller.isSitting}    isSpecial:{controller.isSpecial}    " +
				$"isSprinting:{controller.isSprinting}    isStrafing:{controller.isStrafing}    isSwimming:{controller.isSwimming}    " +
				$"isTalking:{controller.isTalking}    moveInput:{controller.moveInput}");
			Debug.Log($"aimInput:{controller.aimInput}    jumpInput:{controller.jumpInput}    cameraRelativeInput:{controller.cameraRelativeInput}    " +
				$"_bowPull:{controller.bowPull}    rightWeapon:{controller.rightWeapon}    leftWeapon:{controller.leftWeapon}");
			Debug.Log($"hasRightWeapon:{controller.hasRightWeapon}    hasLeftWeapon:{controller.hasLeftWeapon}    hasDualWeapons:{controller.hasDualWeapons}    " +
				$"hasTwoHandedWeapon:{controller.hasTwoHandedWeapon}    hasShield:{controller.hasShield}");
        }
    }
}