using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class AttackCastContext
    {
        public readonly AttackCastType Type;
        public readonly Side Side;

        public AttackCastContext(AttackCastType type, Side side)
        {
            Type = type;
            Side = side;
        }
    }
}