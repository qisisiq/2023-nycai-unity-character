using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class CastContext
    {
        public readonly CastType Type;
        public readonly Side Side;

        public CastContext(CastType type, Side side)
        {
            Type = type;
            Side = side;
        }
    }
}