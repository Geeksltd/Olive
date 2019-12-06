var errorTemplates;
(function (errorTemplates) {
    errorTemplates.SERVICE = `
<main>
  <div class="error" >
<h2>Oops!</h2>
   <h3>
      Keep calm and try again later!
   </h3>
   <p>
      Something went wrong in the <b>[#SERVICE#]</b> service.
   </p>
   <div class="buttons-row">
      <div class="buttons">
         <a name="ShowMeTheError" class="btn btn-primary" href="[#URL#]" target="_blank" default-button="true">Show me the error</a>
      </div>
   </div>
  </div>
</main>
`;
})(errorTemplates || (errorTemplates = {}));
//# sourceMappingURL=errorTemplates.js.map