using Dapper;
using System.Data;

namespace DockerRegistryUI.Controllers
{
    public class UriTypeHandler : SqlMapper.TypeHandler<Uri>
    {
        public override void SetValue(IDbDataParameter parameter, Uri? value)
        {
            parameter.Value = value?.AbsoluteUri;
        }

        public override Uri? Parse(object value)
        {
            if (value is null || value is DBNull)
                return null;

            var valueAsString = value.ToString();
            if (valueAsString is null)
                return null;

            return new Uri(valueAsString);
        }
    }
}
