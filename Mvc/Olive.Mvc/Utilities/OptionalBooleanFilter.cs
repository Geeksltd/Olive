namespace Olive.Mvc
{
    public class OptionalBooleanFilter
    {
        public readonly bool? Value;
        public readonly static OptionalBooleanFilter Null = new OptionalBooleanFilter(null);
        public readonly static OptionalBooleanFilter True = new OptionalBooleanFilter(true);
        public readonly static OptionalBooleanFilter False = new OptionalBooleanFilter(false);

        private OptionalBooleanFilter(bool? value) => Value = value;

        public override string ToString()
        {
            if (Value == null) return "Null";
            else return Value.Value.ToString();
        }

        public static OptionalBooleanFilter Parse(string text)
        {
            text = text.OrEmpty().ToLower();

            if (text == "null") return Null;

            if (text.IsAnyOf("true", "yes")) return True;

            if (text.IsAnyOf("false", "no")) return False;

            return null;
        }

        public static implicit operator OptionalBooleanFilter(bool? value) => new OptionalBooleanFilter(value);

        public static implicit operator bool? (OptionalBooleanFilter value) => value.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            try
            {
                return Value == ((OptionalBooleanFilter)obj).Value;
            }
            catch
            {
                // No logging is needed
                return false;
            }
        }
    }
}