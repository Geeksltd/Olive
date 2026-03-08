**# Olive.AzureSearch**

## Overview
`Olive.AzureSearch` is a repository designed to work with Azure Cognitive Search. It provides functionalities to manage search filters, push data, retrieve search results, and remove search filters.

---

## **Class: AzureSearchFilterRepository<T>**

### **Constructor**
```csharp
public AzureSearchFilterRepository(AzureSearchConfigModel configuration)
```
- Initializes the repository with an Azure search index based on the provided `AzureSearchConfigModel`.

### **Methods**

#### **Clear()**
```csharp
public void Clear()
```
- Clears the current index if it exists and reinitializes it.
- Handles exceptions where the index is not found.

#### **PushSearchFilter(IEnumerable<T> searchFilters)**
```csharp
public void PushSearchFilter(IEnumerable<T> searchFilters)
```
- Uploads a batch of search filters into the index.
- Uses `IndexDocumentsBatch.Create()` to process multiple documents.

#### **GetBySearchTerm(string searchTerm, SearchOptions searchOptions = null, int maxCountToAddSuffixResponse = 60)**
```csharp
public IEnumerable<T> GetBySearchTerm(string searchTerm, SearchOptions searchOptions = null, int maxCountToAddSuffixResponse = 60)
```
- Searches the index using the provided search term.
- If the initial search does not return enough results, it performs an additional search with a suffix pattern.
- Returns distinct results.

#### **RemoveSearchFilter(string searchTerm, params string[] searchFields)**
```csharp
public bool RemoveSearchFilter(string searchTerm, params string[] searchFields)
```
- Searches for records matching the given search term and removes them from the index.
- Uses `DeleteDocuments()` to delete matching documents.

#### **GetDefaultSearchOptions()**
```csharp
public SearchOptions GetDefaultSearchOptions()
```
- Creates default search options, setting filterable fields and defining query settings.
 
---

## **Class: AzureSearchConfigModel**

### **Properties**
```csharp
public string AdminKey { get; set; }
public string ServiceUrl { get; set; }
public bool Enabled { get; set; }
public string DefaultIndex { get; set; }
```
- Contains configuration settings required for connecting to Azure Cognitive Search.

---

## **Class: AzureSearchFilterDTOBase**

### **Properties**
```csharp
[SimpleField(IsKey = true)]
public string Id { get; set; }
```
- Represents the base DTO for Azure search filters.
- Includes an `Id` field marked as a unique key.

---

## **Usage Example**

```csharp
var config = new AzureSearchConfigModel
{
    AdminKey = "your-admin-key",
    ServiceUrl = "https://your-search-service.search.windows.net",
    DefaultIndex = "your-index",
    Enabled = true
};

var repository = new AzureSearchFilterRepository<MySearchDTO>(config);

// Add filters
repository.PushSearchFilter(new List<MySearchDTO>
{
    new MySearchDTO { Id = "1", Name = "Example" }
});

// Retrieve search results
var results = repository.GetBySearchTerm("Example");

// Remove a filter
repository.RemoveSearchFilter("Example");
```

---

## **Notes**
- Ensure that the Azure Cognitive Search service is correctly configured before using the repository.
- The `AdminKey` must be valid to perform index modifications.
- Search queries support full-text search and wildcard matching.

