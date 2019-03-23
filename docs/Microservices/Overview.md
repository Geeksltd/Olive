# Olive Microservices

Olive facilitates microservice implementation for typical business applications by providing a productive framework and set of components that can speed you up. This guide will help get you started.

## Prerequisites
To successfully develop microservice solutions using M# and Olive you need to know the following:
- **.NET:** ASP.NET Core Mvc,  Web Apis, RESTful design, Task parallel library (*async/await*)
- **Front end:** Bootstrap 4, SASS, Jquery, Typescript, RequireJS
- **Olive:** Olive Entities and Data Access Api
- **[M# in Visual Studio](http://learn.msharp.co.uk/#/Overview/README)**

## Distributed UI via Access Hub 
To get the most benefit out of the microservices architecture, each service (small web app) should have full autonomy over its UI. Instead of sharing the UI code in a central monolith application, the UI pages and modules specific to each microservice should be a part of its own source code.

Each microservice will be responsible for its own html (razor) templates, custom CSS and javascript, rendering and user interactions. This allows each service development team, to work independently to develop, test and deploy the service, including its UI.

![GitHub Logo](AccessHub.png)
[Edit diagram](https://www.draw.io/?url=https://raw.githubusercontent.com/Geeksltd/Olive/master/docs/Microservices/AccessHub.png)


## Composite UI
In almost every business application, you can think of the overall page UI structure as consisting of three concerns:

#### Overall layout microservice: Access Hub
This is effectively the host or container for all pages of the application. It also provides a placeholder where the UI fragment for the actual features (unique pages) will be displayed inside the overall layout, so to the end user everything seems integrated and consistent. In Olive this is implemented through a single application named **AccessHub**.

Access Hub provides a unified UI experience for the end user. To the end user, AccessHub is *"the application"*. It has a main URL that the user will go to, in order to use the solution. Everything else will happen from there.

It provides:

- Overall layout: including banner, logo, footer, and other common elements.
- Overall navigation: including main menu, sub menu level 1, ...
- Url orchestration: Although each microservice is hosted on a different physical URL, but the urls are rewritten as friendly and unified fragments on the main application URL (of AccessHub itself).
- Common theme: Shared css files
- Common scripts: Shared javascript components

#### Business Feature microservices
The actual business functionality is delivered to the user as UI fragments. Each one is usually an MVC page with just the unique content for that page such as form fields, list modules, ... but not the overall shared layout. It can then be hosted inside AccessHub either as a Web Component (fully Ajax based) or an IFrame (for legacy services).

There will usually be many such microservices in the solution, each with one or a few pages related to that microservice.

![Overall structure](https://i.imgur.com/EqqTjDy.jpg)

> To the end user, the starting point is Access Hub home page. From there he can click on each menu item to get to the final destination page. The fact that the final page, which provide the desired functionality, is coming from another application (microservice) is seemless and irrelevant.

## Shared Theme (CSS and Javascript)
Access Hub will come with shared Javascripts, CSS theme and genral layout code, that is indirectly used by all *feature microservices*. This enables the feature microservices to be developed as simple pages in a blank template, without worrying about styles or javascript framework.
