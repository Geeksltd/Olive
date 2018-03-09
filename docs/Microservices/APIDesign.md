# API Design guideline

Software can act in two ways to provide it's service:

- The software can provide services to a user and interacts directly to a human mostly via a UI.
- The software can provide services to another software/service and interact in a way and with a standard that we call it API.

 APIs are the language that software talks to another software. So the API design is the UX for your software because some other people (Developers) are going to use it in their service.
 Here I'm going to get you some practices about REST API design that you should be aware of. The fun fact here is that **THERE IS NO SILVER BULLET**. So follow the good practices, avoid bad practices and be flexible when it makes sense.
 Here tou'll learn about some practices that you should be careful of.

## Request and URLs design

### Resources

In each REST API, resources are identified with the URL. If an API user wants something, they should be specified in the URL such as identifications, resource groups, etc.

**Don't use verbs**
these are some bad practices that you should avoid:
`GET /GetAllEmployees`
`POST /CreateUser`

**Use nouns**
`GET /Employees/` => Gets all employees. It can have paging.
`GET /Employees/22`
`POST /Employees/` => Creates a new employee
Notice:  Use plural nouns when you have a resource which can be a resource group.

**Use handful of HTTP verbs**
HTTP verbs are not CRUD directly, but you can use it in this way

- GET => Read
- POST => Create
- PUT => Update (Mostly major update to a resource), Create (Sometimes)
- PATCH => Update (Partial or specific update to a resource)
- DELETE => Delete

ex:
`GET /Users/` => Gets all users. It can have paging.
`GET /Users/Mohsens22` => Gets user Mohsens22.
`POST /Users/` => Creates a new user.
`PUT /Users/Mohsens22` , `PATCH /Users/Mohsens22` => Updates user Mohsens22's information.
`DELETE /Users/Mohsens22` => Deletes user Mohsens22.

**Note:** Don't manipulate data with GET.
**Another note:** Don't use HTTP verbs that no one is familiar with. The verbs mentioned above are enough in almost all cases.

**Endpoints**
There are some practices about endpoints that most of the most API developers follow:

- api.mydomain.com/resource or api.mydomain.com/v1/resource
- mydomain.com/api/resource or mydomain.com/api/v1/resource
- mydomain.com/resource or mydomain.com/v1/resource

As we use microservice architecture and any microservice can include UI as well as API, we recommend the second practice. So use mydomain.com/api/v1/resource.

### Versioning

There are some solutions for API versioning, but we prefer this one:
`/api/v1/resource`
Why? Because it's self descriptive and easy to understand and maintain.
## Response design

### Return easy to understand objects

I saw some developers that they love to take it hard. They return complex objects in their APIs that if you see them, you will be confused. Also sometimes they don't name their object's properties well. So try to send an easy to understand object with self descriptive property names.

#### Bad practice

```json
{
  "cn":"Geeks",
  "t1":"M#",
  "st":"2017-08-26T19:43:46.2438573+04:30"
}
```

#### Good practice

```json
{
  "companyName":"Geeks",
  "title":"M#",
  "start":"2017-08-26T19:43:46.2438573+04:30"
}
```

**Note:** Don't respond too much data in body. Always ask yourself these tow questions: Do I need them all? Are they what *REALLY* consumers need?
**Note:** Make your requests as less as possible.
**Note:** In this example we see a DateTime. It's prefered to send and get datetimes as UTC. It's a bad practice to work with local time (Some developers do but avoid that).
**Another note:**: It's good to use camelCase names for json objects. But Newtonsoft.Json does it for default.

### Use handful of HTTP status codes

After API user makes a request, you should tell him/her what happened with the request. Was is OK? Or there was an issue with the request. HTTP status codes are the most handful and also standard way of saying "What happened with the request". I saw some developers (including me times ago) who send 200 OK status code with error message inside body! It's a bad practice, never ever even think of it. But it's OK to add some more details and ways to fix or report the issue in the body.
Most common and useful HTTP status codes are:

- 200 OK
- 201 Created
- 400 Bad request
- 404 Not found
- 401 Not authorized (We don't know you)
- 403 Forbidden (We know you and you don't have enough permission to access this resource)

**Note:** Don't use HTTP codes that no one is familiar with. The codes mentioned above are enough in most of the cases. But if there is another code that you think it is a good fit for your situation, go ahead and use it (Don't forget documentation).

### Errors

Errors happen! But the point is that you should give enough information to API user, so he/she should understand what to do next. It's really important to send a good piece of information about the error and the more important part is documentation. Document any expected and unexpected error, provide enough information about them and help API users to fix their issues. Also let them create issues in GitHub (or any other issue tracker) and do QAs about your API's problems (sometimes they contribute and help you to fix it) and make it publicly and easily accessible; I recommend GitHub Readmes.
Also on the server side try to log data so if an unexpected error is reported by a user, you can fix it sooner.

## Documentation

Bad API with a good documentation is better than a good API with bad (or with no) documentation. API lives and dies with documentation. As we are developers and we are a little bit lazy to document, I always try to find an easy way. Here we use swagger for API docs that is accessible via `/swagger/` resource. If you want to know more about swagger [HERE](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?tabs=visual-studio) is a good read for getting started. Also if there was anything more for documentation, creating README.MD is a good choice. Readmes are a good choice for error documentation and any other notes that swagger doesn't support it by default. Swagger is fully suported in Olive/M#.
To use swagger, you should add some information as XML comments above each controller/action.

### Required information

We took it easy at the start. The only required information is **Summary**. But you can add more information when it makes sense.

```xml
/// <summary>
/// Deletes a specific TodoItem.
/// </summary>
```

**Note:** Make your API users notified about API changes, version updates and other notices.
**Another note:** Don't change API endpoints and resource URLs as much as you can. It confuses API users. Breaking changes are always bad for an API that is used in other services.

## Other notes

### KISS (Keep It Stupidly Simple)

Keep your API as simple as possible. In API design, you most of the times should forget about creativity because you might create something completely new (it can be a really nice tool) but no developer if familiar with that or it might take time for the API user to get close to that. So take care of the things that most of the developers are familiar with.
Provide what is necessary. Nothing less, Nothing more.

### Security

- Use SSL for the endpoint. Better to force HTTPS.
- Don't use your own crypto; PLEASE.
- Use the authentication method that is recommended by Olive.

### Never trust any input. VALIDATE VALIDATE VALIDATE

Anyone can send you anything to your API. Some of them can be harmful or trouble making. It can be a bad request to access to an unauthorized resource or sending incomplete and invalid request. Validate the data that API user is sending to you.