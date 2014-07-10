namespace BudgetAnalyser.Engine.Budget.Data
{
    public class BudgetBucketDto
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public BucketTypeDto Type { get; set; }
    }
}