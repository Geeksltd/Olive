# Olive MVC: Ajax page lifecycle
...

## Ajax-based navigation
- Normally when an <A href='...' /> tag is clicked, a full page request will run, which is slow because the whole Javascript, CSS, etc has to load.
- Most page navigations inside an application will share the same Javascript, template (footer, banner, etc) and css.
- It's much more efficient to just replace the MAIN content using ajax.
- This is what Mvc applications do, but only when the A tag has *data-redirect='ajax'*.
- In other words the *data-redirect='ajax'* setting on an A tag is saying "The current page and the arget page have the same template. So only change the <main/> element"

TODO: Explain how it works:
TODO: Read https://github.com/Geeksltd/Olive.MvcJs/blob/master/src/mvc/ajaxRedirect.ts
TODO: Read https://github.com/Geeksltd/Olive.MvcJs/blob/master/src/olivePage.ts#L119
TODO: Explain what happens when BACK is pressed in that case.
TODO: Explain how the server side template supports this (i.e. splitting the Layout into two files, one of which is only <Main> and is returned when the request is Ajax.)

TODO: Dev task: When processing a response which is <MAIN> we are currently replacing the content, but the meta title of the page is being ignored.
This needs to be fixed somehow in the ajax processing logic.
For example we can look for <meta> inside the <main> and apply that on the current page.
