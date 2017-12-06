namespace Olive.Entities
{
    /// <summary>
    /// Enables adding a direct SQL WHERE criteria to the database query.
    /// </summary>
    public class DirectDatabaseCriterion : Criterion
    {
        /// <summary>
        /// Gets the parameters used in the specified custom SQL criteria.
        /// </summary>
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the SQL criteria.
        /// </summary>
        public string SqlCriteria { get; set; }

        /// <summary>
        /// N/A.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new object Value
        {
            get { return null; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Specifies whether this criteria is compatible with normal caching.
        /// </summary>
        public bool IsCacheSafe { get; set; }

        /// <summary>
        /// N/A.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new FilterFunction FilterFunction
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectDatabaseCriterion" /> class.
        /// </summary>
        public DirectDatabaseCriterion(string sqlCriteria)
            : base("N/A", "N/A") => SqlCriteria = sqlCriteria;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectDatabaseCriterion" /> class.
        /// </summary>
        /// <param name="parameters">Item1 = Parameter name (without the @ character). Item2 = parameter value.</param>
        public DirectDatabaseCriterion(string sqlCriteria, params Tuple<string, object>[] parameters)
            : this(sqlCriteria)
        {
            foreach (var p in parameters)
                Parameters.Add(p.Item1, p.Item2);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectDatabaseCriterion" /> class.
        /// </summary>
        public DirectDatabaseCriterion(string sqlCriteria, Dictionary<string, object> parameters)
            : this(sqlCriteria) => Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectDatabaseCriterion" /> class.
        /// </summary>
        /// <param name="parameters">Example: new {Parameter1 = SomeValue(), Parameter2 = AnotherValue()}</param>
        public DirectDatabaseCriterion(string sqlCriteria, object parameters)
            : this(sqlCriteria)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            Parameters = parameters.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(parameters));
        }

        /// <summary>
        /// Returns a string that represents this instance.
        /// </summary>
        public override string ToString() =>
            $"{SqlCriteria}|{Parameters.Select(x => "{0}={1}".FormatWith(x.Key, x.Value)).ToString("|")}";

        public string MapSqlCriteria(Dictionary<string, string> propertyMappings)
        {
            if (PropertyName.IsEmpty() || PropertyName == "N/A") return SqlCriteria;

            return SqlCriteria.Replace($"${{{{{PropertyName}}}}}", propertyMappings[PropertyName]);
        }
    }
}