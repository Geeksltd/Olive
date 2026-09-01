# Olive.Email.Imap

## Overview
This documentation provides details about the `Olive.Email.Imap` and `ImapService`, which are used for handling email failures and interacting with an IMAP server to retrieve and process emails.

---

## **BaseEmailFailureService**
### **Description**
The `BaseEmailFailureService` is an abstract class that handles failure emails by fetching new messages from an IMAP service, checking if they indicate a failure, and marking the corresponding email messages as failed in the database.

### **Constructor**
```csharp
public BaseEmailFailureService(
    IImapService imapService,
    IDatabase database,
    ILogger<BaseEmailFailureService> logger)
```
- `imapService`: An instance of `IImapService` to retrieve emails.
- `database`: Database access for storing email failure logs.
- `logger`: Logging service for recording email failure events.

### **Methods**
#### **Check()**
```csharp
public async Task Check()
```
- Retrieves new email messages.
- Checks if an email is a failure.
- Marks related emails as failure in the database.
- Marks emails as seen in IMAP. 
 
---

## **IImapService**
### **Interface Definition**
```csharp
public interface IImapService
```
Defines a contract for IMAP-based email retrieval and processing.

### **Methods**
#### **GetNewMessage(string folder = "Inbox")**
```csharp
Task<IEnumerable<MimeMessage>> GetNewMessage(string folder = "Inbox");
```
- Retrieves new messages from a specified folder.

#### **GetReferences(MimeMessage main)**
```csharp
Task<IEnumerable<MimeMessage>> GetReferences(MimeMessage main);
```
- Fetches referenced messages linked to the given email.

#### **MarkAsSeen(MimeMessage message)**
```csharp
Task MarkAsSeen(MimeMessage message);
```
- Marks an email as read.

---

## **ImapService Implementation**
### **Class Definition**
```csharp
public class ImapService : IImapService
```
Provides an implementation for the IMAP service using `MailKit` to interact with email servers.

### **Constructor**
```csharp
public ImapService(IConfiguration configuration)
```
- `configuration`: Retrieves email server settings.

### **Methods**
#### **GetNewMessage(string folder = "Inbox")**
```csharp
public async Task<IEnumerable<MimeMessage>> GetNewMessage(string folder = "Inbox")
```
- Connects to IMAP.
- Searches for unread messages.
- Retrieves new messages.

#### **GetReferences(MimeMessage main)**
```csharp
public async Task<IEnumerable<MimeMessage>> GetReferences(MimeMessage main)
```
- Searches for emails that reference the given message (i.e., email threads).

#### **MarkAsSeen(MimeMessage message)**
```csharp
public async Task MarkAsSeen(MimeMessage message)
```
- Marks a message as read using IMAP flags.

---

## **Dependency Injection Extensions**
### **AddEmailFailureService<TEmailFailureService>()**
```csharp
public static IServiceCollection AddEmailFailureService<TEmailFailureService>(this IServiceCollection services)
```
- Registers an email failure handling service.

### **AddImapService()**
```csharp
public static IServiceCollection AddImapService(this IServiceCollection services)
```
- Registers the `ImapService`.

---

## **Usage Example**
```csharp 
services.AddEmailFailureService<CustomEmailFailureService>();
services.AddImapService();

var provider = Context.Current;
var emailFailureService = provider.GetRequiredService<IEmailFailureService>();
await emailFailureService.Check();
```
- Registers and initializes the email failure service.
- Calls `Check()` to process new emails.

---

## **Conclusion**
This system efficiently processes failure emails and interacts with an IMAP server to retrieve and mark emails. The modular design ensures that services can be extended and customized for different email processing needs.

