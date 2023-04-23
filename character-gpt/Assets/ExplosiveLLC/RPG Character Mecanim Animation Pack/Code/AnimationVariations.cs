using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims
{
    /// <summary>
    /// This class contains all the variations for given action types.
    /// </summary>
    public class AnimationVariations
    {
        public static readonly EmoteType[] Bow = {EmoteType.Bow1, EmoteType.Bow2};
        public static readonly AttackCastType[] AttackCast = {AttackCastType.Cast1, AttackCastType.Cast2, AttackCastType.Cast3};
        public static readonly CrawlType[] CrawlTypes = {CrawlType.Crawl};
        public static readonly CastType[] Buff = { CastType.Buff1, CastType.Buff2 };
        public static readonly CastType[] AOE = { CastType.AOE1, CastType.AOE2 };
        public static readonly CastType[] Summon = { CastType.Summon1, CastType.Summon2 };
        public static readonly KickType[] LeftKicks = { KickType.LeftKick1, KickType.LeftKick2 };
        public static readonly KickType[] RightKicks = { KickType.RightKick1, KickType.RightKick2 };
        public static readonly KnockbackType[] Knockbacks = { KnockbackType.Knockback1, KnockbackType.Knockback2 };
        public static readonly KnockdownType[] Knockdowns = { KnockdownType.Knockdown1 };
        public static readonly HitType[] Hits = { HitType.Forward1, HitType.Forward2, HitType.Back1, HitType.Left1, HitType.Right1 };
        public static readonly BlockedHitType[] BlockedHits = { BlockedHitType.BlockedHit1, BlockedHitType.BlockedHit2 };
        public static readonly ShootingAttack[] ShootingAttacks = { ShootingAttack.Attack1, ShootingAttack.Attack2 };

        public static readonly TalkType[] Conversations =
		{
            TalkType.Talk1, TalkType.Talk2, TalkType.Talk3, TalkType.Talk4,
            TalkType.Talk5, TalkType.Talk6, TalkType.Talk7, TalkType.Talk8
        };

        public static readonly TwoHandedSwordAttack[] TwoHandedSwordAttacks =
        {
            TwoHandedSwordAttack.Attack1, TwoHandedSwordAttack.Attack2, TwoHandedSwordAttack.Attack3,
            TwoHandedSwordAttack.Attack4, TwoHandedSwordAttack.Attack5, TwoHandedSwordAttack.Attack6,
            TwoHandedSwordAttack.Attack7, TwoHandedSwordAttack.Attack8, TwoHandedSwordAttack.Attack9,
            TwoHandedSwordAttack.Attack10, TwoHandedSwordAttack.Attack11
        };

        public static readonly TwoHandedSpearAttack[] TwoHandedSpearAttacks =
        {
            TwoHandedSpearAttack.Attack1, TwoHandedSpearAttack.Attack2, TwoHandedSpearAttack.Attack3,
            TwoHandedSpearAttack.Attack4, TwoHandedSpearAttack.Attack5, TwoHandedSpearAttack.Attack6,
            TwoHandedSpearAttack.Attack7, TwoHandedSpearAttack.Attack8, TwoHandedSpearAttack.Attack9,
            TwoHandedSpearAttack.Attack10, TwoHandedSpearAttack.Attack11
        };

        public static readonly TwoHandedAxeAttack[] TwoHandedAxeAttacks =
        {
            TwoHandedAxeAttack.Attack1, TwoHandedAxeAttack.Attack2, TwoHandedAxeAttack.Attack3,
            TwoHandedAxeAttack.Attack4, TwoHandedAxeAttack.Attack5, TwoHandedAxeAttack.Attack6
        };

        public static readonly TwoHandedBowAttack[] TwoHandedBowAttacks =
        {
            TwoHandedBowAttack.Attack1, TwoHandedBowAttack.Attack2, TwoHandedBowAttack.Attack3,
            TwoHandedBowAttack.Attack4, TwoHandedBowAttack.Attack5, TwoHandedBowAttack.Attack6
        };

        public static readonly TwoHandedCrossbowAttack[] TwoHandedCrossbowAttacks =
        {
            TwoHandedCrossbowAttack.Attack1, TwoHandedCrossbowAttack.Attack2, TwoHandedCrossbowAttack.Attack3,
            TwoHandedCrossbowAttack.Attack4, TwoHandedCrossbowAttack.Attack5, TwoHandedCrossbowAttack.Attack6
        };

        public static readonly TwoHandedCrossbowAttack[] TwoHandedStaffAttacks =
        {
            TwoHandedCrossbowAttack.Attack1, TwoHandedCrossbowAttack.Attack2, TwoHandedCrossbowAttack.Attack3,
            TwoHandedCrossbowAttack.Attack4, TwoHandedCrossbowAttack.Attack5, TwoHandedCrossbowAttack.Attack6
        };

        public static readonly UnarmedAttack[] UnarmedLeftAttacks =
        { UnarmedAttack.LeftAttack1, UnarmedAttack.LeftAttack2, UnarmedAttack.LeftAttack3 };

        public static readonly UnarmedAttack[] UnarmedRightAttacks =
        { UnarmedAttack.RightAttack1, UnarmedAttack.RightAttack2, UnarmedAttack.RightAttack3 };

        public static readonly ShieldAttack[] ShieldAttacks =
		{ ShieldAttack.Attack1, ShieldAttack.Attack2, ShieldAttack.Attack3 };

        public static readonly SwordAttack[] LeftSwordAttacks =
        {
            SwordAttack.LeftAttack1, SwordAttack.LeftAttack2, SwordAttack.LeftAttack3,
            SwordAttack.LeftAttack4, SwordAttack.LeftAttack5, SwordAttack.LeftAttack6,
            SwordAttack.LeftAttack7
        };

        public static readonly SwordAttack[] RightSwordAttacks =
        {
            SwordAttack.RightAttack1, SwordAttack.RightAttack2, SwordAttack.RightAttack3,
            SwordAttack.RightAttack4, SwordAttack.RightAttack5, SwordAttack.RightAttack6,
            SwordAttack.RightAttack7
        };

        public static readonly MaceAttack[] LeftMaceAttacks =
        { MaceAttack.LeftAttack1, MaceAttack.LeftAttack2, MaceAttack.LeftAttack3 };

        public static readonly MaceAttack[] RightMaceAttacks =
        { MaceAttack.RightAttack1, MaceAttack.RightAttack2, MaceAttack.RightAttack3 };

        public static readonly DaggerAttack[] LeftDaggerAttacks =
        { DaggerAttack.LeftAttack1, DaggerAttack.LeftAttack2, DaggerAttack.LeftAttack3 };

        public static readonly DaggerAttack[] RightDaggerAttacks =
        { DaggerAttack.RightAttack1, DaggerAttack.RightAttack2, DaggerAttack.RightAttack3 };

        public static readonly PistolAttack[] LeftPistolAttacks =
        { PistolAttack.LeftAttack1, PistolAttack.LeftAttack2, PistolAttack.LeftAttack3 };

        public static readonly PistolAttack[] RightPistolAttacks =
        { PistolAttack.RightAttack1, PistolAttack.RightAttack2, PistolAttack.RightAttack3 };

        public static readonly ItemAttack[] LeftItemAttacks =
        { ItemAttack.LeftAttack1, ItemAttack.LeftAttack2, ItemAttack.LeftAttack3, ItemAttack.LeftAttack4 };

        public static readonly ItemAttack[] RightItemAttacks =
        { ItemAttack.RightAttack1, ItemAttack.RightAttack2, ItemAttack.RightAttack3, ItemAttack.RightAttack4 };

        public static readonly SpearAttack[] RightSpearAttacks =
        {
            SpearAttack.RightAttack1, SpearAttack.RightAttack2, SpearAttack.RightAttack3,
            SpearAttack.RightAttack4, SpearAttack.RightAttack5, SpearAttack.RightAttack6
        };

        public static readonly DualAttack[] DualAttacks =
        { DualAttack.Attack1, DualAttack.Attack2, DualAttack.Attack3 };
    }
}