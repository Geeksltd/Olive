# Olive GPT

This is a simple utility to interact with the OpenAI Chat GPT API. It's designed to promote good prompt engineering.


## Getting started
- You will need an API Key from OpenAI. To obtain yours, you will need to create an account directly on www.openai.com and use that in the application settings file.
- In your project, add this nuget reference: `Olive.Gpt`
- Create an instance of the API: `var gpt = new Olive.Gpt.Api(Config.Get("OpenAIApi"));`
- Invoke `GetResponse(...)` on the api object.

For Example:
```csharp
var gpt = new Olive.Gpt.Api(Config.Get("OpenAIApi"));
string response = await gpt.GetResponse("Who invented the computer mouse?");
```

## How to use properly?
The quality of what you get from ChatGPT depends entirely on the quality of your prompt.
In the above basic example, the response is most likely not what you expect.
What you should do is to provide more details about the context and expectations.

To help you with that, this Olive component provides the `Command` object, helping you get specific.

### Example 1
The following example, results in a single phrase (e.g. the name of the person) being returned, as opposed to a longer description:

```csharp
using Olive.Gpt;
...

var command = new Command("Who invented the computer mouse?")
			      .Length(Length.Phrase);

string response = await gpt.GetResponse(command);
```

### Example 2
The following example results in an essay with the following self-explanatory characteristics:

```csharp

var command = new Command("Who invented the computer mouse?")
                  .Context("Academic research homework about the history of the invention.")
                  .Audience("First-year computer science students")
                  .Length(Length.TwoPages)
                  .Format(Format.Essay)
                  .Tone(Tone.Formal, Tone.Narrative)
                  .Include(SupportingComponent.MultiplePerspectives, SupportingComponent.Citations);
 
string response = await gpt.GetResponse(command);
```


### Example 3
The following example is also self-explanatory. It will result in a very different outcome.

```csharp

var command = new Command("Who invented the computer mouse?") 
                  .Length(Length.ShortParagraph)
                  .Format(Format.Poem)
                  .Tone(Tone.Sarcastic, Tone.Humorous);
 
string response = await gpt.GetResponse(command);
```

## What you can configure:

You can specify:

- **Context** - Provide background information, data, or context for accurate content generation.
- **Objective** - State the goal or purpose of the response (e.g., inform, persuade, entertain).
- **Scope** - Define the scope or range of the topic.
- **Audience** - Specify the target audience for tailored content.
- **Examples** - Provide examples of desired style, structure, or content.
- **ActAs** - Indicate a role or perspective to adopt(e.g., expert, critic, enthusiast).   

You can also easily pick and choose:

- **Tone**: Formal, Informal, Persuasive, Descriptive, Expository, Narrative, Conversational, Sarcastic, Humorous, Emotional, Inspirational, Technical, Poetic, Slang, Colloquial, Euphemistic, Diplomatic, Authoritative, Didactic, Ironic, Cynical, Empathetic, Enthusiastic, Rhetorical, Objective, Subjective, Dismissive, Pessimistic, Optimistic
- **Length**: Phrase, Sentence, ShortParagraph, LongParagraph, TwoParagraphs, Page, TwoPages, FivePages
- **Format**: Essay, BulletPoints, Outline, Dialogue, List, Table, Flowchart, Definition, FAQs, Narrative, Poem, Anecdote, Script, NewsArticle, Review, Letter, Memo, Proposal, SocialMediaPost
- **Supporting Components**: Analogies, PreAddressCounterArguments, MultiplePerspectives, Citations, Quotes, Statistics
