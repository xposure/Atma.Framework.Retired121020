namespace Atma.Entities
{
    using System;
    using Atma.Memory;
    using BenchmarkDotNet.Attributes;

    public readonly unsafe ref struct ReadSpanOfOne<T>
        where T : unmanaged
    {
        private readonly ReadOnlySpan<T> _value;

        public ReadSpanOfOne(T* value)
        {
            _value = new ReadOnlySpan<T>(value, 1);
        }

        public ref readonly T Value => ref _value[0];
    }

    public readonly unsafe ref struct WriteSpanOfOne<T>
        where T : unmanaged
    {
        private readonly Span<T> _value;

        public WriteSpanOfOne(T* value)
        {
            _value = new Span<T>(value, 1);
        }

        public ref T Value => ref _value[0];

    }

    public unsafe ref struct ReadSpanWithIndex<T>
       where T : unmanaged
    {
        internal int Index;
        private readonly ReadOnlySpan<T> _value;

        public ReadSpanWithIndex(Span<T> value)
        {
            Index = 0;
            _value = value;
        }

        public ref readonly T Value => ref _value[Index];
    }

    public unsafe ref struct WriteSpanWithIndex<T>
        where T : unmanaged
    {
        internal int Index;
        private readonly Span<T> _value;

        public WriteSpanWithIndex(Span<T> value)
        {
            Index = 0;
            _value = value;
        }

        public ref T Value => ref _value[Index];

    }

    public readonly unsafe ref struct ReadSpanWithPtrIndex<T>
     where T : unmanaged
    {
        private readonly int* Index;
        private readonly ReadOnlySpan<T> _value;

        public ReadSpanWithPtrIndex(int* index, Span<T> value)
        {
            Index = index;
            _value = value;
        }

        public ref readonly T Value => ref _value[*Index];
    }

    public unsafe ref struct WriteSpanWithPtrIndex<T>
        where T : unmanaged
    {
        private readonly int* Index;
        private readonly Span<T> _value;

        public WriteSpanWithPtrIndex(int* index, Span<T> value)
        {
            Index = index;
            _value = value;
        }

        public ref T Value => ref _value[*Index];

    }

    public ref struct SystemIterateWithCopy
    {
        public float dt;
        public Position position;
        //ReadOnly
        public Velocity velocity;

        public void Execute()
        {
            position.X += velocity.X * dt;
            position.Y += velocity.Y * dt;
        }
    }

    public unsafe ref struct SystemIterateWithPtr
    {
        public float dt;
        public Position* position;

        //[ReadOnly]
        public Velocity* velocity;

        public void Execute()
        {
            position->X += velocity->X * dt;
            position->Y += velocity->Y * dt;
        }
    }

    public unsafe ref struct SystemRawPtrAndLength
    {
        public float dt;
        public Position* position;

        //[ReadOnly]
        public Velocity* velocity;

        public void Execute(int length)
        {
            for (var i = 0; i < length; i++)
            {
                ref var p = ref position[i];
                ref readonly var v = ref velocity[i];
                p.X += v.X * dt;
                p.Y += v.Y * dt;
            }
        }
    }



    public unsafe ref struct SystemIterateWithSpan
    {
        public float dt;
        public Span<Position> position;
        public ReadOnlySpan<Velocity> velocity;

        public void Execute()
        {
            ref var p = ref position[0];
            ref readonly var v = ref velocity[0];
            p.X += v.X * dt;
            p.Y += v.Y * dt;
        }
    }

    public unsafe ref struct SystemIterateWithSpanOfOne
    {
        public float dt;
        public WriteSpanOfOne<Position> position;
        public ReadSpanOfOne<Velocity> velocity;

        public void Execute()
        {
            ref var p = ref position.Value;
            ref readonly var v = ref velocity.Value;
            p.X += v.X * dt;
            p.Y += v.Y * dt;
        }
    }

    public unsafe ref struct SystemIterateWithRWSpanAndIndex
    {
        public float dt;
        public WriteSpanWithIndex<Position> position;
        public ReadSpanWithIndex<Velocity> velocity;

        public void Execute()
        {
            ref var p = ref position.Value;
            ref readonly var v = ref velocity.Value;
            p.X += v.X * dt;
            p.Y += v.Y * dt;
        }
    }

    public unsafe ref struct SystemIterateWithRWSpanAndIndexPtr
    {
        public float dt;
        public WriteSpanWithPtrIndex<Position> position;
        public ReadSpanWithPtrIndex<Velocity> velocity;

        public void Execute()
        {
            ref var p = ref position.Value;
            ref readonly var v = ref velocity.Value;
            p.X += v.X * dt;
            p.Y += v.Y * dt;
        }
    }

    public unsafe ref struct SystemIterateWithSpanAndIndex
    {
        public float dt;
        public Span<Position> position;
        public ReadOnlySpan<Velocity> velocity;

        public void Execute(int index)
        {
            ref var p = ref position[index];
            ref readonly var v = ref velocity[index];
            p.X += v.X * dt;
            p.Y += v.Y * dt;
        }
    }

    public unsafe ref struct SystemSpanWithLength
    {
        public float dt;
        public Span<Position> position;
        public ReadOnlySpan<Velocity> velocity;

        public void Execute(int length)
        {
            for (var i = 0; i < length; i++)
            {
                ref var p = ref position[i];
                ref readonly var v = ref velocity[i];
                p.X += v.X * dt;
                p.Y += v.Y * dt;
            }
        }
    }

    public unsafe ref struct SystemSpanWithLengthNoRef
    {
        public float dt;
        public Span<Position> position;
        public ReadOnlySpan<Velocity> velocity;

        public void Execute(int length)
        {
            for (var i = 0; i < length; i++)
            {
                position[i].X += velocity[i].X * dt;
                position[i].Y += velocity[i].Y * dt;
            }
        }
    }


    public unsafe struct SystemNativeArrayWithLength
    {
        public float dt;
        public NativeArray<Position> position;
        public NativeArray<Velocity> velocity;

        public void Execute(int length)
        {
            for (var i = 0; i < length; i++)
            {
                ref var p = ref position[i];
                ref readonly var v = ref velocity[i];
                p.X += v.X * dt;
                p.Y += v.Y * dt;
            }
        }
    }

    public unsafe struct SystemNativeSliceWithLength
    {
        public float dt;
        public NativeSlice<Position> position;
        public NativeSlice<Velocity> velocity;

        public void Execute(int length)
        {
            for (var i = 0; i < length; i++)
            {
                ref var p = ref position[i];
                ref readonly var v = ref velocity[i];
                p.X += v.X * dt;
                p.Y += v.Y * dt;
            }
        }
    }

    //[MediumRunJob]
    public unsafe class ReadWriteStructs : Benchmark
    {
        public float dt = 0.016f;

        [Params(100000)]
        public int N;

        private IAllocator _memory;

        private NativeArray<Position> _positions;
        private NativeArray<Velocity> _velocities;

        protected override void OnSetup()
        {
            _memory = new HeapAllocator(_logFactory);
            _positions = new NativeArray<Position>(_memory, N);
            _velocities = new NativeArray<Velocity>(_memory, N);
        }

        protected override void OnIterationSetup()
        {
            for (var i = 0; i < N; i++)
            {
                _positions[i] = new Position(i, i);
                _positions[i] = new Position(N - i, N - i);
            }
        }

        protected override void OnCleanup()
        {
            _positions.Dispose();
            _velocities.Dispose();
            _memory.Dispose();
        }

        protected override void OnIterationCleanup()
        {
        }

        [Benchmark]
        public void IterateWithCopy()
        {
            var processor = new SystemIterateWithCopy();
            processor.dt = dt;
            for (var i = 0; i < N; i++)
            {
                processor.position = _positions[i];
                processor.velocity = _velocities[i];

                processor.Execute();

                _positions[i] = processor.position;
            }
        }


        [Benchmark]
        public void IterateWithPtrs()
        {
            var processor = new SystemIterateWithPtr();
            processor.dt = dt;
            processor.position = _positions.RawPointer;
            processor.velocity = _velocities.RawPointer;
            for (var i = 0; i < N; i++)
            {
                processor.Execute();
                processor.position++;
                processor.velocity++;
            }
        }


        [Benchmark(Baseline = true)]
        public void PtrsAndLength()
        {
            var processor = new SystemRawPtrAndLength();
            processor.dt = dt;
            processor.position = _positions.RawPointer;
            processor.velocity = _velocities.RawPointer;
            processor.Execute(N);
        }



        [Benchmark]
        public void IterateWithSpan()
        {
            var processor = new SystemIterateWithSpan();
            processor.dt = dt;
            for (var i = 0; i < N; i++)
            {
                processor.position = new Span<Position>(_positions.RawPointer + i, 1);
                processor.velocity = new Span<Velocity>(_velocities.RawPointer + i, 1);

                processor.Execute();
            }
        }


        //Way too slow to even keep 
        // [Benchmark]
        // public void IterateWithSpanOfOne()
        // {
        //     var processor = new SystemIterateWithSpanOfOne();
        //     processor.dt = dt;
        //     for (var i = 0; i < N; i++)
        //     {
        //         processor.position = new WriteSpanOfOne<Position>(_positions.RawPointer + i);
        //         processor.velocity = new ReadSpanOfOne<Velocity>(_velocities.RawPointer + i);

        //         processor.Execute();
        //     }
        // }

        [Benchmark]
        public void IterateWithRWSpanAndIndex()
        {
            var processor = new SystemIterateWithRWSpanAndIndex();
            processor.dt = dt;
            processor.position = new WriteSpanWithIndex<Position>(_positions);
            processor.velocity = new ReadSpanWithIndex<Velocity>(_velocities);
            for (var i = 0; i < N; i++)
            {
                processor.Execute();
                processor.position.Index++;
                processor.velocity.Index++;
            }
        }

        [Benchmark]
        public void IterateWithRWSpanAndIndexPtr()
        {
            var processor = new SystemIterateWithRWSpanAndIndexPtr();
            processor.dt = dt;
            var index = stackalloc[] { 0 };
            processor.position = new WriteSpanWithPtrIndex<Position>(index, _positions);
            processor.velocity = new ReadSpanWithPtrIndex<Velocity>(index, _velocities);
            for (var i = 0; i < N; i++)
            {
                processor.Execute();
                (*index)++;
            }
        }

        [Benchmark]
        public void IterateWithSpanAndIndex()
        {
            var processor = new SystemIterateWithSpanAndIndex();
            processor.dt = dt;
            processor.position = _positions;
            processor.velocity = (Span<Velocity>)_velocities;
            for (var i = 0; i < N; i++)
                processor.Execute(i);
        }

        [Benchmark]
        public void SpanWithLength()
        {
            var processor = new SystemSpanWithLength();
            processor.dt = dt;
            processor.position = _positions;
            processor.velocity = (Span<Velocity>)_velocities;
            processor.Execute(N);
        }

        [Benchmark]
        public void SpanWithLengthNoRef()
        {
            var processor = new SystemSpanWithLengthNoRef();
            processor.dt = dt;
            processor.position = _positions;
            processor.velocity = (Span<Velocity>)_velocities;
            processor.Execute(N);
        }


        [Benchmark]
        public void NativeArrayWithLength()
        {
            var processor = new SystemNativeArrayWithLength();
            processor.dt = dt;
            processor.position = _positions;
            processor.velocity = _velocities;
            processor.Execute(N);
        }

        [Benchmark]
        public void NativeSliceWithLength()
        {
            var processor = new SystemNativeSliceWithLength();
            processor.dt = dt;
            processor.position = _positions.TestSlice();
            processor.velocity = _velocities.TestSlice();
            processor.Execute(N);
        }

    }

}