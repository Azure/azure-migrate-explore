namespace Azure.Migrate.Export.Models
{
    public class EmissionsDetails
    {
        public string Source { get; set; }
        public double Scope1Compute { get; set; }
        public double Scope1Storage { get; set; }
        public double Scope2Compute { get; set; }
        public double Scope2Storage { get; set; }
        public double Scope3Compute { get; set; }
        public double Scope3Storage { get; set; }
    }
}
