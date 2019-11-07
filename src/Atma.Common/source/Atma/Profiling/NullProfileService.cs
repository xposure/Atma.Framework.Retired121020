using Atma.DI;

namespace Atma.Profiling
{
    [DefaultService]
    public class NullProfileService : IProfileService
    {
        public readonly static NullProfileService Null = new NullProfileService();

        private class ProfileThread : IProfileThread
        {
            private class ProfileFrame : IProfileFrame
            {
                public double End => 1;

                public double Start => 0;

                public double Timer => 1;

                public ProfileScope Begin(string name)
                {
                    return new ProfileScope(this, 0, 0, 0);
                }

                public void Pop(in ProfileScope scope)
                {
                }
            }

            private static readonly ProfileFrame Frame = new ProfileFrame();
            public IProfileFrame CurrentFrame => Frame;

            public ProfileScope Begin(string name) => Frame.Begin(name);
        }

        private static readonly ProfileThread Thread = new ProfileThread();

        public IProfileThread Current => Thread;

        public IProfileThread AddProfileThread() => Thread;

        public IProfileThread GetProfileThread() => Thread;

        public void BeginFrame()
        {

        }

        public void EndFrame()
        {

        }
    }
}
