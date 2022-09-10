using NzbDrone.Core.Configuration;
using Radarr.Http;

namespace Radarr.Api.V4.Config
{
    [V4ApiController("config/importlist")]

    public class ImportListConfigController : ConfigController<ImportListConfigResource>
    {
        public ImportListConfigController(IConfigService configService)
            : base(configService)
        {
        }

        protected override ImportListConfigResource ToResource(IConfigService model)
        {
            return ImportListConfigResourceMapper.ToResource(model);
        }
    }
}
