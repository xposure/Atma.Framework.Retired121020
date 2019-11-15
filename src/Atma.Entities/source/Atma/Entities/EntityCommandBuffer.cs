namespace Atma.Entities
{
    using System;
    using Atma.Memory;

    public unsafe readonly ref struct EntityCommandBuffer //: UnmanagedDispose
    {
        private enum CommandTypes
        {
            CreateEntity,
            RemoveEntity,
            AssignComponent,
            UpdateComponent,
            ReplaceComponent,
            RemoveComponent
        }

        private struct EntityCommand
        {
            public CommandTypes CommandType;
            public int Size;

            public EntityCommand(CommandTypes type)
            {
                CommandType = type;
                Size = sizeof(EntityCommand);
            }

            public void Process(EntityManager entityManager)
            {
                throw new NotImplementedException();
            }
        }

        private struct CreateEntityCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public int ComponentCount;

            public CreateEntityCommand(int componentCount)
            {
                CommandType = CommandTypes.CreateEntity;
                Size = sizeof(CreateEntityCommand);
                ComponentCount = componentCount;
            }

            public static uint Process(EntityManager entityManager, CreateEntityCommand* it)
            {
                Span<ComponentType> componentTypes = new Span<ComponentType>((void*)(it + 1), it->ComponentCount);
                return entityManager.Create(componentTypes);
            }
        }

        private struct RemoveEntityCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public uint Entity;

            public RemoveEntityCommand(uint entity)
            {
                CommandType = CommandTypes.RemoveEntity;
                Size = sizeof(RemoveEntityCommand);
                Entity = entity;
            }

            public static void Process(EntityManager entityManager, RemoveEntityCommand* it)
            {
                entityManager.Remove(it->Entity);
            }
        }

        private struct ReplaceComponentCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public uint Entity;
            public ComponentType ComponentType;

            public ReplaceComponentCommand(uint entity, in ComponentType componentType)
            {
                CommandType = CommandTypes.ReplaceComponent;
                Size = sizeof(ReplaceComponentCommand);
                Entity = entity;
                ComponentType = componentType;
            }

            public static void Process(EntityManager entityManager, ReplaceComponentCommand* it, uint lastEntity)
            {
                entityManager.Replace(it->Entity > 0u ? it->Entity : lastEntity, &it->ComponentType, (void*)(it + 1));
            }
        }

        private struct UpdateComponentCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public uint Entity;
            public ComponentType ComponentType;

            public UpdateComponentCommand(uint entity, in ComponentType componentType)
            {
                CommandType = CommandTypes.UpdateComponent;
                Size = sizeof(UpdateComponentCommand);
                Entity = entity;
                ComponentType = componentType;
            }

            public static void Process(EntityManager entityManager, UpdateComponentCommand* it, uint lastEntity)
            {
                entityManager.Update(it->Entity > 0u ? it->Entity : lastEntity, &it->ComponentType, (void*)(it + 1));
            }
        }

        private struct AssignComponentCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public uint Entity;
            public ComponentType ComponentType;

            public AssignComponentCommand(uint entity, in ComponentType componentType)
            {
                CommandType = CommandTypes.AssignComponent;
                Size = sizeof(AssignComponentCommand);
                Entity = entity;
                ComponentType = componentType;
            }

            public static void Process(EntityManager entityManager, AssignComponentCommand* it, uint lastEntity)
            {
                entityManager.Assign(it->Entity > 0u ? it->Entity : lastEntity, &it->ComponentType, (void*)(it + 1));
            }
        }

        //TODO: the old system would stack commands on top and run them once for performance
        private struct RemoveComponentCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public uint Entity;
            public int ComponentId;

            public RemoveComponentCommand(uint entity, int componentId)
            {
                CommandType = CommandTypes.RemoveComponent;
                Size = sizeof(RemoveComponentCommand);
                Entity = entity;
                ComponentId = componentId;
            }

            public static bool Process(EntityManager entityManager, RemoveComponentCommand* it, uint lastEntity)
            {
                return entityManager.Remove(it->Entity > 0u ? it->Entity : lastEntity, it->ComponentId);
            }
        }

        private readonly NativeBuffer _buffer;

        public EntityCommandBuffer(IAllocator allocator, int sizeInBytes = 65536)
        {
            _buffer = new NativeBuffer(allocator, sizeInBytes);
        }

        public unsafe void CreateEntity(Span<ComponentType> componentTypes)
        {
            //old code did not store the components and there is overhead to this
            //but its the only way to make sure the em doesn't crash if it never 
            //seen the spec before (old system stored all specs in a static array)

            CreateEntityCommand* it = _buffer.Add(new CreateEntityCommand(componentTypes.Length));
            for (var i = 0; i < componentTypes.Length; i++)
                _buffer.Add(componentTypes[i]);

            it->Size += sizeof(ComponentType);
        }
        public void CreateEntity(EntitySpec spec)
        {
            Span<ComponentType> componentTypes = spec.ComponentTypes;
            CreateEntity(componentTypes);
        }

        public void RemoveEntity(uint entity)
        {
            _buffer.Add(new RemoveEntityCommand(entity));
        }

        public void ReplaceComponent<T>(in T t)
            where T : unmanaged
        {
            var type = ComponentType<T>.Type;
            ReplaceComponentCommand* it = _buffer.Add(new ReplaceComponentCommand(0u, type));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void ReplaceComponent<T>(uint entity, in T t)
            where T : unmanaged
        {
            var type = ComponentType<T>.Type;
            ReplaceComponentCommand* it = _buffer.Add(new ReplaceComponentCommand(entity, type));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void RemoveComponent<T>(uint entity)
            where T : unmanaged
        {
            var type = ComponentType<T>.Type;
            _buffer.Add(new RemoveComponentCommand(entity, type.ID));
        }

        public void AssignComponent<T>(in T t)
            where T : unmanaged
        {
            var type = ComponentType<T>.Type;
            AssignComponentCommand* it = _buffer.Add(new AssignComponentCommand(0u, type));
            _buffer.Add(t);
            it->Size += type.Size;
        }
        public void AssignComponent<T>(uint entity, in T t)
            where T : unmanaged
        {
            var type = ComponentType<T>.Type;
            AssignComponentCommand* it = _buffer.Add(new AssignComponentCommand(entity, type));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void UpdateComponent<T>(in T t)
           where T : unmanaged
        {
            var type = ComponentType<T>.Type;
            UpdateComponentCommand* it = _buffer.Add(new UpdateComponentCommand(0u, type));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void UpdateComponent<T>(uint entity, in T t)
           where T : unmanaged
        {
            var type = ComponentType<T>.Type;
            UpdateComponentCommand* it = _buffer.Add(new UpdateComponentCommand(entity, type));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void Execute(EntityManager em)
        {
            var rawPtr = _buffer.RawPointer;
            var lastEntity = 0u;
            while (rawPtr < _buffer.EndPointer)
            {
                var cmd = (EntityCommand*)rawPtr;
                switch (cmd->CommandType)
                {
                    case CommandTypes.CreateEntity:
                        lastEntity = CreateEntityCommand.Process(em, (CreateEntityCommand*)cmd);
                        break;
                    case CommandTypes.RemoveEntity:
                        RemoveEntityCommand.Process(em, (RemoveEntityCommand*)cmd);
                        lastEntity = 0u;
                        break;
                    case CommandTypes.AssignComponent:
                        AssignComponentCommand.Process(em, (AssignComponentCommand*)cmd, lastEntity);
                        break;
                    case CommandTypes.ReplaceComponent:
                        ReplaceComponentCommand.Process(em, (ReplaceComponentCommand*)cmd, lastEntity);
                        break;
                    case CommandTypes.RemoveComponent:
                        //removing the last component of an entity has the side effect of deleting it, could cause bugs
                        if (RemoveComponentCommand.Process(em, (RemoveComponentCommand*)cmd, lastEntity))
                            lastEntity = 0u;
                        break;
                    case CommandTypes.UpdateComponent:
                        UpdateComponentCommand.Process(em, (UpdateComponentCommand*)cmd, lastEntity);
                        break;
                }
                rawPtr += cmd->Size;
            }

            _buffer.Reset();
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }
    }
}