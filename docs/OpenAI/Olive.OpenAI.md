# Olive.OpenAI

This document provides an overview and usage examples for the public class and methods in the `Olive.OpenAI` namespace. It enables interaction with OpenAI's chat API, supporting both single-response and streaming-response modes. A configuration section details the required settings in an `appsettings.json` file.

---

## Table of Contents

1. [OpenAI](#openai)
   - [Overview](#openai-overview)
   - [Methods](#openai-methods)
2. [Configuration](#configuration)

---

## OpenAI

### OpenAI Overview

The `OpenAI` class provides an interface to OpenAI's chat functionality using the `ChatClient` from the `OpenAI.Chat` namespace. It allows sending messages and receiving responses either as a single string or as a streaming sequence, requiring an API key and optionally a model configuration from `appsettings.json`.

### OpenAI Methods

- **`GetResponse(string[] messages, string instruction = null)`**
  - Asynchronously sends an array of messages to OpenAI and returns a single response as a string.
  - **Usage Example:**
    ```csharp
    var openAI = new OpenAI(); // Assumes OpenAI:Key is set in appsettings.json
    string[] messages = { "Hello, how are you?", "Can you help me today?" };
    string response = await openAI.GetResponse(messages, "Respond in a friendly tone");
    Console.WriteLine(response); // Outputs: "Hi there! I'm doing great, thanks for asking. How can I assist you today?"
    ```

- **`GetResponseStream(string[] messages, string instruction = null)`**
  - Asynchronously streams responses from OpenAI for an array of messages, yielding chunks of text as they arrive.
  - **Usage Example:**
    ```csharp
    var openAI = new OpenAI(); // Assumes OpenAI:Key is set in appsettings.json
    string[] messages = { "Tell me a story" };
    await foreach (var chunk in openAI.GetResponseStream(messages, "Tell a short story about a cat"))
    {
        Console.Write(chunk); // Outputs story chunks as they stream, e.g., "Once" "upon" "a time..."
    }
    ```

---

## Configuration

The `OpenAI` class relies on specific configuration settings stored in an `appsettings.json` file with a JSON structure. Below are the required and optional configurations:

- **`OpenAI:Key`** (Required)
  - The API key for authenticating with OpenAI's services.
  - **Example:**
    ```json
    {
      "OpenAI": {
        "Key": "your-openai-api-key"
      }
    }
    ```
  - If not provided via constructor or config, an `ArgumentException` will be thrown.

- **`OpenAI:Models:ChatModel`** (Optional)
  - Specifies the chat model to use (defaults to `"gpt-4o"` if not set).
  - **Example:**
    ```json
    {
      "OpenAI": {
        "Key": "your-openai-api-key",
        "Models": {
          "ChatModel": "gpt-3.5-turbo"
        }
      }
    }
    ```

### Full `appsettings.json` Example
```json
{
  "OpenAI": {
    "Key": "your-openai-api-key",
    "Models": {
      "ChatModel": "gpt-3.5-turbo"
    }
  }
}
```

### Notes
- If you pass an API key directly to the `OpenAI` constructor (e.g., `new OpenAI("your-key")`), it will override the `OpenAI:Key` from `appsettings.json`.
- Ensure the API key is valid and has appropriate permissions for chat functionality.
- The `Olive.Config.Get` method is assumed to read from `appsettings.json` correctly in your application setup.