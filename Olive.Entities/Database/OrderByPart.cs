namespace Olive.Entities
{
    public class OrderByPart
    {
        public string Property;
        public bool Descending;

        public override string ToString() => Property + (Descending ? ".DESC" : null);
    }
}
