using Dapper;
using System.Data;

namespace DockerRegistryUI.Controllers
{
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            if (value == null || value is DBNull)
            {
                return Guid.Empty;
            }

            if (value is string stringValue && Guid.TryParse(stringValue, out var guidValue))
            {
                return guidValue;
            }

            throw new DataException("Failed to parse Guid value.");
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }
    }
}
