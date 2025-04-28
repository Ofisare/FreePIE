
using System.Threading;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;


using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins
{
    internal class MmfTelemetryConfig
    {
        public string Name { get; set; } = "MmfTelemetry";        
    }

    internal class MmfTelemetry<TData> : TelemetryBase<TData, MmfTelemetryConfig>
        where TData : struct
    {

        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        private int _dataSize = Marshal.SizeOf<TData>();

        //static Mutex mutex;


        public MmfTelemetry(MmfTelemetryConfig config) : base(config)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        protected override void Configure(MmfTelemetryConfig config)
        {

            _mmf = MemoryMappedFile.CreateOrOpen(config.Name, Marshal.SizeOf<TData>());

            _accessor = _mmf.CreateViewAccessor();

        }


        public override TData Receive()
        {

            TData data = default(TData);
            _accessor.Read(0, out data);
            return data;

        }



        public override int Send(TData data)
        {
            _accessor.Write(0, ref data);
            return _dataSize;
        }

        public override void Dispose()
        {
            _mmf?.Dispose();
            _accessor?.Dispose();
        }


        public override Task<TData> ReceiveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Receive());
        }

    }
}
