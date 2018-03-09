# Microservices Security: Single-Sign-On

In microservice scenarios, authentication is typically handled centrally as you don't usually want the users to have to log in to each application separately. Single-sign-on is implemented through [Claims-based security](https://en.wikipedia.org/wiki/Claims-based_identity) which is the default identity model in ASP.NET Core. 

![GitHub Logo](Auth.png)

[Edit diagram](https://www.draw.io/?url=https://raw.githubusercontent.com/Geeksltd/Olive/master/docs/Microservices/Auth.png)

## Authentication via AccessHub
In Olive, the AccessHub microservice acts as a security token service (STS). As you can see in the diagram below.

The user will be authenticated to AccessHub either using username/password fields, or via an ID provider such as Google, Facebook, Microsoft, etc. [Learn more about Olive Authentication](https://geeksltd.github.io/Olive/#/Security/Security).

Once the user has been authenticated, her **email** is established and known to AccessHub. At this moment, it needs to identify the user's name, ID (Guid) and roles, from which to create a standard ASP.NET Authentication cookie and send back to the user's browser.

That cookie (i.e. Claims Token), will then be sent by the browser to each microservice, where the target ASP.NET process will decode the cookie and restore the user's ClaimsPrincipal. That enables all standard security features of ASP.NET to work as you'd expect.

## Authorisation via People Service
Once AccessHub has established the user's email, it needs to retrieve the user's information and roles. This will normally come from a database where an admin user can define other system users and their permissions.

In the standard Olive microservice solution template, there is a People service that is responsible for this. That's where you manage the users, roles and other application-specific security data.

#### Flexible data structure
The actual data structure to define people's roles can vary from project to project. For example in a particular application, you may have organisation, departments, ... and then each user's **effective roles** would be calculated from their membership to such business units, followed by some business rules and calculations. In other applications, you may have a simple role dropdown per user.

#### Flat list of roles
Irrespective of your source data structure in the People Service, ultimately you need to generate a flat list of roles as string items, because that's how role-based security works in ASP.NET. 

#### Authorisation API
For AccessHub to obtain the authorisation information from the people service, it calls an Api that's provided by your people service. It will pass the user's email as input parameter and expect the user's ID, Name and Roles (string array) back. It can then get on with the task of creating an Auth cookie.
