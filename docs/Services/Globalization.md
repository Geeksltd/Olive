# Olive.Globalization

## Overview

The Olive Globalization module provides comprehensive language localization and translation capabilities, making it easy to support multiple languages within Olive-based applications. It seamlessly integrates translation services such as Google Translate, caches translations locally to improve performance, and provides utilities to translate strings, phrases, and entire HTML contents efficiently.

---

## Table of Contents

- [Getting Started](#getting-started)
    - [Installation](#installation)
    - [Configuration](#configuration)
- [Key Features](#key-features)
    - [Translation Utilities](#translation-utilities)
    - [Integrating Google Translate](#integrating-google-translate)
    - [Automatic Translation Caching](#automatic-translation-caching)
    - [Language and Phrase Interfaces](#language-and-phrase-interfaces)
- [Core Components](#core-components)
    - [Translator](#translator-class)
    - [ILanguage Interface](#ilanguage-interface)
    - [ITranslationProvider Interface](#itranslationprovider-interface)
    - [IPhraseTranslation Interface](#iphrasetranslation-interface)
- [Using EnglishSpelling Helper](#using-englishspelling-helper)
- [Usage Examples](#usage-examples)
- [Handling HTML Translations](#handling-html-translations)
- [Customization and Advanced Use](#customization-and-advanced-use)
- [Dependencies](#dependencies)
- [Exception Handling and Logging](#exception-handling-and-logging)
- [Important Notes](#important-notes)

---

## Getting Started

### Installation

Include Olive Globalization in your project via NuGet:

```powershell
Install-Package Olive.Globalization
```

### Configuration

Configure Google Translate API keys in your `appsettings.json` file:

```json
{
  "Globalization": {
    "GoogleTranslate": {
      "Enabled": true,
      "ApiKey": "[YOUR_GOOGLE_TRANSLATE_API_KEY]",
      "QueryLength": 115,
      "CharacterLimit": 2000
    }
  }
}
```

---

## Key Features

### Translation Utilities

Provides seamless translation of phrases with support for special character preservation and decorator handling.

- Easy-to-use API methods
- Automatic support for determining context language and default language
- Prevents unnecessary translation API calls with local caching

### Integrating Google Translate

- Supports Google's powerful translation services
- Caches translations locally to optimize cost and improve performance
- Can detect the language automatically and provide confidence levels

### Automatic Translation Caching

Translations from external translation providers (such as Google Translate) are automatically cached locally, significantly improving translation performance and reducing external API calls.

### Language and Phrase Interfaces

Abstraction through interfaces (`ILanguage`, `IPhraseTranslation`, and `ITranslationProvider`) enables easy integration and customization of translation and localization services.

---

## Core Components

### Translator Class

The main class managing translations and coordinating between local cache and external providers.

**Methods & Events:**

- `Translate(...)`: Translate plain text or phrases to the desired language.
- `TranslateHtml(...)`: Translate entire HTML content intelligently while preserving its structure.
- `TranslationDownloaded` event: Invoked when a translation is downloaded externally.

### ILanguage Interface

Provides language definitions, such as English, French or Spanish.

**Properties:**

- `string Name`: Friendly name (e.g. "English").
- `string IsoCode`: ISO language code (e.g. "en").
- `bool IsDefault`: Determines if this is the application's default language.

### ITranslationProvider Interface

Defines methods for external translation providers that can translate phrases to different languages.

Methods:

- `Translate(string phrase, string sourceIsoCode, string targetIsoCode)`: Translate phrases between languages.
- `MaximumTextLength`: Maximum characters supported per translation request.

### IPhraseTranslation Interface

Defines translations fetched from translation providers, stored locally for reuse.

Properties:

- `Phrase`: Original phrase
- `Translation`: Translated text
- `Language`: Language to which phrase was translated

---

## Using EnglishSpelling Helper

Olive provides functionality to switch between British and American spelling variants of English content.

Usage Example:

```csharp
var britishText = EnglishSpelling.ToBritish("color");
var americanText = EnglishSpelling.ToAmerican("colour");
```

---

## Usage Examples

### Translating Simple Text Phrases

```csharp
// Translate "Hello World" into Spanish
var language = await Context.Current.Language();
var translation = await Translator.Translate("Hello World", language);
```

### Implementing TranslationDownloaded Event Handler

```csharp
Translator.TranslationDownloaded += async args =>
{
    // Save translations locally when downloaded
    await Database.Save(entity);
};
```

---

## Handling HTML Translations

Efficiently translates complex HTML content structures without breaking their formatting.

```csharp
var originalHtml = "<p>Hello World</p>";
var language = await Context.Current.Language();

var translatedHtml = await Translator.TranslateHtml(originalHtml, language);
```

---

## Customization and Advanced Use

### Implementing Custom Context Language Provider

Implement `IContextLanguageProvider` to define a custom logic to retrieve the current language of a context:

```csharp
public class MyLanguageProvider : IContextLanguageProvider
{
    public Task<ILanguage> GetCurrentLanguage()
    {
        // Get language based on custom logic
    }
}
```

Register this provider in your application's dependency injection container.

### Handling TranslationDownloaded Events

You can hook into global translation events. Useful for logging or storing translations:

```csharp
Translator.TranslationDownloaded += async args =>
{
    // Track downloaded translations
    await Database.Save(entity);
};
```

---

## Dependencies

- Olive framework core packages:
```
Install-Package Olive
Install-Package Olive.Entities
Install-Package Olive.Globalization
```

- Google API Access: Configure Google Translate API keys properly before using the service.

---

## Exception Handling and Logging

- Comprehensive logging of translation process via Olive's built-in logging.
- Detailed exception handling and logging for translation API errors.
- Informs of configuration issues (e.g., invalid API keys or exceeded quotas).

Example exception message:

```
Exception: Google API Error: REQUEST_DENIED
```

---

## Important Notes

- Google Translate has a query character limit per request. Ensure text chunks do not exceed configured limit.
- Regularly monitor translation quotas and API usage limits provided by Google.