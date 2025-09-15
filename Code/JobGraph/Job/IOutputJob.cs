using System;
using System.Collections.Generic;

namespace MANIFOLD.Jobs {
    public interface IOutputSocket {
        public IInputJob Job { get; }
        public Type DataType { get; }
        public int Index { get; }
    }

    public record Output<TData>(IInputJob<TData> Job, int Index) : IOutputSocket {
        IInputJob IOutputSocket.Job => Job;
        public Type DataType => typeof(TData);
    }

    public interface IOutputJob : IJob {
        public IReadOnlyList<IOutputSocket> Outputs { get; }
        public object OutputData { get; }

        public void AddOutput(IInputJob job, int targetIndex);
        public void RemoveOutput(IInputJob job, int targetIndex);
        public void DisconnectOutputs();
    }
    
    /// <summary>
    /// A job that outputs to input jobs.
    /// </summary>
    /// <typeparam name="TData">Data to output.</typeparam>
    public interface IOutputJob<TData> : IOutputJob {
        public new IReadOnlyList<Output<TData>> Outputs { get; }
        public new TData OutputData { get; }
        
        public void AddOutput(IInputJob<TData> job, int targetIndex);
        public void RemoveOutput(IInputJob<TData> job, int targetIndex);
    }
}
