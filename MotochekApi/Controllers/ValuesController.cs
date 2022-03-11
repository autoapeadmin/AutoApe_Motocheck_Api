using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MotochekApi.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        //api/values?rego=gkd979$fn=
        public HttpResponseMessage GetVehiclesDetails(string rego, string fn = "details", string licence = "", string firstName = "", string lastName = "",string companyName = "")
        {
            switch (fn)
            {
                case "vehicleDetails": //OK
                    CdiClient.CdiServices.Vehicle.ArrayOfInquireVehicleDetailsResponseResponseDetailResponseDetail objVehicle = CdiClient.CdiClientExamples.Registration.ConfirmCurrentRegisteredPerson.getVehicleDetails(rego);
                    return Request.CreateResponse(HttpStatusCode.OK, objVehicle, Configuration.Formatters.JsonFormatter);
                case "currentDetails": //OK
                    CdiClient.CdiServices.Registration.ArrayOfInquireCurrentDetailsResponseResponseDetailResponseDetail objVehicle7 = CdiClient.CdiClientExamples.Registration.ConfirmCurrentRegisteredPerson.getCurrentDetailsVehicle(rego);
                    return Request.CreateResponse(HttpStatusCode.OK, objVehicle7, Configuration.Formatters.JsonFormatter);
                case "police": //OK
                    CdiClient.CdiServices.Vehicle.ArrayOfInquireStolenVehicleResponseResponseDetailResponseDetail objVehicle2 = CdiClient.CdiClientExamples.Registration.ConfirmCurrentRegisteredPerson.getVehicleStolen(rego);
                    return Request.CreateResponse(HttpStatusCode.OK, objVehicle2, Configuration.Formatters.JsonFormatter);
                case "free": //OK
                    CdiClient.CdiServices.Vehicle.ArrayOfInquireVehicleUsageHistoryResponseResponseDetailResponseDetail objVehicle3 = CdiClient.CdiClientExamples.Registration.ConfirmCurrentRegisteredPerson.getCurrentDetails(rego);
                    return Request.CreateResponse(HttpStatusCode.OK, objVehicle3.Vehicle, Configuration.Formatters.JsonFormatter);
                case "ruc": //OK
                    CdiClient.CdiServices.RucLicence.ArrayOfInquireLatestRucLicenceDetailsResponseResponseDetailResponseDetail objVehicle5 = CdiClient.CdiClientExamples.Registration.ConfirmCurrentRegisteredPerson.getRUC(rego);
                    return Request.CreateResponse(HttpStatusCode.OK, objVehicle5.Item, Configuration.Formatters.JsonFormatter);
                case "owner":
                    CdiClient.CdiServices.Registration.ArrayOfConfirmCurrentRegisteredPersonResponseResponseDetailResponseDetail objVehicle6 = CdiClient.CdiClientExamples.Registration.ConfirmCurrentRegisteredPerson.getCurrentOwner(rego, companyName, licence, firstName, lastName);
                    return Request.CreateResponse(HttpStatusCode.OK, objVehicle6, Configuration.Formatters.JsonFormatter);
                case "ownerHistory":
                    CdiClient.CdiServices.Registration.ArrayOfInquireRegisteredPersonHistoryResponseResponseDetailResponseDetail objVehicle8 = CdiClient.CdiClientExamples.Registration.ConfirmCurrentRegisteredPerson.getOwnerHistory(rego);
                    return Request.CreateResponse(HttpStatusCode.OK, objVehicle8, Configuration.Formatters.JsonFormatter);
                default:
                    CdiClient.CdiServices.Vehicle.ArrayOfInquireVehicleUsageHistoryResponseResponseDetailResponseDetail objVehicle9 = CdiClient.CdiClientExamples.Registration.ConfirmCurrentRegisteredPerson.getCurrentDetails(rego);
                    return Request.CreateResponse(HttpStatusCode.OK, objVehicle9.Vehicle, Configuration.Formatters.JsonFormatter);
            }
            //888MIL
            //CdiServices.Registration.ArrayOfConfirmCurrentRegisteredPersonResponseResponseDetailResponseDetail getCurrentOwner(string rego, string date, string licence, string firstName, string lastName)
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
