# Olive.Entities.Data.Replication.DataGenerator.UI

## Overview

The Olive Replication Data Generator UI provides developer commands and UI elements for easily generating mock data for entities managed by Olive's replication module. It facilitates quick creation, viewing, and management of test data, significantly streamlining the development and testing processes.

---

## Table of Contents:

- [Key Components](#key-components)
- [ReplicationDataGeneratorUICommand](#replicationdatageneratoruicommand)
- [Serialization Resolver](#replicationmessageserializationresolver)
- [IModuleGenerator Interface](#imodulegenerator-interface)
- [ModuleGenerators](#module-generators)
  - [BaseModuleGenerator](#basemodulegenerator-class)
  - [FormModuleGenerator](#formmodulegenerator-class)
  - [FormSubmitGenerator](#formsubmitgenerator-class)
  - [ListModuleGenerator](#listmodulegenerator-class)
- [Extension Methods](#extension-methods)
- [Usage Examples](#usage-examples)

---

## Key Components

- **ReplicationDataGeneratorUICommand**: Developer command to invoke or render different UI modules.
- **IModuleGenerator Interface**: Defines methods for UI module generation (list, form, submit).
- **Serialization resolver**: Ensures entity serialization for replication messages.
- **BaseModuleGenerator & subclasses**: Generate HTML view components dynamically based on entity definitions.
- **Extensions methods**: Easily integrates mocking capabilities into services and dependency injection.

---

## ReplicationDataGeneratorUICommand

Implements `IDevCommand`, allowing developers to generate HTML UI based on Olive replication entities directly through configured development commands.

### Methods and Properties:

- `Name`: Command identifier as `"generate-{Namespace}-{Type}"`.
- `Title`: Human-readable identifier.
- `IsEnabled()`: Checks if endpoint is properly initialized.
- `Run()`: Generates the appropriate UI based on user HTTP requests (list views, form views, or form submissions).

### Example:

```csharp
var command = new ReplicationDataGeneratorUICommand("MyNamespace", "User");
command.SetEndpoint(myEndpoint);
string content = await command.Run();
```

---

## ReplicationMessageSerializationResolver

Custom contract resolver derived from `DefaultContractResolver` used to serialize entities into replication messages:

- Ensures only writable properties are serialized.
- Primarily used internally when generating replication messages for mock data.

---

## IModuleGenerator Interface

Defines a consistent way to generate UI modules:

- `Render(string nameSpace, string typeName)`: Generate HTML content for the entity interaction (list, edit form, submit).

Implemented by:
- `FormModuleGenerator`
- `FormSubmitGenerator`
- `ListModuleGenerator`

---

## Module Generators

Common classes extending `BaseModuleGenerator` to generate different UI modules dynamically.

### BaseModuleGenerator Class

- Shared functionalities like rendering HTML basics, fetching entity type information, and extracting property definitions.
- Provides base rendering logic used across sub-generators.

### FormModuleGenerator Class

Generates an HTML form for creating new entity instances:

- Dynamically creates fields based on entity type properties.
- Supports different HTML input types based on entity property types (text, checkbox, etc.).

### FormSubmitGenerator Class

Processes submitted form data, saves the entity to the database through the replication system:

- Handles form submission logic, data validation, and replication message generation.
- Provides error handling and validation feedback mechanisms.

### ListModuleGenerator Class

Displays existing entity records as an HTML table:

- Dynamically constructs tables based on entity property definitions.
- Provides an intuitive view to examine existing mock data.

---

## Extension Methods

### Usage within Startup.cs:

- **AddMockEndpoint\<T\>(...)**: Registers defined entities into the application’s service collection for mock UI generation.

- **UseMockEndpoint(...)**: Associates the configured endpoint with registered developer commands within the application’s runtime provider.

Example:
```csharp
// Register in ConfigureServices method
services.AddMockEndpoint<MyDestinationEndpoint>("MyNamespace");

// Configure the endpoint during application startup
provider.UseMockEndpoint(myDestinationEndpoint, "MyNamespace");
```

---

## Usage Examples 

### Using the Generated Developer Commands:

Via browser or tool:
- To see a list view:
  ```
  /cmd/generate-MyNamespace-User
  ```
- To access a form view directly for creating new data:
  ```
  /cmd/generate-MyNamespace-User?action=new
  ```

---

## Important Notes:

- Proper endpoint initialization (`SetEndpoint()` or using dependency injection) is required.
- HTML rendering follows Bootstrap for styling and responsiveness.
- Extension methods simplify registrations and wiring; always use them for easier maintainability.

---

## Exception Handling:

Appropriate validation messages and exception catching are provided in form submission to aid developer debugging during UI data generation sessions.