using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chess.Perft.Interfaces;
using Perft.Parsers;

namespace Perft
{
    public sealed class PerftV2
    {
        public PerftV2(IEpdParser parser)
        {
            Parser = parser;
        }

        public IEpdParser Parser { get; set; }

        public Task<int> Run() => Run(CancellationToken.None);

        public Task<int> Run(CancellationToken cancellationToken) => InternalRun(cancellationToken);

        private async Task<int> InternalRun(CancellationToken cancellationToken)
        {
            return 0;
        }

        private static IEnumerable<IPerft> GeneratePerft(IEnumerable<IEpdSet> sets)
        {
            if (!sets.Any())
                return Enumerable.Empty<IPerft>();

            return Enumerable.Empty<IPerft>();
        }


    }
}