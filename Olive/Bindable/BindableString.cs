using System.Linq;
using Olive;

namespace Olive
{
    public class BindableString : Bindable<string>
    {
        static string[] SkeletonWords => new[] { "▅▅▅▅▅▅▅", "▅▅▅", "▅▅▅▅", "▅▅▅▅▅", "▅▅" };

        public readonly Bindable<string> State = new Bindable<string>();

        public BindableString() => State.Bind(this, x => "skeleton".OnlyWhen(IsSkeleton(x)));

        static bool IsSkeleton(string text) => text.HasValue() && text.All(x => x.IsAnyOf(' ', '▅'));

        public BindableString Skeleton(int words = 1)
        {
            Value = Enumerable.Range(0, words)
                .Select(index => SkeletonWords[index % SkeletonWords.Length])
                .ToString(" ");

            return this;
        }
    }
}