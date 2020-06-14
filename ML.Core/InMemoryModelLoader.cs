using Microsoft.Extensions.ML;
using Microsoft.Extensions.Primitives;
using Microsoft.ML;
using System.Threading;

namespace ML.Core
{
    public class InMemoryModelLoader : ModelLoader
    {
        private readonly ITransformer _model;

        public InMemoryModelLoader(ITransformer model)
        {
            _model = model;
        }

        public override ITransformer GetModel() => _model;

        public override IChangeToken GetReloadToken() =>
            //TODO This IChangeToken will never notify a change.
            new CancellationChangeToken(CancellationToken.None);
    }
}
