# Olive Microservices

Microservice architecture is a method of developing software applications as a **suite of independently deployable**, small, modular services (i.e. applications) in which **each service runs a unique process** and communicates through a well-defined, lightweight mechanism to serve a business goal.

If you're not familiar with the Microservices concept and how it compares with traditional monolith architecture, [this is a good read](https://smartbear.com/learn/api-design/what-are-microservices/) to get you started.

# Overview: Microservices in Olive
There are many ways to implement microservices for business applications. You can of course use Olive to implement your architectural style of choice.

However Olive also facilitates microservice implementation for typical business applications by providing a particular framework and set of components that can speed you up.
This guide will help get you started.

![GitHub Logo](https://github.com/Geeksltd/Olive/blob/master/Services/Olive.Microservices/Microservices.Architecture.png)

# UI Architecture
In general for implementing UI in Microservices architecture you have two choices:
- **Monolith UI:** A single UI application (e.g. SPA) containing all html templates, css, javascript, etc, but consuming API services of many microservice apps.
- **Composite UI:** Each microservice will be responsible for its own UI (html templates, javascript, css) rendering and interactions.

When you A monolith UI approach is more traditional and easier to understand. But will counter many of the benefits of Microservices (such as independent deployability) and is not a solution that we recommend.

## Composite UI
...
