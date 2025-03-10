# Olive.OpenAl.Voice

This document provides an overview and usage example for the public class and method in the `Olive.OpenAI.Voice` namespace. It enables interaction with OpenAI's audio transcription capabilities, specifically for converting audio data to text using a speech-to-text model.
Ensure that the `OpenAI:Key` configuration is properly set in your application settings before instantiation.

---

## Table of Contents

1. [OpenAI](#openai)
   - [Overview](#openai-overview)
   - [Methods](#openai-methods)

---

## OpenAI

### OpenAI Overview

The `OpenAI` class provides a simple interface to transcribe audio data into text using OpenAI's audio transcription service. It leverages the `AudioClient` from the `OpenAI.Audio` namespace and requires an API key and model configuration from the application's settings.

### OpenAI Methods

- **`ToText(byte[] audio)`**
  - Asynchronously converts an audio byte array to text using OpenAI's speech-to-text model.
  - **Usage Example:**
    ```csharp
    // Ensure OpenAI:Key is set in AppSettings or web.config, e.g., "your-openai-api-key"
    var openAI = new OpenAI();

    // Example: Load an MP3 audio file as a byte array
    byte[] audioData = File.ReadAllBytes("sample.mp3");
    
    // Transcribe the audio to text
    string text = await openAI.ToText(audioData);
    Console.WriteLine(text); // Outputs the transcribed text, e.g., "Hello, this is a test."
    ```