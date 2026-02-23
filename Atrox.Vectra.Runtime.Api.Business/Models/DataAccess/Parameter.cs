namespace Atrox.Vectra.Runtime.Api.Business.Models.DataAccess
{
    public class Parameter
    {
        public string ParameterName { get; set; }
        public List<string> DataTypes { get; set; }
        public int Length { get; set; }
        public bool IsOutput { get; set; }
        public string DefaultValue { get; set; }
    }
}


