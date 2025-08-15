# Olive.Gpt

This document provides an overview and usage examples for the key public classes and methods in the `Olive.Gpt` namespace. It integrates with OpenAI's API for chat completions, image generation (DALL-E), and assistant management, with optional AWS S3 storage for images. A configuration section details optional settings for S3 integration in an `appsettings.json` file.

---

## Table of Contents

[Getting Started](#getting-started)
[Api](#api)
 - [Overview](#api-overview)
 - [Key Methods](#api-key-methods)
[AssistantApi](#assistantapi)
 - [Overview](#assistantapi-overview)
 - [Key Methods](#assistantapi-key-methods)
[Command](#command)
 - [Overview](#command-overview)
 - [Key Methods](#command-key-methods)
[Examples](#examples)
 
---
## Getting Started
- You will need an API Key from OpenAI. To obtain yours, you will need to create an account directly on www.openai.com and use that in the application settings file.
- In your project, add this nuget reference: `Olive.Gpt`
- Create an instance of the API: `var gpt = new Olive.Gpt.Api(Config.Get("OpenAIApi"));`
- Invoke `GetResponse(...)` on the api object.

For Example:
```csharp
var gpt = new Olive.Gpt.Api(Config.Get("OpenAIApi"));
string response = await gpt.GetResponse("Who invented the computer mouse?");
```
---

## Api

### Api Overview

The `Api` class is the primary interface for interacting with OpenAI's chat and image generation APIs. It supports single responses, streaming responses, transformation workflows, and DALL-E image generation with optional S3 storage.

### Api Key Methods

- **`GetResponse(string command, string model = null, ResponseFormats responseFormat = ResponseFormats.NotSet, Action<string> streamHandler = null)`**
  - Retrieves a single response from OpenAI for a command.
  - **Usage Example:**
    ```csharp
    var api = new Api("your-api-key");
    var response = await api.GetResponse("Write a short story");
    Console.WriteLine(response);
    ```

- **`GetResponseStream(string command, string model = null, ResponseFormats responseFormat = ResponseFormats.NotSet)`**
  - Streams responses from OpenAI for a command.
  - **Usage Example:**
    ```csharp
    var api = new Api("your-api-key");
    await foreach (var chunk in api.GetResponseStream("Tell me a joke"))
    {
        Console.Write(chunk);
    }
    ```

- **`GetTransformationResponse(IEnumerable<string> steps, ResponseFormats responseFormat = ResponseFormats.JsonObject)`**
  - Executes a series of transformation steps, feeding each result into the next.
  - **Usage Example:**
    ```csharp
    var api = new Api("your-api-key");
    var steps = new[] { "Write a greeting", "Format as JSON: #CURRENT_RESULT#" };
    var result = await api.GetTransformationResponse(steps);
    Console.WriteLine(result); // e.g., "{\"greeting\":\"Hello!\"}"
    ```

- **`GenerateDalleImage(string prompt, string model = "dall-e-3", Dictionary<string, object> parameters = null, bool saveToS3 = true)`**
  - Generates an image using DALL-E and optionally saves it to S3.
  - **Usage Example:**
    ```csharp
    var api = new Api("your-api-key");
    var url = await api.GenerateDalleImage("A dog in space");
    Console.WriteLine(url); // S3 URL or temporary OpenAI URL
    ```

---

## AssistantApi

### AssistantApi Overview

The `AssistantApi` class manages OpenAI Assistants, supporting creation, editing, deletion, and thread/message operations for both v1 and v2 API versions.

### AssistantApi Key Methods

- **`CreateNewAssistant(OpenAiCreateAssistantDto assistantDto)`**
  - Creates a new v1 assistant.
  - **Usage Example:**
    ```csharp
    var api = new AssistantApi("your-api-key");
    var assistant = new OpenAiCreateAssistantDto { Name = "Bot", Model = "gpt-3.5-turbo" };
    var assistantId = await api.CreateNewAssistant(assistant);
    Console.WriteLine(assistantId);
    ```

- **`EditAssistant(string assistantId, OpenAiCreateAssistantDto assistantDto)`**
  - Edits an existing assistant (v1).
  - **Usage Example:**
    ```csharp
    var api = new AssistantApi("your-api-key");
    var assistantDto = new OpenAiCreateAssistantDto { Name = "UpdatedBot" };
    var updatedId = await api.EditAssistant("assistant-id", assistantDto);
    ```

- **`CreateNewThread(List<ChatMessage> messages = null)`**
  - Creates a new thread with optional initial messages.
  - **Usage Example:**
    ```csharp
    var api = new AssistantApi("your-api-key");
    var threadId = await api.CreateNewThread(new List<ChatMessage> { new ChatMessage("user", "Hi") });
    ```

- **`AddMessageToThread(ChatMessage message, string threadId)`**
  - Adds a message to a thread.
  - **Usage Example:**
    ```csharp
    var api = new AssistantApi("your-api-key");
    var messageId = await api.AddMessageToThread(new ChatMessage("user", "Hello"), "thread-id");
    ```

- **`CreateNewRun(string assistantId, string threadId)`**
  - Creates a new run for an assistant on a thread.
  - **Usage Example:**
    ```csharp
    var api = new AssistantApi("your-api-key");
    var runId = await api.CreateNewRun("assistant-id", "thread-id");
    ```

- **`GetRunStatus(string threadId, string runId)`**
  - Retrieves the status of a run.
  - **Usage Example:**
    ```csharp
    var api = new AssistantApi("your-api-key");
    var status = await api.GetRunStatus("thread-id", "run-id");
    Console.WriteLine(status);
    ```

- **`GetThreadMessages(string threadId, string lastMessageId)`**
  - Retrieves messages from a thread after a specified message ID.
  - **Usage Example:**
    ```csharp
    var api = new AssistantApi("your-api-key");
    var messages = await api.GetThreadMessages("thread-id", "last-message-id");
    foreach (var msg in messages) Console.WriteLine(msg.Content);
    ```

---

## Command

### Command Overview

The `Command` class builds structured prompts for OpenAI by allowing customization of context, tone, format, and more, generating a human-readable string representation.

### Command Key Methods

- **`Command(string request)`**
  - Initializes a command with a base request.
  - **Usage Example:**
    ```csharp
    var command = new Command("Write a story");
    ```

- **`Context(string context)`**
  - Adds context to the command.
  - **Usage Example:**
    ```csharp
    var command = new Command("Write a story").Context("Set in a futuristic city");
    ```

- **`Tone(params Tone[] tones)`**
  - Specifies the tone(s) for the response.
  - **Usage Example:**
    ```csharp
    var command = new Command("Explain AI").Tone(Tone.Formal);
    ```

- **`Format(Format format)`**
  - Sets the response format.
  - **Usage Example:**
    ```csharp
    var command = new Command("List benefits").Format(Format.BulletPoints);
    ```

- **`ToString()`**
  - Generates the full command string.
  - **Usage Example:**
    ```csharp
    var command = new Command("Write a story")
        .Context("Set in a futuristic city")
        .Tone(Tone.Narrative)
        .Format(Format.Letter);
    Console.WriteLine(command.ToString());
    // Outputs: "Context: Set in a futuristic city\nTone: narrative\nFormat: story\nWrite a story"
    ```

--- 

## Examples
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

### What you can configure:

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

---

### Notes
- **API Key:** The OpenAI API key must be provided when instantiating `Api` or `AssistantApi` (e.g., `new Api("your-api-key")`). It is not sourced from configuration.
- **S3 Integration:** If `saveToS3` is `true` in `GenerateDalleImage`, S3 settings are required. If not provided, the method falls back to returning a temporary OpenAI URL.