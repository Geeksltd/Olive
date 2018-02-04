using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive.Entities;

namespace Olive.Email
{
    partial class EmailExtensions
    {
        /// <summary>
        /// Gets the mandatory placeholder tokens for this template.
        /// </summary>
        public static IEnumerable<string> GetPlaceholderTokens(this IEmailTemplate template) =>
            template.MandatoryPlaceholders.Or("").Split(',').Trim().Select(t => $"[#{t.ToUpper()}#]");

        /// <summary>
        /// Ensures the mandatory placeholders are all specified in this template.
        /// </summary>
        public static void EnsurePlaceholders(this IEmailTemplate template)
        {
            // Make sure that all place holders appear in the email body or subject.
            var missingElements = template.GetPlaceholderTokens().Except(t => (template.Subject + template.Body).Contains(t));
            if (missingElements.Any())
                throw new ValidationException("Email template subject or body must have all place-holders for {0}. The missing ones are: {1}", template.Key, missingElements.ToString(", "));
        }

        /// <summary>
        /// Merges the subjcet of this email template with the specified data.
        /// </summary>
		/// <param name="template">The email template</param>
        /// <param name="mergeData">An anonymouse object. All property names should correspond to the placeholder names.
        /// For example: new {FirstName = GetFirstName() , LastName = "john"}</param>
        public static string MergeSubject(this IEmailTemplate template, object mergeData) => Merge(template.Subject, mergeData);

        /// <summary>
        /// Merges the body of this email template with the specified data.
        /// </summary>
        /// <param name="template">The email template</param>
        /// <param name="mergeData">An anonymouse object. All property names should correspond to the placeholder names.
        /// For example: new {FirstName = GetFirstName() , LastName = "john"}</param>
        public static string MergeBody(this IEmailTemplate template, object mergeData) => Merge(template.Body, mergeData);

        /// <summary>
        /// Merges the specified template with the provided.
        /// </summary>
        static string Merge(string template, object mergeData)
        {
            var result = template;

            foreach (var p in mergeData.GetType().GetProperties())
            {
                var key = $"[#{p.Name.ToUpper()}#]";
                var value = $"{p.GetValue(mergeData)}";

                result = result.Replace(key, value);
            }

            return result;
        }
    }
}