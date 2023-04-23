using UnityEngine;

namespace RPGCharacterAnims.Lookups
{
    /* This contains all the hash based lookups for animations.
     *
     * As this is not a static class you can inherit from this yourself if you add your
     * own custom animations in your own use cases.
     */
    public class AnimationParameters
    {
        public static readonly int TriggerNumber = Animator.StringToHash("TriggerNumber");
        public static readonly int Trigger = Animator.StringToHash("Trigger");
        public static readonly int Moving = Animator.StringToHash("Moving");
        public static readonly int Aiming = Animator.StringToHash("Aiming");
        public static readonly int Stunned = Animator.StringToHash("Stunned");
        public static readonly int Swimming = Animator.StringToHash("Swimming");
        public static readonly int Blocking = Animator.StringToHash("Blocking");
        public static readonly int Injured = Animator.StringToHash("Injured");
        public static readonly int Weapon = Animator.StringToHash("Weapon");
        public static readonly int WeaponSwitch = Animator.StringToHash("WeaponSwitch");
        public static readonly int Side = Animator.StringToHash("Side");
        public static readonly int LeftWeapon = Animator.StringToHash("LeftWeapon");
        public static readonly int RightWeapon = Animator.StringToHash("RightWeapon");
        public static readonly int Jumping = Animator.StringToHash("Jumping");
        public static readonly int Action = Animator.StringToHash("Action");
        public static readonly int SheathLocation = Animator.StringToHash("SheathLocation");
        public static readonly int Talking = Animator.StringToHash("Talking");
        public static readonly int VelocityX = Animator.StringToHash("Velocity X");
        public static readonly int VelocityZ = Animator.StringToHash("Velocity Z");
        public static readonly int AimHorizontal = Animator.StringToHash("AimHorizontal");
        public static readonly int AimVertical = Animator.StringToHash("AimVertical");
        public static readonly int BowPull = Animator.StringToHash("BowPull");
        public static readonly int Charge = Animator.StringToHash("Charge");
        public static readonly int AnimationSpeed = Animator.StringToHash("AnimationSpeed");
        public static readonly int Sprint = Animator.StringToHash("Sprint");
        public static readonly int Crouch = Animator.StringToHash("Crouch");
        public static readonly int Idle = Animator.StringToHash("Idle");
    }
}