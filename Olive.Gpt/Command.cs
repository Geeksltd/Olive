using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Olive.Gpt
{
    public partial class Command
    {
        string context, scope, request, audience, examples, language, objective, actAs;
        Tone[] tones;
        Format? format;
        Length? length;
        SupportingComponent[] include;

        public Command(string request) => this.request = request;

        /// <summary>
        /// Provide background information, data, or context for accurate content generation.
        /// </summary>
        public Command Context(string context) => Do(this.context = context);

        public Command Length(Length length) => Do(this.length = length);

        /// <summary>
        /// Indicate the language for the response, if different from the prompt.
        /// </summary>
        public Command Expect(string language) => Do(this.language = language);

        public Command Format(Format format) => Do(this.format = format);

        public Command Tone(params Tone[] tones) => Do(this.tones = tones);
        
        public Command Include(params SupportingComponent[] items) => Do(include = items);
        
        /// <summary>
        /// State the goal or purpose of the response (e.g., inform, persuade, entertain).
        /// </summary>
        public Command Objective(string objective) => Do(this.objective = objective);

        /// <summary>
        /// Indicate a role or perspective to adopt(e.g., expert, critic, enthusiast).
        /// </summary>
        public Command ActAs(string actAs) => Do(this.actAs = actAs);

        /// <summary>
        /// Define the scope or range of the topic.
        /// </summary>
        public Command Scope(string scope) => Do(this.scope = scope);

        /// <summary>
        /// Specify the target audience for tailored content.
        /// </summary>
        public Command Audience(string audience) => Do(this.audience = audience);

        /// <summary>
        /// Provide examples of desired style, structure, or content.
        /// </summary>
        public Command ForExample(string example) => Do(this.examples = example);

        Command Do(object alreadySet) => this;

        public override string ToString()
        {
            var r = new StringBuilder();

            if (context.HasValue()) r.AppendLine("Context: " + context);
            if (scope.HasValue()) r.AppendLine("Scope: " + scope);
            if (audience.HasValue()) r.AppendLine("Audience: " + audience);
            if (objective.HasValue()) r.AppendLine("Objective: " + objective);
            if (language.HasValue()) r.AppendLine("Language: " + language);
            if (actAs.HasValue()) r.AppendLine("Act as: " + actAs);
            if (tones.HasAny()) r.AppendLine("Tone: " + tones.Select(x => x.ToString().ToLiteralFromPascalCase()).ToString(", ", " and "));
            if (format != null) r.AppendLine("Format: " + format.ToString().ToLiteralFromPascalCase());
            if (length != null) r.AppendLine("Length: " + length.ToString().ToLiteralFromPascalCase());
            r.AppendLine(request);

            if (examples != null) r.AppendLine("\nFor example: " + examples);
            if (include.HasAny()) r.AppendLine("\nInclude: " + include.Select(x => x.ToString().ToLiteralFromPascalCase()).ToString(", ", " and "));

            return r.ToString();
        }
    }
}