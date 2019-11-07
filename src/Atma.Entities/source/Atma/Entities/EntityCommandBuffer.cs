namespace Atma.Entities
{
    using Atma.Memory;

    using static Atma.Debug;

    public unsafe struct EntityCommandBuffer
    {
        private enum CommandTypes
        {
            CreateEntity,
            DestroyEntity,
            SetComponent,
            RemoveCompontent,
        }

        private struct EntityCommand
        {
            public CommandTypes CommandType;
            public int PayloadSize;
            //public int OperationIndex;
            public void SetPayload<T>()
                where T : unmanaged
            {
                PayloadSize = SizeOf<T>.Size - SizeOf<EntityCommand>.Size;
            }
        }

        private struct CreateEntityCommand
        {
            public EntityCommand Header;
            public int ArchetypeIndex;
            public int Count;

            public static CreateEntityCommand Create(int archetype)
            {
                var command = new CreateEntityCommand();
                command.Header.CommandType = CommandTypes.CreateEntity;
                command.Header.SetPayload<CreateEntityCommand>();
                command.ArchetypeIndex = archetype;
                command.Count = 0;
                return command;
            }
        }

        private struct DeleteEntityCommand
        {
            public EntityCommand Header;
            public int Count;

            public static DeleteEntityCommand Create()
            {
                var command = new DeleteEntityCommand();
                command.Header.CommandType = CommandTypes.DestroyEntity;
                command.Header.SetPayload<DeleteEntityCommand>();
                command.Count = 0;
                return command;
            }
        }

        private struct SetComponentCommand
        {
            public EntityCommand Header;
            public int Entity;
            public int ComponentType;

            public static SetComponentCommand Create(int entity, int componentType)
            {
                var command = new SetComponentCommand();
                command.Header.CommandType = CommandTypes.SetComponent;
                command.Header.SetPayload<SetComponentCommand>();
                command.Entity = entity;
                command.ComponentType = componentType;
                return command;
            }
        }

        private struct RemoveComponentCommand
        {
            public EntityCommand Header;
            public int Entity;
            public int ComponentType;

            public static RemoveComponentCommand Create(int entity, int componentType)
            {
                var command = new RemoveComponentCommand();
                command.Header.CommandType = CommandTypes.RemoveCompontent;
                command.Header.SetPayload<RemoveComponentCommand>();
                command.Entity = entity;
                command.ComponentType = componentType;
                return command;
            }
        }

        private NativeBuffer _buffer;
        private EntityCommand* _lastCommand;

        public EntityCommandBuffer(Allocator allocator, int sizeInBytes)
        {
            _buffer = new NativeBuffer(allocator, sizeInBytes);
            _lastCommand = null;
        }

        public void CreateEntity(EntityArchetype archetype, int count = 1)
        {
            CreateEntityCommand* cmd = _buffer.Add(CreateEntityCommand.Create(archetype.Index));

            cmd->Count = count;

            _lastCommand = (EntityCommand*)cmd;
        }

        public void DeleteEntity(int entity)
        {
            DeleteEntityCommand* cmd = null;
            if (_lastCommand != null && _lastCommand->CommandType == CommandTypes.DestroyEntity)
                cmd = (DeleteEntityCommand*)_lastCommand;

            if (cmd == null)
                cmd = _buffer.Add(DeleteEntityCommand.Create());

            _buffer.Add(entity);
            cmd->Count++;
            cmd->Header.PayloadSize += SizeOf<int>.Size;
            _lastCommand = (EntityCommand*)cmd;
        }

        public void SetComponent<T>(in T t)
         where T : unmanaged
        {
            SetComponentCommand* cmd = _buffer.Add(SetComponentCommand.Create(0, typeof(T).GetHashCode()));
            _buffer.Add(t);
            cmd->Header.PayloadSize += SizeOf<T>.Size;
            _lastCommand = null;
        }

        public void SetComponent<T>(int entity, in T t)
            where T : unmanaged
        {
            SetComponentCommand* cmd = _buffer.Add(SetComponentCommand.Create(entity, typeof(T).GetHashCode()));
            _buffer.Add(t);
            cmd->Header.PayloadSize += SizeOf<T>.Size;
            _lastCommand = null;
        }

        public void RemoveComponent<T>()
            where T : unmanaged
        {
            RemoveComponentCommand* cmd = _buffer.Add(RemoveComponentCommand.Create(0, typeof(T).GetHashCode()));
            _lastCommand = null;
        }

        public void RemoveComponent<T>(int entity)
            where T : unmanaged
        {
            RemoveComponentCommand* cmd = _buffer.Add(RemoveComponentCommand.Create(entity, typeof(T).GetHashCode()));
            _lastCommand = null;
        }


        public void Playback(EntityManager entityManager)
        {
            _lastCommand = null;

            NativeArray<int> lastEntities = new NativeArray<int>();
            var rawPtr = _buffer.RawPointer;
            while (rawPtr < _buffer.EndPointer)
            {
                var cmd = (EntityCommand*)rawPtr;
                //for now we want to clear the last entities created
                //unless it matches these types
                switch (cmd->CommandType)
                {
                    case CommandTypes.SetComponent:
                    case CommandTypes.RemoveCompontent:
                        break;
                    default:
                        if (lastEntities.IsCreated)
                            lastEntities.Dispose();
                        break;
                }

                switch (cmd->CommandType)
                {
                    case CommandTypes.CreateEntity:
                        {
                            var it = (CreateEntityCommand*)cmd;
                            var archetype = entityManager.Archetypes[it->ArchetypeIndex];
                            entityManager.CreateEntity(archetype, it->Count, out lastEntities);
                        }
                        break;
                    case CommandTypes.DestroyEntity:
                        {
                            var it = (DeleteEntityCommand*)cmd;
                            var payload = (int*)(it + 1); ;
                            var entities = new NativeSlice<int>(payload, it->Count);
                            entityManager.DestroyEntity(entities);
                        }
                        break;
                    case CommandTypes.SetComponent:
                        {
                            var it = (SetComponentCommand*)cmd;
                            var payload = (void*)(it + 1); ;

                            if (it->Entity == 0)
                            {
                                Assert(lastEntities.IsCreated);
                                for (var i = 0; i < lastEntities.Length; i++)
                                    entityManager.SetComponentData(lastEntities[i], it->ComponentType, payload);
                            }
                            else
                            {
                                entityManager.SetComponentData(it->Entity, it->ComponentType, payload);
                            }
                        }

                        break;
                    case CommandTypes.RemoveCompontent:
                        {
                            var it = (RemoveComponentCommand*)cmd;

                            if (it->Entity == 0)
                            {
                                Assert(lastEntities.IsCreated);
                                for (var i = 0; i < lastEntities.Length; i++)
                                    entityManager.RemoveComponentData(lastEntities[i], it->ComponentType);
                            }
                            else
                            {
                                entityManager.RemoveComponentData(it->Entity, it->ComponentType);
                            }
                        }

                        break;
                    default:
                        Assert(false);
                        break;
                }

                rawPtr += cmd->PayloadSize + SizeOf<EntityCommand>.Size;
            }

            if (lastEntities.IsCreated)
                lastEntities.Dispose();

            _buffer.Reset();
        }
    }
}
