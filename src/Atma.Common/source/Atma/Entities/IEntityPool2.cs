namespace Atma.Entities
{
    public interface IEntityPool2
    {
        int Take();
        void Return(int id);
        ref Entity this[int index] { get; }
    }
}