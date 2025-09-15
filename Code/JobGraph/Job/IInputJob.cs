using System;
using System.Collections.Generic;

namespace MANIFOLD.Jobs {
    public interface IInputSocket {
        public IOutputJob Job { get; }
        public Type DataType { get; }
    }

    public record Input<TData>(IOutputJob<TData> Job) : IInputSocket {
        IOutputJob IInputSocket.Job => Job;
        public Type DataType => typeof(TData);
    }
    
    public interface IInputJob : IJob {
        public IReadOnlyList<IInputSocket> Inputs { get; }

        public void SetInput(int index, IOutputJob job);
        public void DisconnectInputs();
    }
    
    /// <summary>
    /// A job that takes data as input.
    /// </summary>
    /// <typeparam name="TData">Data to take in.</typeparam>
    public interface IInputJob<TData> : IInputJob {
        public new IReadOnlyList<Input<TData>> Inputs { get; }
        
        public new void SetInput(int index, IOutputJob<TData> job);
    }
}
