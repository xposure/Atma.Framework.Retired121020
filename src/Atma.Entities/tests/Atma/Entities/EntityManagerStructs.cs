#pragma warning disable 0649
namespace Atma.Entities
{
    using System;

    public unsafe struct ValidPtr
    {
        public Valid* Valid;

        public override string ToString()
        {
            return Valid->ToString();
        }
    }

    public struct Valid
    {
        public int X, Y;
        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }
    }

    public struct Valid2
    {
        public int Z;
        public override string ToString()
        {
            return $"Z: {Z}";
        }
    }

    public struct Valid3
    {
        public int W;
        public override string ToString()
        {
            return $"W: {W}";
        }
    }

    public struct Valid4
    {
        public int _;

        public override string ToString()
        {
            return $"_: {_}";
        }
    }

    public struct Valid5
    {
        public int _;
        public override string ToString()
        {
            return $"_: {_}";
        }

    }

    public struct Valid6
    {
        public int _;
        public override string ToString()
        {
            return $"_: {_}";
        }

    }

    public struct Invalid
    {
        public Object obj;
        public override string ToString()
        {
            return $"*";
        }
    }
    
    public struct ShouldFilterStruct
    {
        public Write<Valid> valid;
    }

    public struct GroupArray
    {
        public bool oddSize;
        public float dt;
        public int length;
        public Read<Entity> entity;
        public Read<Valid> valid;
        public Write<Valid2> valid2;
    }

    public struct GroupWithEntity2
    {
        public Entity entity;
        public Valid valid;
        public Valid2 valid2;
    }

    public struct Group
    {
        public Valid valid;
        public Valid2 valid2;

        public override string ToString()
        {
            return $"Group {{ valid: {valid}, valid2: {valid2} }}";
        }
    }

    [AnyComponent(typeof(Valid2), typeof(Valid3))]
    public struct GroupConditional
    {
        public readonly Valid valid;
        public Valid2 valid2;
        public Valid3 valid3;
    }


    [IgnoreComponent(typeof(Valid3))]
    public struct GroupIgnore
    {
        public readonly Valid valid;
        public Valid2 valid2;
        public Valid5 valid5;
    }

    public struct GroupWithEntity
    {
        public readonly Entity entity;
        public Valid2 valid2;
        public Valid5 valid5;
        public override string ToString()
        {
            return $"entity: {entity}, valid2: {valid2}, valid5: {valid5}";
        }
    }

}
