namespace Atma.Entities
{
    public interface IEntityPool2
    {
        uint Take(int specIndex, int chunkIndex, int index);
        void Return(uint entity);
        bool IsValid(uint entity);
        ref readonly Entity2 Get(uint entity);
        //ref Entity this[int index] { get; }
    }
}