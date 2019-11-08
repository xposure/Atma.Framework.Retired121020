namespace Atma.Entities
{
    public interface IEntityPool2
    {
        uint Take();
        void Return(uint entity);
        bool IsValid(uint entity);
        //ref Entity2 Get(uint entity);
        ref Entity2 this[uint index] { get; }
    }
}