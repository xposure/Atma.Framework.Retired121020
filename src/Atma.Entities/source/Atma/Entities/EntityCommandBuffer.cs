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
            SetEntity,
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

        private struct SetEntityCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public int EntityCount;

            public SetEntityCommand(int entityCount)
            {
                CommandType = CommandTypes.SetEntity;
                Size = sizeof(EntityCommand);
                EntityCount = entityCount;
            }

            public static Span<uint> Process(EntityManager entityManager, SetEntityCommand* it)
            {
                return new Span<uint>(it + 1, it->EntityCount);
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

            public static void Process(EntityManager entityManager, CreateEntityCommand* it, Span<uint> lastEntities)
            {
                System.Span<ComponentType> componentTypes = new System.Span<ComponentType>(it + 1, it->ComponentCount);
                entityManager.Create(componentTypes, lastEntities);
            }
        }

        private struct RemoveEntityCommand
        {
            public CommandTypes CommandType;
            public int Size;

            public RemoveEntityCommand(int dummy)
            {
                CommandType = CommandTypes.RemoveEntity;
                Size = sizeof(RemoveEntityCommand);
            }

            public static void Process(EntityManager entityManager, RemoveEntityCommand* it, Span<uint> lastEntities)
            {
                //TODO: bulk remove entity
                Assert.GreatherThan(lastEntities.Length, 0);
                entityManager.Remove(lastEntities);
            }
        }

        private struct ReplaceComponentCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public ComponentType ComponentType;
            public int DataCount;

            public ReplaceComponentCommand(in ComponentType componentType, int dataCount)
            {
                CommandType = CommandTypes.ReplaceComponent;
                Size = sizeof(ReplaceComponentCommand);
                ComponentType = componentType;
                DataCount = dataCount;
            }

            public static void Process(EntityManager entityManager, ReplaceComponentCommand* it, Span<uint> lastEntities)
            {
                // Assert.GreatherThan(lastEntities.Length, 0);
                // if (it->DataCount == 1)
                // {
                //     entityManager.SetComponentInternal(&it->ComponentType, lastEntities, it + 1, true, false);
                // }
                // else
                // {
                //     Assert.EqualTo(lastEntities.Length, it->DataCount);
                //     entityManager.SetComponentInternal(&it->ComponentType, lastEntities, it + 1, false, true);
                // }
            }
        }

        private struct UpdateComponentCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public ComponentType ComponentType;
            public int DataCount;

            public UpdateComponentCommand(in ComponentType componentType, int dataCount)
            {
                CommandType = CommandTypes.UpdateComponent;
                Size = sizeof(UpdateComponentCommand);
                ComponentType = componentType;
                DataCount = dataCount;
            }


            public static void Process(EntityManager entityManager, UpdateComponentCommand* it, Span<uint> lastEntities)
            {
                // Assert.GreatherThan(lastEntities.Length, 0);
                // if (it->DataCount == 1)
                // {
                //     entityManager.UpdateInternal(&it->ComponentType, lastEntities, it + 1, true);
                // }
                // else
                // {
                //     Assert.EqualTo(lastEntities.Length, it->DataCount);
                //     entityManager.UpdateInternal(&it->ComponentType, lastEntities, it + 1, false);
                // }
            }
        }

        private struct AssignComponentCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public ComponentType ComponentType;
            public int DataCount;

            public AssignComponentCommand(in ComponentType componentType, int dataCount)
            {
                CommandType = CommandTypes.AssignComponent;
                Size = sizeof(AssignComponentCommand);
                ComponentType = componentType;
                DataCount = dataCount;
            }

            public static void Process(EntityManager entityManager, AssignComponentCommand* it, Span<uint> lastEntities)
            {
                Assert.GreatherThan(lastEntities.Length, 0);
                var dataPtr = (void*)(it + 1);
                if (it->DataCount == 1)
                {
                    //assigning one piece of data to all entities
                    for (var i = 0; i < lastEntities.Length; i++)
                        entityManager.AssignInternal(lastEntities, &it->ComponentType, ref dataPtr, true);
                }
                else
                {
                    Assert.EqualTo(lastEntities.Length, it->DataCount);
                    entityManager.AssignInternal(lastEntities, &it->ComponentType, ref dataPtr, false);
                }
            }
        }

        //TODO: the old system would stack commands on top and run them once for performance
        private struct RemoveComponentCommand
        {
            public CommandTypes CommandType;
            public int Size;
            public int ComponentId;

            public RemoveComponentCommand(int componentId)
            {
                CommandType = CommandTypes.RemoveComponent;
                Size = sizeof(RemoveComponentCommand);
                ComponentId = componentId;
            }

            public static void Process(EntityManager entityManager, RemoveComponentCommand* it, Span<uint> lastEntities)
            {
                //TODO: bulk remove
                Assert.GreatherThan(lastEntities.Length, 0);
                for (var i = 0; i < lastEntities.Length; i++)
                    entityManager.RemoveInternal(lastEntities[i], it->ComponentId);
            }
        }

        private readonly NativeBuffer _buffer;

        public EntityCommandBuffer(IAllocator allocator, int sizeInBytes = 65536)
        {
            _buffer = new NativeBuffer(allocator, sizeInBytes);
        }

        public unsafe void CreateEntity(System.Span<ComponentType> componentTypes, int count = 1)
        {
            //old code did not store the components and there is overhead to this
            //but its the only way to make sure the em doesn't crash if it never 
            //seen the spec before (old system stored all specs in a static array)

            ReserveSetEntity(count);

            //if this buffer array resizes after the pointer is taken, its going to be invalid...
            var reserveSize = sizeof(CreateEntityCommand) + sizeof(ComponentType) * componentTypes.Length;
            _buffer.EnsureCapacity(reserveSize);

            CreateEntityCommand* it = _buffer.Add(new CreateEntityCommand(componentTypes.Length));

            for (var i = 0; i < componentTypes.Length; i++)
                _buffer.Add(componentTypes[i]);
            it->Size += sizeof(ComponentType);
        }


        public void CreateEntity(EntitySpec spec, int count = 1)
        {
            System.Span<ComponentType> componentTypes = spec.ComponentTypes;
            CreateEntity(componentTypes, count);
        }

        private Span<uint> ReserveSetEntity(int count)
        {
            _buffer.EnsureCapacity(sizeof(SetEntityCommand) + sizeof(uint) * count);
            var ptr = _buffer.Add(new SetEntityCommand(count));
            return new Span<uint>(ptr + 1, count);
        }

        public void SetEntity(uint entity)
        {
            var data = ReserveSetEntity(1);
            data[0] = entity;
        }

        public void SetEntity(Span<uint> entities)
        {
            var data = ReserveSetEntity(entities.Length);
            for (var i = 0; i < entities.Length; i++)
                data[i] = entities[i];
        }

        public void RemoveEntity(uint entity)
        {
            _buffer.EnsureCapacity(sizeof(RemoveEntityCommand) + sizeof(uint));
            SetEntity(entity);

            _buffer.Add(new RemoveEntityCommand(0));
        }

        public void ReplaceComponent<T>(in T t)
            where T : unmanaged
        {
            var reserveSize = sizeof(ReplaceComponentCommand) + SizeOf<T>.Size;
            _buffer.EnsureCapacity(reserveSize);

            var type = ComponentType<T>.Type;
            ReplaceComponentCommand* it = _buffer.Add(new ReplaceComponentCommand(type, 1));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void ReplaceComponent<T>(uint entity, in T t)
            where T : unmanaged
        {
            SetEntity(entity);

            var reserveSize = sizeof(ReplaceComponentCommand) + SizeOf<T>.Size;
            _buffer.EnsureCapacity(reserveSize);

            var type = ComponentType<T>.Type;
            ReplaceComponentCommand* it = _buffer.Add(new ReplaceComponentCommand(type, 1));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void RemoveComponent<T>(uint entity)
            where T : unmanaged
        {
            SetEntity(entity);

            var type = ComponentType<T>.Type;
            _buffer.Add(new RemoveComponentCommand(type.ID));
        }

        public void AssignComponent<T>(in T t)
            where T : unmanaged
        {
            var reserveSize = sizeof(AssignComponentCommand) + SizeOf<T>.Size;
            _buffer.EnsureCapacity(reserveSize);

            var type = ComponentType<T>.Type;
            AssignComponentCommand* it = _buffer.Add(new AssignComponentCommand(type, 1));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void AssignComponent<T>(uint entity, in T t)
            where T : unmanaged
        {
            SetEntity(entity);

            var reserveSize = sizeof(AssignComponentCommand) + SizeOf<T>.Size;
            _buffer.EnsureCapacity(reserveSize);

            var type = ComponentType<T>.Type;
            AssignComponentCommand* it = _buffer.Add(new AssignComponentCommand(type, 1));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void UpdateComponent<T>(in T t)
           where T : unmanaged
        {
            var reserveSize = sizeof(UpdateComponentCommand) + SizeOf<T>.Size;
            _buffer.EnsureCapacity(reserveSize);

            var type = ComponentType<T>.Type;
            UpdateComponentCommand* it = _buffer.Add(new UpdateComponentCommand(type, 1));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void UpdateComponent<T>(uint entity, in T t)
           where T : unmanaged
        {
            SetEntity(entity);

            var reserveSize = sizeof(UpdateComponentCommand) + SizeOf<T>.Size;
            _buffer.EnsureCapacity(reserveSize);

            var type = ComponentType<T>.Type;
            UpdateComponentCommand* it = _buffer.Add(new UpdateComponentCommand(type, 1));
            _buffer.Add(t);
            it->Size += type.Size;
        }

        public void Execute(EntityManager em)
        {
            var rawPtr = _buffer.RawPointer;
            Span<uint> lastEntities = Span<uint>.Empty;
            while (rawPtr < _buffer.EndPointer)
            {
                var cmd = (EntityCommand*)rawPtr;
                switch (cmd->CommandType)
                {
                    case CommandTypes.SetEntity:
                        lastEntities = SetEntityCommand.Process(em, (SetEntityCommand*)cmd);
                        break;
                    case CommandTypes.CreateEntity:
                        CreateEntityCommand.Process(em, (CreateEntityCommand*)cmd, lastEntities);
                        break;
                    case CommandTypes.RemoveEntity:
                        RemoveEntityCommand.Process(em, (RemoveEntityCommand*)cmd, lastEntities);
                        lastEntities = Span<uint>.Empty;
                        break;
                    case CommandTypes.AssignComponent:
                        AssignComponentCommand.Process(em, (AssignComponentCommand*)cmd, lastEntities);
                        break;
                    case CommandTypes.ReplaceComponent:
                        ReplaceComponentCommand.Process(em, (ReplaceComponentCommand*)cmd, lastEntities);
                        break;
                    case CommandTypes.RemoveComponent:
                        //removing the last component of an entity has the side effect of deleting it, could cause bugs
                        RemoveComponentCommand.Process(em, (RemoveComponentCommand*)cmd, lastEntities);
                        break;
                    case CommandTypes.UpdateComponent:
                        UpdateComponentCommand.Process(em, (UpdateComponentCommand*)cmd, lastEntities);
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