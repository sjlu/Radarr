using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class UpgradeSpecification : IImportDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeSpecification(IConfigService configService,
                                    ICustomFormatCalculationService formatService,
                                    Logger logger)
        {
            _configService = configService;
            _formatService = formatService;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;
            var qualityProfile = localMovie.Movie.QualityProfile;
            var qualityComparer = new QualityModelComparer(qualityProfile);

            if (localMovie.Movie.MovieFileId > 0)
            {
                var movieFile = localMovie.Movie.MovieFile;

                if (movieFile == null)
                {
                    _logger.Trace("Unable to get movie file details from the DB. MovieId: {0} MovieFileId: {1}", localMovie.Movie.Id, localMovie.Movie.MovieFileId);

                    return Decision.Accept();
                }

                var qualityCompare = qualityComparer.Compare(localMovie.Quality.Quality, movieFile.Quality.Quality);

                if (qualityCompare < 0)
                {
                    _logger.Debug("This file isn't a quality upgrade for movie. New Quality is {0}. Skipping {1}", localMovie.Quality.Quality, localMovie.Path);

                    return Decision.Reject("Not an upgrade for existing movie file. New Quality is {0}", localMovie.Quality.Quality);
                }

                // Same quality, propers/repacks are preferred and it is not a revision update. Reject revision downgrade.

                if (qualityCompare == 0 &&
                    downloadPropersAndRepacks != ProperDownloadTypes.DoNotPrefer &&
                    localMovie.Quality.Revision.CompareTo(movieFile.Quality.Revision) < 0)
                {
                    _logger.Debug("This file isn't a quality revision upgrade for movie. Skipping {0}", localMovie.Path);
                    return Decision.Reject("Not a quality revision upgrade for existing movie file(s)");
                }

                movieFile.Movie = localMovie.Movie;
                var currentFormats = _formatService.ParseCustomFormat(movieFile);
                var currentScore = qualityProfile.CalculateCustomFormatScore(currentFormats);

                if (qualityCompare == 0 && localMovie.CustomFormatScore < currentScore)
                {
                    _logger.Debug("New file's custom formats [{0}] do not improve on [{1}], skipping",
                        localMovie.CustomFormats.ConcatToString(),
                        currentFormats.ConcatToString());

                    return Decision.Reject("Not a Custom Format upgrade for existing movie file(s)");
                }
            }

            return Decision.Accept();
        }
    }
}
