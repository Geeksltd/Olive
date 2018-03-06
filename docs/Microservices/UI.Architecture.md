
# Olive Microservices: UI Architecture

In general for implementing UI in Microservices architecture you have two choices:

- **Monolith UI:** A single UI application (e.g. SPA) containing all html templates, css, javascript, etc, but consuming API services of many microservice apps.
- **Composite UI:** Each microservice will be responsible for its own UI (html templates, javascript, css) rendering and interactions.

When you A monolith UI approach is more traditional and easier to understand. But will negate many of the benefits of Microservices (such as independent deployability) and is not a solution that we recommend. 

## Composite UI
In almost every business application, you can think of the overall page UI structure as consisting of three concerns:

**Overall layout microservice: Access Hub**
This is effectively the host or container for all pages of the application. It provides the overall layout including banner, logo, footer, and other common elements. Also it provides the overall navigation such as main menu, sub menu level 1, ...

It also provides a placeholder where the UI fragment for the actual features (unique pages) will be displayed inside the overall layout, so to the end user everything seems integrated and consistent.

In Olive this is implemented through a single application named **AccessHub**.

**Business Feature microservices**
The actual business functionality is delivered to the user as UI fragments. Each one is usually an MVC page with just the unique content for that page such as form fields, list modules, ... but not the overall shared layout. Since it is hosted inside an IFrame in AccessHub, such pages use a blank layout themselves.

There will usually be many such microservices in the solution each with one or a few pages related to that microservice.

![Overall structure](https://i.imgur.com/EqqTjDy.jpg)

**Shared Theme (CSS and Javascript**
Although the feature microservices are simple pages in a blank template, but they still need to look consistent, use the same fonts, colours, etc. Also many standard Olive javascript features are required by them. To avoid duplication, a single ***Theme*** microservice will provide the shared CSS and Javascript that is referenced by all other microservice pages.

> To the end user, the starting point is Access Hub home page. From there he can click on each menu item to get to the final destination page. The fact that the final page, which provide the desired functionality, is coming from another application (microservice) is seemless and irrelevant.
