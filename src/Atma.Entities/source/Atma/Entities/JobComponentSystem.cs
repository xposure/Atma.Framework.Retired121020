namespace Atma.Entities
{
    using Atma.Jobs;

    public abstract partial class JobComponentSystem : ComponentSystem
    {
        protected internal JobManager JobManager { get; internal set; }



        protected override void Update()
        {
            BeforeUpdate();
            throw new System.Exception();
            AfterUpdate();
        }

        protected virtual void BeforeUpdate() { }

        public virtual void AfterUpdate() { }


    }
}
