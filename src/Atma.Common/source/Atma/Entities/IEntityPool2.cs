namespace Atma.Entities
{
    public interface IEntityPool2
    {
        uint Take(int specIndex, int chunkIndex, int index);
        void Return(uint id);
        bool IsValid(uint id);
        //ref Entity this[int index] { get; }
    }
}