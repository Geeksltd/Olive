namespace Olive.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Olive;
    using Olive.Entities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using Olive.Web;
    using Olive.Mvc;
    using Olive.Entities.Data;
    using System.Threading.Tasks;

    public class BaseDataServerApiController : ApiController
    {
        Permission Permission;

        static HttpRequest HttpRequest => Context.Request;
        Type RequestedType => Permission?.Type;

        public virtual async Task<IActionResult> GetList(string type)
        {
            Permission = Permission.For(type);
            
            if (Permission?.CanRead != true)
                return Utilities.BadRequest("No read permission is granted for " + type);

            if (Permission.AuthorizeReadWhen?.Invoke() == false)
                return Utilities.BadRequest("Failed the 'read authorization' rule in the permission settings on " + RequestedType.FullName);

            try
            {
                var requestedCriteria = HttpRequest.Param("Criteria").Get(x => JsonConvert.DeserializeObject<List<Criterion>>(x)) ?? new List<Criterion>();

                var criteria = (Permission.AppendCriteria ?? new List<ICriterion>()).Concat(requestedCriteria);

                var data = (await Database.Instance.Of(Permission.Type).Where(criteria).GetList()).ToArray();

                if (Permission.PostQueryFilter != null)
                    data = data.Where(item => Permission.PostQueryFilter(item)).ToArray();

                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                Log.Error("Database API failed: " + HttpRequest.ToRawUrl() + HttpRequest.Form.ToStringOrEmpty().WithPrefix(Environment.NewLine + "FORM:"), ex);
                return Utilities.BadRequest(ex.Message);
            }
        }

        public virtual async Task<IActionResult> Get(string type, string id)
        {
            Permission = Permission.For(type);

            if (Permission?.CanRead != true)
                return Utilities.BadRequest("No read permission is granted for " + type);

            if (Permission.AuthorizeReadWhen?.Invoke() == false)
                return Utilities.BadRequest("Failed the 'read authorization' rule in the permission settings on " + type);

            if (id.IsEmpty()) return Utilities.BadRequest("Database.Get requires ID to be specified in the query string.");

            try
            {
                var criteria = (Permission.AppendCriteria ?? new List<ICriterion>()).Concat(new Criterion("ID", id));
                var data = await Database.Instance.Of(Permission.Type).Where(criteria).Top(1).FirstOrDefault();
                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                Log.Error("Database API failed: " + HttpRequest.ToRawUrl() + HttpRequest.Form.ToStringOrEmpty().WithPrefix(Environment.NewLine + "FORM:"), ex);
                return Utilities.BadRequest(ex.Message);
            }
        }

        public virtual async Task<IActionResult> Save(string type)
        {
            Permission = Permission.For(type);

            if (Permission?.CanSave != true) return Utilities.BadRequest("No save permission is granted for " + type);

            var postedData = await HttpRequest.Body.ReadAllText();
            if (postedData.IsEmpty()) return Utilities.BadRequest($"No details are sent to save a record of {type}");

            var actuallyPostedData = JsonConvert.DeserializeObject(postedData) as JObject;

            var postedObject = JsonConvert.DeserializeObject(postedData, RequestedType) as IEntity;

            if (postedObject == null) return Utilities.BadRequest($"No details are sent to save a record of {type}");

            var toSave = ((await Database.Instance.Of(RequestedType).Where(new Criterion("Id", postedObject.GetId())).FirstOrDefault())?.Clone() ?? RequestedType.CreateInstance()) as IEntity;

            // TODO: Check the following comment
            var properties = toSave.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => /*x.Defines<JsonExposedAttribute>() &&*/ x.CanWrite);

            if (toSave != null)
            {
                foreach (var propertyData in actuallyPostedData.Properties())
                {
                    var property = properties.FirstOrDefault(x => x.MatchesRequestedProperty(propertyData.Name));
                    if (property == null) return Utilities.BadRequest($"Could not find any serializable property matching '{propertyData.Name}'");

                    var value = propertyData.Value.ToStringOrEmpty();

                    try { property.SetValue(toSave, value.To(property.PropertyType)); }
                    catch
                    {
                        return Utilities.BadRequest($"The specified value of '{value}' for '{property.Name}' cannot be converted to '{property.PropertyType}'");
                    }
                }
            }

            if (Permission.AuthorizeSaveWhen?.Invoke(toSave) == false)
                return Utilities.BadRequest("Failed the 'save authorization' rule in the permission settings on " + type);

            try
            {
                await Database.Instance.Save(toSave);
                return new OkResult();
            }
            catch (Exception ex)
            {
                Log.Error("Database API failed: " + HttpRequest.ToRawUrl() + HttpRequest.Form.ToStringOrEmpty().WithPrefix(Environment.NewLine + "FORM:"), ex);
                return Utilities.BadRequest(ex.Message);
            }
        }

        public virtual async Task<IActionResult> Delete(string type, string id)
        {
            Permission = Permission.For(type);

            if (Permission?.CanDelete != true) return Utilities.BadRequest("No delete permission is granted for " + type);

            var toDelete = await Database.Instance.Of(RequestedType).Where(new Criterion("Id", id)).FirstOrDefault();

            if (toDelete == null) return new OkResult();

            if (Permission.AuthorizeDeleteWhen?.Invoke(toDelete) == false)
                return Utilities.BadRequest("Failed the 'delete authorization' rule in the permission settings on " + type);

            try
            {
                await Database.Instance.Delete(toDelete);
                return new OkResult();
            }
            catch (Exception ex)
            {
                Log.Error("Database API failed: " + HttpRequest.ToRawUrl() + HttpRequest.Form.ToStringOrEmpty().WithPrefix(Environment.NewLine + "FORM:"), ex);
                return Utilities.BadRequest(ex.Message);
            }
        }

        public virtual async Task<IActionResult> Patch(string type, string id)
        {
            Permission = Permission.For(type);

            if (Permission?.CanSave != true)
                return Utilities.BadRequest("No save permission is granted for " + type);

            var objectToUpdate = await Database.Instance.Of(RequestedType).Where(new Criterion("Id", id)).FirstOrDefault();

            if (objectToUpdate == null)
                return Utilities.BadRequest($"There is no {type} record with the id of '{id}'");

            var changes = new Dictionary<PropertyInfo, object>();
            // TODO: Check the following comment
            var properties = RequestedType.GetProperties(BindingFlags.Instance | BindingFlags.Public);//.Where(x => x.Defines<JsonExposedAttribute>());
            foreach (var key in HttpRequest.Form.Keys)
            {
                var property = properties.FirstOrDefault(x => x.MatchesRequestedProperty(key));
                if (property == null) return Utilities.BadRequest($"Could not find any serializable property matching '{key}'");

                if (property.Name == "ID") return Utilities.BadRequest("ID cannot be updated.");

                var value = HttpRequest.Form[key].ToString();

                try { changes.Add(property, value.To(property.PropertyType)); }
                catch { return Utilities.BadRequest($"The specified value of '{value}' for '{key}' cannot be converted to '{property.PropertyType}'"); }
            }

            if (Permission.AuthorizePatchWhen?.Invoke(objectToUpdate, changes) == false)
                return Utilities.BadRequest("Failed the 'patch authorization' rule in the permission settings on " + type);

            try
            {
                var clone = objectToUpdate.Clone() as IEntity;

                changes.Do(i => i.Key.SetValue(clone, i.Value));

                await Database.Instance.Save(clone);

                return new OkResult();
            }
            catch (Exception ex)
            {
                Log.Error("Database API failed: " + HttpRequest.ToRawUrl() + HttpRequest.Form.ToStringOrEmpty().WithPrefix(Environment.NewLine + "FORM:"), ex);
                return Utilities.BadRequest(ex.Message);
            }
        }
    }
}