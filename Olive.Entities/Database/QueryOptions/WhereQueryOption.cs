namespace Olive.Entities
{
    public class WhereQueryOption : QueryOption
    {
        public string SqlCriteria { get; set; }

        public WhereQueryOption() { }

        public WhereQueryOption(string sqlCriteria) => SqlCriteria = sqlCriteria;
    }
}
