using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Olive.Globalization
{
    public class EnglishSpelling
    {
        string Original;
        string[] ToReplace;
        readonly Dictionary<string, string> Map;

        public EnglishSpelling(string htmlOrText, Dictionary<string, string> map)
        {
            Original = htmlOrText.TrimOrEmpty();
            Map = map;
            if (Original.HasValue())
                ToReplace = map.Where(v => Original.ContainsWholeWord(v.Key)).Select(v => v.Key).ToArray();
        }

        public static string ToBritish(string htmlOrText) => new EnglishSpelling(htmlOrText, SpellingMap.AmericanToBritish).Convert();
        public static string ToAmerican(string htmlOrText) => new EnglishSpelling(htmlOrText, SpellingMap.BritishToAmerican).Convert();

        public string Convert()
        {
            if (Original.IsEmpty()) return string.Empty;
            if (ToReplace.None() || Original.Lacks("<")) return Replace(Original);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(Original);

            Replace(htmlDoc.DocumentNode);

            return htmlDoc.DocumentNode.InnerHtml;
        }

        void Replace(HtmlNode node)
        {
            node.ChildNodes.Do(Replace);

            if (node.NodeType == HtmlNodeType.Text)
            {
                var to = node.InnerHtml;
                if (to.IsEmpty()) return;
                if (to.ContainsAny(ToReplace))
                    node.InnerHtml = Replace(to);
            }
        }

        string Replace(string text)
        {
            if (text.ContainsAny(ToReplace))
                ToReplace.Do(v => text = text.ReplaceWholeWord(v, Map[v]));

            return text;
        }
    }

    static class SpellingMap
    {
        public readonly static Dictionary<string, string> AmericanToBritish, BritishToAmerican;

        static SpellingMap()
        {
            AmericanToBritish = ConvertIze();
            AmericanToBritish.Add(Others());
            BritishToAmerican = AmericanToBritish.ToDictionary(v => v.Value, v => v.Key);
        }

        static Dictionary<string, string> ConvertIze() => "accessorize,acclimatize,acclimatized,acclimatizes,acclimatizing,actualize,agonize,agonized,agonizes,agonizing,alphabetize,americanize,amortize,anaesthetize,anglicize,antagonize,antagonized,antagonizes,antagonizing,apologize,apologized,apologizes,apologizing,apostrophize,atomize,authorize,authorized,authorizes,authorizing,baptize,baptized,baptizes,baptizing,bastardize,brutalize,brutalized,cannibalize,canonize,capitalize,capitalized,capitalizes,capitalizing,carbonize,carburize,categorize,categorized,categorizing,cauterize,cauterized,cauterizes,cauterizing,centralize,centralized,characterize,characterized,characterizes,characterizing,civilize,civilized,civilizes,civilizing,colonize,colonized,colonizes,colonizing,commercialize,commercialized,compartmentalize,compartmentalized,compartmentalizing,contextualize,contextualized,criminalize,criminalizing,criticize,criticized,criticizes,criticizing,crystallize,crystallized,crystallizes,crystallizing,customize,customized,decarbonize,decentralize,decentralized,decimalize,dehumanize,delegitimize,demobilize,democratize,democratized,democratizing,demonize,demonized,demonizing,demoralize,demoralized,denationalize,deputize,destabilize,digitize,digitized,diphthongize,dramatize,dramatized,dramatizes,dramatizing,economize,economized,economizes,economizing,empathize,energize,energized,energizes,energizing,epitomize,epitomized,epitomizes,epitomizing,equalize,equalizing,eulogize,eulogized,eulogizes,eulogizing,europeanize,evangelize,evangelized,evangelizes,evangelizing,extemporize,extemporized,extemporizes,extemporizing,externalize,factorize,familiarize,feminize,fertilize,fertilized,fertilizes,fertilizing,fetishize,fictionalize,fictionalized,focalize,formalize,formalized,fossilize,fraternize,fraternized,fraternizes,fraternizing,galvanize,galvanized,galvanizes,galvanizing,generalize,generalized,generalizes,generalizing,glamorize,globalize,globalized,harmonize,harmonized,harmonizes,harmonizing,hospitalize,humanize,humanizes,humanizing,hybridize,hypnotize,hypnotized,idealize,idealized,idealizes,idealizing,idolize,idolized,idolizing,immobilize,immortalize,immunize,incentivize,incentivized,individualize,individualized,industrialize,industrialized,institutionalize,internalize,internalized,internalizing,internationalize,italicize,itemize,jeopardize,jeopardized,jeopardizes,jeopardizing,legalize,legalized,legalizes,legalizing,legitimize,legitimized,legitimizes,legitimizing,liberalize,liberalized,liberalizes,liberalizing,lionize,liquidize,localize,localized,localizes,localizing,magnetize,magnetized,magnetizes,magnetizing,marginalize,marginalized,materialize,materialized,materializes,maximize,maximized,maximizes,maximizing,mechanize,memorialize,memorize,memorized,memorizes,memorizing,mesmerize,mesmerized,mesmerizes,mesmerizing,metabolize,minimize,minimized,minimizes,minimizing,mobilize,mobilized,mobilizes,mobilizing,modernize,modernized,modernizes,modernizing,moisturize,moisturized,moisturizing,monetize,monetized,monetizing,monopolize,monopolized,monopolizes,monopolizing,moralize,moralized,moralizes,moralizing,nasalize,nationalize,naturalize,naturalized,neutralize,neutralized,neutralizes,neutralizing,nominalize,normalize,normalized,normalizing,optimize,optimized,optimizing,organize,organized,organizes,organizing,ostracize,ostracized,ostracizes,ostracizing,oxidize,oxidized,oxidizes,oxidizing,palatalize,particularize,pasteurize,patronize,patronized,patronizes,patronizing,pedestrianize,penalize,penalized,penalizes,penalizing,personalize,personalized,personalizing,philosophize,picturize,plagiarize,plagiarized,plagiarizes,plagiarizing,pluralize,polarize,polarized,polarizing,politicize,politicized,polymerize,popularize,popularized,popularizes,popularizing,prioritize,prioritized,prioritizes,prioritizing,privatize,privatized,privatizing,proselytize,proselytized,proselytizes,proselytizing,publicize,publicized,publicizing,pulverize,pulverized,pulverizes,pulverizing,radicalize,radicalized,rationalize,rationalized,rationalizes,rationalizing,realize,realized,realizes,realizing,reauthorize,reauthorized,recognize,recognized,recognizes,recognizing,regularize,reorganize,reorganized,reorganizes,reorganizing,revitalize,revitalized,revitalizes,revitalizing,revolutionize,revolutionized,revolutionizes,revolutionizing,rhapsodize,romanticize,romanticized,romanticizing,satirize,satirized,satirizing,scandalize,scandalized,scandalizes,scandalizing,schematize,scrutinize,scrutinized,scrutinizes,scrutinizing,secularize,sensationalize,sensationalized,sensitize,sensitized,sentimentalize,sermonize,sexualize,socialize,socialized,socializing,solemnize,soliloquize,specialize,specialized,specializes,specializing,stabilize,stabilized,stabilizes,stabilizing,standardize,standardized,sterilize,sterilized,sterilizes,sterilizing,stigmatize,stigmatized,stigmatizes,stigmatizing,subsidize,subsidized,subsidizes,subsidizing,summarize,summarized,summarizes,summarizing,symbolize,symbolized,symbolizes,symbolizing,sympathize,sympathizing,synchronize,synchronized,synchronizes,synchronizing,systematize,tantalize,tantalized,tantalizes,tantalizing,temporize,temporized,temporizes,temporizing,terrorize,terrorized,terrorizes,terrorizing,theorize,theorized,theorizes,theorizing,tranquillize,tranquillized,tranquillizes,tranquillizing,trivialize,trivialized,tyrannize,tyrannized,tyrannizes,tyrannizing,unauthorized,uncivilized,utilize,utilized,utilizes,utilizing,vaporize,vaporized,vaporizes,vaporizing,verbalize,victimize,victimized,victimizes,victimizing,visualize,visualized,visualizes,visualizing,vocalize,vulgarize"
            .Split(',').SelectMany(x => new[] { x, x[0].ToUpper() + x.Substring(1) })
            .ToDictionary(x => x, x => x.Replace("z", "s"));

        static Dictionary<string, string> Others() => "aluminum:aluminium|catalog:catalogue|cataloged:catalogued|cataloging:cataloguing|catalogs:catalogues|center:centre|centered:centred|centering:centring|centers:centres|color:colour|colored:coloured|coloring:colouring|colors:colours|defense:defence|defenses:defences|dreamed:dreamt|enroll:enrol|enrolls:enrols|favorite:favourite|favorites:favourites|fiber:fibre|fibers:fibres|fulfill:fulfil|fulfills:fulfils|gray:grey|grayed:greyed|graying:greying|grays:greys|jewelry:jewellery|labor:labour|labored:laboured|laboring:labouring|labors:labours|maneuver:manoeuvre|maneuvered:manoeuvred|maneuvering:manoeuvring|maneuvers:manoeuvres|meter:metre|metered:metred|metering:metring|meters:metres|mold:mould|molded:moulded|molding:moulding|molds:moulds|neighbor:neighbour|neighbored:neighboured|neighboring:neighbouring|neighbors:neighbours|offense:offence|offenses:offences|pajamas:pyjamas|plow:plough|plowed:ploughed|plowing:ploughing|plows:ploughs|skillful:skilful|spelled:spelt|sulfur:sulphur|sulfured:sulphured|sulfuring:sulphuring|sulfurs:sulphurs|theater:theatre|theaters:theatres|toward:towards|traveler:traveller|travelers:travellers"
        .Split('|').Distinct().Select(c => c.Split(':')).ToDictionary(x => x[0], x => x[1]);
    }
}