namespace Genetics.GeneSummarization
{
    public abstract class AbstractSummary
    {
        public int invalidClassifications = 0;
        public GeneticDriver sourceDriver;

        protected AbstractSummary(GeneticDriver sourceDriver)
        {
            this.sourceDriver = sourceDriver;
        }

        public abstract void ClassifyValue(float discreteValue);
    }
}
