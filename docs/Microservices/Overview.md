# Olive Microservices

> **Important** If you're new to Microservices and Docker, [read this book first](https://www.microsoft.com/net/download/thank-you/microservices-architecture-ebook).

Microservice architecture is a method of developing software applications as a **suite of independently deployable**, small, modular services (i.e. applications) in which **each service runs a unique process** and communicates through a well-defined, lightweight mechanism to serve a business goal.

Olive facilitates microservice implementation for typical business applications by providing a productive framework and set of components that can speed you up. This guide will help get you started.

## Distributed UI via Access Hub 
To get the most benefit out of the microservices architecture, each service (small web app) should have full autonomy over its UI. Instead of sharing the UI code in a central monolith application, the UI pages and modules specific to each microservice should be a part of its own source code.

Each microservice will be responsible for its own html (razor) templates, custom CSS and javascript, rendering and user interactions. This allows each service development team, to work independently to develop, test and deploy the service, including its UI.

![GitHub Logo](AccessHub.png)
[Edit diagram](https://www.draw.io/?url=https://raw.githubusercontent.com/Geeksltd/Olive/master/docs/Microservices/AccessHub.png)


## Composite UI
In almost every business application, you can think of the overall page UI structure as consisting of three concerns:

### Overall layout microservice: Access Hub
This is effectively the host or container for all pages of the application. It provides the overall layout including banner, logo, footer, and other common elements. Also it provides the overall navigation such as main menu, sub menu level 1, ...

It also provides a placeholder where the UI fragment for the actual features (unique pages) will be displayed inside the overall layout, so to the end user everything seems integrated and consistent.

In Olive this is implemented through a single application named **AccessHub**.

### Business Feature microservices
The actual business functionality is delivered to the user as UI fragments. Each one is usually an MVC page with just the unique content for that page such as form fields, list modules, ... but not the overall shared layout. It can then be hosted inside AccessHub either as a Web Component (fully Ajax based) or an IFrame (for legacy services).

There will usually be many such microservices in the solution, each with one or a few pages related to that microservice.

![Overall structure](https://i.imgur.com/EqqTjDy.jpg)

## Shared Theme (CSS and Javascript)
Although the feature microservices are simple pages in a blank template, but they still need to look consistent, use the same fonts, colours, etc. Also many standard Olive javascript features are required by them. To avoid duplication, a single ***Theme*** microservice will provide the shared CSS and Javascript that is referenced by all other microservice pages.

> To the end user, the starting point is Access Hub home page. From there he can click on each menu item to get to the final destination page. The fact that the final page, which provide the desired functionality, is coming from another application (microservice) is seemless and irrelevant.
