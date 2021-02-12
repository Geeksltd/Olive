namespace Olive
{
    using System;
    using System.Linq;

    partial class OliveExtensions
    {
        static string[] SkeletonWords => new[] { "▅▅▅▅▅▅▅", "▅▅▅", "▅▅▅▅", "▅▅▅▅▅", "▅▅" };

        /// <summary>
        /// Refreshes this bindable's value when the specified other dependency is updated.
        /// This is useful for calculation based bindables, whose expression uses the dependency.
        /// </summary>
        public static T RefreshOn<T, TDependency>(this T @this, Bindable<TDependency> dependency)
            where T : Bindable
        {
            if (dependency is null) throw new ArgumentNullException(nameof(dependency));
            dependency.Changed += @this.Refresh;
            return @this;
        }

        /// <summary>
        /// If it's true, makes it false. If false, makes it true.
        /// </summary>
        public static void Toggle(this Bindable<bool> @this) => @this.Value = !@this.Value;

        public static T Skeleton<T>(this T @this, int words = 1) where T : Bindable<string>
        {
            @this.Value = Enumerable.Range(0, words)
                .Select(index => SkeletonWords[index % SkeletonWords.Length])
                .ToString(" ");

            return @this;
        }
    }
}