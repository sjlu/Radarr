using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using Radarr.Api.V4.CustomFormats;
using Radarr.Api.V4.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V4.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public MovieResource Movie { get; set; }
        public List<Language> Languages { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
    }
}
