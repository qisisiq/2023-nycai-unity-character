using System;
using UnityEngine;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Extensions
{
    /// <summary>
    /// A set of helpers to use the RPG Character animation handler.
    /// </summary>
    public static class AnimatorExtensions
    {
        /// <summary>
        /// Sets the type of animation trigger for the RPG Character animation controller and then triggers the animation.
        /// </summary>
        /// <remarks>
        /// The animation trigger mechanic allows us to provide many different triggers through a simple standardised path,
        /// so from this you can trigger any type of AnimationTrigger without having to manually worry about the parameters
        /// associated.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="trigger">The type of trigger.</param>
        public static void SetAnimatorTrigger(this Animator animator, AnimatorTrigger trigger)
        {
			Debug.Log($"SetAnimatorTrigger: {trigger} - {( int )trigger}");
			animator.SetInteger(AnimationParameters.TriggerNumber, (int)trigger);
            animator.SetTrigger(AnimationParameters.Trigger);
        }

        /// <summary>
        /// Sets the type of animation trigger for the RPG Character animation controller with a corresponding action number.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="trigger">The type of trigger.</param>
        /// <param name="actionNumber">The type of action to set.</param>
        public static void SetActionTrigger(this Animator animator, AnimatorTrigger trigger, int actionNumber)
        {
            Debug.Log($"SetActionTrigger: {trigger} - {(int)trigger} - action {actionNumber}");
            animator.SetInteger(AnimationParameters.Action, actionNumber);
            SetAnimatorTrigger(animator, trigger);
        }

        /// <summary>
        /// Sets the animator to trigger an emote.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="emote">The type of emote to set.</param>
        public static void TriggerEmote(this Animator animator, EmoteType emote)
        { SetActionTrigger(animator, AnimatorTrigger.ActionTrigger, (int) emote); }

        /// <summary>
        /// Sets the animator side.
        /// </summary>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="side">The type of emote to set.</param>
        public static void SetSide(this Animator animator, Side side)
        { animator.SetInteger(AnimationParameters.Side, (int)side); }

        /// <summary>
        /// Sets the animator to trigger talking.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="talkType">The enum value to set.</param>
        public static void TriggerTalking(this Animator animator, TalkType talkType)
        { animator.SetInteger(AnimationParameters.Talking, (int)talkType); }

        /// <summary>
        /// Sets the animator to trigger dodging.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="dodgeType">The enum value to set.</param>
        public static void TriggerDodge(this Animator animator, DodgeType dodgeType)
        { animator.SetActionTrigger(AnimatorTrigger.DodgeTrigger, (int)dodgeType); }

        /// <summary>
        /// Sets the animator to trigger climbing.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="climbType">The enum value to set.</param>
        public static void TriggerClimb(this Animator animator, ClimbType climbType)
        { animator.SetActionTrigger(AnimatorTrigger.ClimbLadderTrigger, (int)climbType); }

        /// <summary>
        /// Sets the animator to trigger crawling.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="crawlType">The enum value to set.</param>
        public static void TriggerCrawl(this Animator animator, CrawlType crawlType)
        { animator.SetActionTrigger(AnimatorTrigger.CrawlTrigger, (int)crawlType); }

        /// <summary>
        /// Sets the animator to trigger turning.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="turnType">The enum value to set.</param>
        public static void TriggerTurn(this Animator animator, TurnType turnType)
        { animator.SetActionTrigger(AnimatorTrigger.TurnTrigger, (int)turnType); }

        /// <summary>
        /// Sets the animator to trigger dive rolling.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="rollType">The enum value to set.</param>
        public static void TriggerDiveRoll(this Animator animator, DiveRollType rollType)
        { animator.SetActionTrigger(AnimatorTrigger.DiveRollTrigger, (int)rollType); }

        /// <summary>
        /// Sets the animator to trigger rolling.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="rollType">The enum value to set.</param>
        public static void TriggerRoll(this Animator animator, RollType rollType)
        { animator.SetActionTrigger(AnimatorTrigger.RollTrigger, (int)rollType); }

        /// <summary>
        /// Sets the animator to trigger knockback.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="knockbackType">The enum value to set.</param>
        public static void TriggerKnockback(this Animator animator, KnockbackType knockbackType)
        { animator.SetActionTrigger(AnimatorTrigger.KnockbackTrigger, (int)knockbackType); }

        /// <summary>
        /// Sets the animator to trigger knockdown.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="knockdownType">The enum value to set.</param>
        public static void TriggerKnockdown(this Animator animator, KnockdownType knockdownType)
        { animator.SetActionTrigger(AnimatorTrigger.KnockdownTrigger, (int)knockdownType); }

        /// <summary>
        /// Sets the animator to trigger casting.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="castType">The enum value to set.</param>
        public static void TriggerCast(this Animator animator, CastType castType)
        { animator.SetActionTrigger(AnimatorTrigger.CastTrigger, (int)castType); }

        /// <summary>
        /// Sets the animator to trigger attack casting.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="castType">The enum value to set.</param>
        public static void TriggerAttackCast(this Animator animator, AttackCastType castType)
        { animator.SetActionTrigger(AnimatorTrigger.AttackCastTrigger, (int)castType); }


        /// <summary>
        /// Sets the animator to trigger getting hit.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="hitType">The enum value to set.</param>
        public static void TriggerGettingHit(this Animator animator, HitType hitType)
        { animator.SetActionTrigger(AnimatorTrigger.GetHitTrigger, (int)hitType); }

        /// <summary>
        /// Sets the animator to trigger getting hit.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="hitType">The raw value to set.</param>
        public static void TriggerGettingHit(this Animator animator, int hitType)
        { animator.SetActionTrigger(AnimatorTrigger.GetHitTrigger, hitType); }

        /// <summary>
        /// Sets the animator to trigger getting hit while blocking.
        /// </summary>
        /// <remarks>
        /// This builds upon the existing SetAnimationTrigger helper and allows you to provide contextual action information.
        /// </remarks>
        /// <param name="animator">The animator to act on.</param>
        /// <param name="hitType">The enum value to set.</param>
        public static void TriggerBlockGettingHit(this Animator animator, BlockedHitType hitType)
        { animator.SetActionTrigger(AnimatorTrigger.GetHitTrigger, (int)hitType); }

        /// <summary>
        /// Set Animator Trigger using legacy Animation Trigger names.
        /// </summary>
        [Obsolete("This is for backwards compatibility with older trigger names.")]
        public static void LegacySetAnimationTrigger(this Animator animator, string trigger)
        {
            var parsedTrigger = (AnimatorTrigger) Enum.Parse(typeof(AnimatorTrigger), trigger);
            Debug.Log($"LegacyAnimationTrigger: {parsedTrigger} - {parsedTrigger}");
            SetAnimatorTrigger(animator, parsedTrigger);
        }

        /// <summary>
        /// Outputs all the stats for debugging purposes.
        /// </summary>
        /// <param name="animator">The animator to debug.</param>
        public static void DebugAnimatorParameters(this Animator animator)
        {
            Debug.Log("ANIMATOR SETTINGS---------------------------");
			Debug.Log($"Aiming:{animator.GetBool(AnimationParameters.Aiming)}   Blocking:{animator.GetBool(AnimationParameters.Blocking)}   " +
				$"Crouch:{animator.GetBool(AnimationParameters.Crouch)}    Injured:{animator.GetBool(AnimationParameters.Injured)}    " +
				$"Moving:{animator.GetBool(AnimationParameters.Moving)}    Sprint:{animator.GetBool(AnimationParameters.Sprint)}    " +
				$"Stunned:{animator.GetBool(AnimationParameters.Stunned)}    Swimming:{animator.GetBool(AnimationParameters.Swimming)}    " +
				$"Action:{animator.GetInteger(AnimationParameters.Action)}    Jumping:{animator.GetInteger(AnimationParameters.Jumping)}    " +
				$"Side:{animator.GetInteger(AnimationParameters.Side)}");
			Debug.Log($"LeftWeapon:{animator.GetInteger(AnimationParameters.LeftWeapon)}    RightWeapon:{animator.GetInteger(AnimationParameters.RightWeapon)}    " +
				$"SheathLocation:{animator.GetInteger(AnimationParameters.SheathLocation)}    Talking:{animator.GetInteger(AnimationParameters.Talking)}    " +
				$"Weapon:{animator.GetInteger(AnimationParameters.Weapon)}    WeaponSwitch:{animator.GetInteger(AnimationParameters.WeaponSwitch)}    " +
				$"Idle:{animator.GetFloat(AnimationParameters.Idle)}    AimHorizontal:{animator.GetFloat(AnimationParameters.AimHorizontal)}    " +
				$"AimVertical:{animator.GetFloat(AnimationParameters.AimVertical)}    AnimationSpeed:{animator.GetFloat(AnimationParameters.AnimationSpeed)}");
			Debug.Log($"BowPull:{animator.GetFloat(AnimationParameters.BowPull)}    Charge:{animator.GetFloat(AnimationParameters.Charge)}     " +
				$"Velocity X:{animator.GetFloat(AnimationParameters.VelocityX)}    Velocity Z:{animator.GetFloat(AnimationParameters.VelocityZ)}");
		}

		/// <summary>
		/// Sets the animation state for weapons.
		/// </summary>
		/// <param name="weaponWeapon">Animator weapon number. Use AnimationData's AnimatorWeapon enum.</param>
		/// <param name="weaponSwitch">Weapon switch. -2 leaves parameter unchanged.</param>
		/// <param name="leftWeapon">Left weapon number. Use Weapon.cs enum.</param>
		/// <param name="rightWeapon">Right weapon number. Use Weapon.cs enum.</param>
		/// <param name="weaponSide">Weapon side: 0-None, 1-Left, 2-Right, 3-Dual.</param>
		public static void SetWeapons(this Animator animator, AnimatorWeapon animatorWeapon, int weaponSwitch, Weapon leftWeapon, Weapon rightWeapon, Side weaponSide)
		{
			{ animator.SetInteger(AnimationParameters.Weapon, ( int )animatorWeapon); }
			if (weaponSwitch != -2) { animator.SetInteger(AnimationParameters.WeaponSwitch, weaponSwitch); }
			if (leftWeapon != Weapon.Relax) { animator.SetInteger(AnimationParameters.LeftWeapon, ( int )leftWeapon); }
			if (rightWeapon != Weapon.Relax) { animator.SetInteger(AnimationParameters.RightWeapon, ( int )rightWeapon); }
			if (weaponSide != Side.Unchanged) { animator.SetInteger(AnimationParameters.Side, ( int )weaponSide); }
		}
	}
}