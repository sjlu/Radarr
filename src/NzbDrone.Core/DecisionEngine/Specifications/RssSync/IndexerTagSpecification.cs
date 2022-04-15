using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class IndexerTagSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IIndexerFactory _indexerFactory;

        public IndexerTagSpecification(Logger logger, IIndexerFactory indexerFactory)
        {
            _logger = logger;
            _indexerFactory = indexerFactory;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            return new List<Decision> { Calculate(subject, searchCriteria) };
        }

        private Decision Calculate(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Release == null || subject.Movie?.Tags == null || subject.Release.IndexerId == 0)
            {
                return Decision.Accept();
            }

            IndexerDefinition indexer;
            try
            {
                indexer = _indexerFactory.Get(subject.Release.IndexerId);
            }
            catch (ModelNotFoundException)
            {
                _logger.Debug("Indexer with id {0} does not exist, skipping indexer tags check", subject.Release.IndexerId);
                return Decision.Accept();
            }

            // If indexer has tags, check that at least one of them is present on the series
            var indexerTags = indexer.Tags;

            if (indexerTags.Any() && indexerTags.Intersect(subject.Movie.Tags).Empty())
            {
                _logger.Debug("Indexer {0} has tags. None of these are present on movie {1}. Rejecting", subject.Release.Indexer, subject.Movie);

                return Decision.Reject("Movie tags do not match any of the indexer tags");
            }

            return Decision.Accept();
        }
    }
}
